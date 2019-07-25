#region using
using Jellequin.Reflection.Emit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#endregion using

namespace MpSoft.AspNetCore.AggregatedServices
{
	public class JellequinAggregatedServiceGenerator:IAggregatedServiceGenerator
	{
		#region static fields and constants
		readonly static Type _objectType = typeof(object);
		readonly static Type _iServiceProviderType = typeof(IServiceProvider);
		readonly static Type _typeType = typeof(Type);
		readonly static Type _boolType = typeof(bool);
		const string _typeNamespace = "JellequinGenerator";
		const string _typeName = "AggregatedService";
		readonly static string _typeFullName = string.Concat(_typeNamespace,".",_typeName);
		#endregion static fields and constants

		#region .ctor
		readonly CreateOptions _createOptions;
		public JellequinAggregatedServiceGenerator(CreateOptions createOptions)
			=> _createOptions=createOptions??new CreateOptions();
		#endregion .ctor

		#region Generate/GenerateAsync
		public async Task<Type> GenerateAsync(Type serviceType,GenerateOptions options)
		{
			AssemblyName asmName;
			if (_createOptions.AssemblyNameProvider==null)
				asmName=CreateAssemblyName(serviceType);
			else
				asmName=_createOptions.AssemblyNameProvider.GetName(serviceType,options);

			IAssemblyRepository asmRepo = _createOptions.AssemblyRepository;
			Stream asmContent = asmRepo==null ? null : await asmRepo.GetAsync(asmName);

			Assembly asm;
			if (asmContent==null)
			{
				AssemblyBuilder asmBuilder = GenerateInternal(asmName,serviceType,options);
				using (MemoryStream dll = new MemoryStream())
				{
					await asmBuilder.SaveAsync(dll,new SaveOptions());
					if (asmRepo!=null)
					{
						asmContent = await asmRepo.CreateAsync(asmName);
						try
						{
							dll.Position=0;
							await dll.CopyToAsync(asmContent);
						}
						finally
						{
							asmRepo.DisposeStream(asmContent);
						}
					}
					asm=Assembly.Load(dll.GetBuffer());
				}
			}
			else
			{
				try
				{
					using (MemoryStream dll = new MemoryStream())
					{
						await asmContent.CopyToAsync(dll);
						asm=Assembly.Load(dll.GetBuffer());
					}
				}
				finally
				{
					asmRepo.DisposeStream(asmContent);
				}
			}
			return asm.GetType(_typeFullName);
		}

		public Type Generate(Type serviceType,GenerateOptions options)
		{
			AssemblyName asmName;
			if (_createOptions.AssemblyNameProvider==null)
				asmName=CreateAssemblyName(serviceType);
			else
				asmName=_createOptions.AssemblyNameProvider.GetName(serviceType,options);

			IAssemblyRepository asmRepo = _createOptions.AssemblyRepository;
			Stream asmContent = asmRepo?.Get(asmName);

			Assembly asm;
			if (asmContent==null)
			{
				AssemblyBuilder asmBuilder = GenerateInternal(asmName,serviceType,options);
				using (MemoryStream dll = new MemoryStream())
				{
					asmBuilder.Save(dll,new SaveOptions());
					if (asmRepo!=null)
					{
						asmContent=asmRepo.Create(asmName);
						try
						{
							dll.Position=0;
							dll.CopyTo(asmContent);
						}
						finally
						{
							asmRepo.DisposeStream(asmContent);
						}
					}
					asm=Assembly.Load(dll.GetBuffer());
				}
			}
			else
			{
				try
				{
					using (MemoryStream dll = new MemoryStream())
					{
						asmContent.CopyTo(dll);
						asm=Assembly.Load(dll.GetBuffer());
					}
				}
				finally
				{
					asmRepo.DisposeStream(asmContent);
				}
			}
			return asm.GetType(_typeFullName);
		}
		#endregion Generate/GenerateAsync

		#region generate main functionality
		static AssemblyBuilder GenerateInternal(AssemblyName assemblyName,Type serviceType,GenerateOptions options)
		{
			ValidateServiceType(serviceType);

			Type[] interfaceTypes = serviceType.GetInterfaces(); new[] { serviceType }.Concat(serviceType.GetInterfaces()).ToArray();
			ArrayAdd(ref interfaceTypes,serviceType);
			ValidateInterfaces(interfaceTypes);

			(Type DeclaringType, (string Name, Type PropertyType)[] Props)[] props1 = GetPropertiesAndValidateMembers(interfaceTypes).GroupBy(x => x.DeclaringType).Select(x => x.First()).ToArray();

			//Has to resolve duplicities. First version simply decline those.
			(Type DeclaringType, string Name, Type PropertyType)[] props2 = props1.SelectMany(x => x.Props.Select(y => (x.DeclaringType, y.Name, y.PropertyType))).ToArray();
			if (props2.GroupBy(x => x.Name).Select(x => x.Take(2).Count()).Any(x => x!=1))
				throw new JellequinAggregatedServiceGeneratorException(JellequinAggregatedServiceGeneratorExceptionReason.PropertiesMustBeUnique);

			//declare new type with generic parameters.
			ModuleBuilder moduleBuilder = new ModuleBuilder(assemblyName);

			TypeBuilder typeBuilder = moduleBuilder.DefineType(_typeName,_typeNamespace,TypeAttributes.Class|TypeAttributes.Public|TypeAttributes.Sealed,_objectType,interfaceTypes);

			//define constructor. It's body will be defined later.
			ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(".ctor",MethodAttributes.Public|MethodAttributes.SpecialName|MethodAttributes.RTSpecialName,CallingConventions.Standard,new Type[] { _iServiceProviderType });
			constructorBuilder.DefineParameter(1,ParameterAttributes.None,"sp");
			//define properties and getters. Their bodies will be defined later.
			(Type DeclaringType, string Name, Type PropertyType, MethodBuilder MethodBuilder)[] propsWithGetter
				= props2.Select(x => (x.DeclaringType, x.Name, x.PropertyType, CreatePropertyAndGetter(typeBuilder,x.Name,x.PropertyType))).ToArray();

			//get service resolver method.
			MethodInfo serviceProviderGetMethod = options.ServicesRequired
				? typeof(ServiceProviderServiceExtensions).GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService),BindingFlags.Public|BindingFlags.Static,null,new Type[] { _iServiceProviderType,_typeType },null)
				: _iServiceProviderType.GetMethod(nameof(IServiceProvider.GetService),BindingFlags.Public|BindingFlags.Instance,null,new Type[] { _typeType },null);

			//generate main class parts based on ResolveMode.
			switch (options.ResolveMode)
			{
				case ResolveMode.OnCreate:
					GenerateModeOnCreate(typeBuilder,propsWithGetter,constructorBuilder,serviceProviderGetMethod);
					break;
				case ResolveMode.OnDemand:
					GenerateModeOnDemand(typeBuilder,propsWithGetter,constructorBuilder,serviceProviderGetMethod);
					break;
				case ResolveMode.OnDemandCached:
					GenerateModeOnDemandCached(typeBuilder,propsWithGetter,constructorBuilder,serviceProviderGetMethod);
					break;
				default:
					throw new JellequinAggregatedServiceGeneratorException(JellequinAggregatedServiceGeneratorExceptionReason.InvalidOptions);
			}

			return new AssemblyBuilder(moduleBuilder,default);
		}

		static MethodBuilder CreatePropertyAndGetter(TypeBuilder typeBuilder,string name,Type propertyType)
		{
			MethodBuilder methodBuilder=typeBuilder.DefineMethod("get_"+name,MethodAttributes.Public|MethodAttributes.Final|MethodAttributes.HideBySig|MethodAttributes.SpecialName|MethodAttributes.NewSlot|MethodAttributes.Virtual,CallingConventions.Standard);
			methodBuilder.SetParameters(Type.EmptyTypes);
			methodBuilder.SetReturnType(propertyType);

			PropertyBuilder propertyBuilder=typeBuilder.DefineProperty(name,PropertyAttributes.None,propertyType,Type.EmptyTypes);
			propertyBuilder.SetGetMethod(methodBuilder);

			return methodBuilder;
		}
		#endregion generate main functionality

		#region mode generators
		static void GenerateModeOnCreate(TypeBuilder typeBuilder,(Type DeclaringType, string Name, Type PropertyType, MethodBuilder MethodBuilder)[] props,ConstructorBuilder constructorBuilder,MethodInfo serviceProviderGetMethod)
		{
			//create .ctor's body
			//.ctor resolves all properties and stores values to field;
			ILGenerator gen = constructorBuilder.GetILGenerator();

			foreach ((Type declaringType, string name, Type propertyType, MethodBuilder methodBuilder) in props)
			{
				//create field for property
				FieldBuilder fieldBuilder = typeBuilder.DefineField($"<{name}>k__BackingField",propertyType,FieldAttributes.Private|FieldAttributes.InitOnly);

				//continues .ctor part generation - resolve service instance and store to field
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldarg_1);
				gen.Emit(ILOpCode.Ldtoken,propertyType);
				gen.Emit(ILOpCode.Call,_typeType.GetMethod("GetTypeFromHandle"));
				gen.Emit(ILOpCode.Call,serviceProviderGetMethod);
				if (propertyType.IsValueType)
					gen.Emit(ILOpCode.Unbox_any,propertyType);
				gen.Emit(ILOpCode.Stfld,fieldBuilder);

				//create property getter body
				ILGenerator genGetter=methodBuilder.GetILGenerator();
				genGetter.Emit(ILOpCode.Ldarg_0);
				genGetter.Emit(ILOpCode.Ldfld,fieldBuilder);
				genGetter.Emit(ILOpCode.Ret);
			}

			gen.Emit(ILOpCode.Ret);
		}

		static void GenerateModeOnDemand(TypeBuilder typeBuilder,(Type DeclaringType, string Name, Type PropertyType, MethodBuilder MethodBuilder)[] props,ConstructorBuilder constructorBuilder,MethodInfo serviceProviderGetMethod)
		{
			//create field to remember provided IServiceProvider
			FieldBuilder fieldBuilder_ServiceProvider = typeBuilder.DefineField("_sp",_iServiceProviderType,FieldAttributes.Private|FieldAttributes.InitOnly);

			//create .ctor's body
			//.ctor just stores parameter sp to _sp field
			ILGenerator gen=constructorBuilder.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldarg_1);
			gen.Emit(ILOpCode.Stfld,fieldBuilder_ServiceProvider);
			gen.Emit(ILOpCode.Ret);

			//create property getters bodies
			//the bodies just return _sp results - using GetService<>() or GetRequiredService<>()
			foreach ((Type declaringType, string name, Type propertyType, MethodBuilder methodBuilder) in props)
			{
				gen=methodBuilder.GetILGenerator();
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldfld,fieldBuilder_ServiceProvider);
				gen.Emit(ILOpCode.Ldtoken,propertyType);
				gen.Emit(ILOpCode.Call,_typeType.GetMethod("GetTypeFromHandle"));
				gen.Emit(ILOpCode.Call,serviceProviderGetMethod);
				if (propertyType.IsValueType)
					gen.Emit(ILOpCode.Unbox_any,propertyType);
				gen.Emit(ILOpCode.Ret);
			}
		}

		static void GenerateModeOnDemandCached(TypeBuilder typeBuilder,(Type DeclaringType, string Name, Type PropertyType, MethodBuilder MethodBuilder)[] props,ConstructorBuilder constructorBuilder,MethodInfo serviceProviderGetMethod)
		{
			//create field to remember provided IServiceProvider
			FieldBuilder fieldBuilder_ServiceProvider = typeBuilder.DefineField("_sp",_iServiceProviderType,FieldAttributes.Private|FieldAttributes.InitOnly);

			//create .ctor's body
			//.ctor just stores parameter sp to _sp field
			ILGenerator gen = constructorBuilder.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldarg_1);
			gen.Emit(ILOpCode.Stfld,fieldBuilder_ServiceProvider);
			gen.Emit(ILOpCode.Ret);

			//create property getters bodies
			//the bodies just return _sp results - using GetService<>() or GetRequiredService<>()
			foreach ((Type declaringType, string name, Type propertyType, MethodBuilder methodBuilder) in props)
			{
				gen=methodBuilder.GetILGenerator();

				//create field for property
				FieldBuilder fieldBuilder_BackingField = typeBuilder.DefineField($"<{name}>k__BackingField",propertyType,FieldAttributes.Private);
				FieldBuilder fieldBuilder_Resolved = typeBuilder.DefineField($"<{name}>k__Resolved",_boolType,FieldAttributes.Private);

				//build property getter's body
				Label labResolved = gen.DefineLabel();
				Label labEnd = gen.DefineLabel();
				gen.DeclareLocal(propertyType,false);
				//if (fieldBuilder_Resolved), goto labResolved
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldfld,fieldBuilder_Resolved);
				gen.Emit(ILOpCode.Brtrue_s,labResolved);

				//local0 = _sp.GetService(propertyType);
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldfld,fieldBuilder_ServiceProvider);
				gen.Emit(ILOpCode.Ldtoken,propertyType);
				gen.Emit(ILOpCode.Call,_typeType.GetMethod("GetTypeFromHandle"));
				gen.Emit(ILOpCode.Call,serviceProviderGetMethod);
				if (propertyType.IsValueType)
					gen.Emit(ILOpCode.Unbox_any,propertyType);
				gen.Emit(ILOpCode.Stloc_0);

				//fieldBuilder_BackingField = local0
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldloc_0);
				gen.Emit(ILOpCode.Stfld,fieldBuilder_BackingField);

				//fieldBuilder_Resolved = true
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldc_i4_1);
				gen.Emit(ILOpCode.Stfld,fieldBuilder_Resolved);

				//return local0
				gen.Emit(ILOpCode.Ldloc_0);
				gen.Emit(ILOpCode.Br_s,labEnd);

				//else
				gen.MarkLabel(labResolved);
				//return fieldBuilder_BackingField
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldfld,fieldBuilder_BackingField);
				gen.MarkLabel(labEnd);
				gen.Emit(ILOpCode.Ret);
			}
		}
		#endregion mode generators

		#region helpers
		static AssemblyName CreateAssemblyName(Type serviceType)
		{
			StringBuilder sb = new StringBuilder(serviceType.FullName).Append('-').Append(serviceType.Module.ModuleVersionId.ToString("N"));
			for (int a = 0;a<sb.Length;a++)
				if (!char.IsLetterOrDigit(sb[a]))
					sb[a]='-';
			return new AssemblyName(sb.ToString());
		}

		static IEnumerable<(Type DeclaringType, (string Name, Type PropertyType)[] Props)> GetPropertiesAndValidateMembers(IEnumerable<Type> interfaceTypes)
			=> interfaceTypes.Select(sourceType =>
			{
				MemberInfo[] members = sourceType.GetMembers(BindingFlags.Public|BindingFlags.Instance).ToArray();
				(PropertyInfo Property,MethodInfo Getter)[] props = members.OfType<PropertyInfo>().Select(x => (x, x.GetGetMethod())).ToArray();
				if ((props.Any(x=> { PropertyInfo prop = x.Property; return !prop.CanRead|prop.CanWrite|!x.Getter.IsPublic|!IsPublicOrNedtedPublic(prop.PropertyType); }))
					||
					members.Except(props.SelectMany(x => new MemberInfo[] { x.Property,x.Getter })).Any())
					throw new JellequinAggregatedServiceGeneratorException(JellequinAggregatedServiceGeneratorExceptionReason.InvalidMemberType);

				return (sourceType, Array.ConvertAll(props,x => { PropertyInfo prop = x.Property; return (prop.Name, prop.PropertyType); }));
			});

		static void ValidateServiceType(Type serviceType)
		{
			if (serviceType==null)
				throw new ArgumentNullException(nameof(serviceType));
			if (!serviceType.IsInterface)
				throw new JellequinAggregatedServiceGeneratorException(JellequinAggregatedServiceGeneratorExceptionReason.ServiceTypeMustBeInterface);
		}

		static void ValidateInterfaces(Type[] interfaceTypes)
		{
			if (interfaceTypes.Any(x=>!IsPublicOrNedtedPublic(x)))
				throw new JellequinAggregatedServiceGeneratorException(JellequinAggregatedServiceGeneratorExceptionReason.ServiceTypeMustBeInterface);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void ArrayAdd(ref Type[] types,Type toAdd)
		{
			int len = types.Length;
			Array.Resize(ref types,len+1);
			types[len]=toAdd;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)] //Is AggressiveInlining used even when there's recursion?
		static bool IsPublicOrNedtedPublic(Type type)
			=> type.IsPublic||(type.IsNestedPublic&&IsPublicOrNedtedPublic(type.DeclaringType));
		#endregion helpers
	}
}

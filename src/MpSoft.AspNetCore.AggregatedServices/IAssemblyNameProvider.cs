#region using
using System;
using System.Reflection;
#endregion using

namespace MpSoft.AspNetCore.AggregatedServices
{
	public interface IAssemblyNameProvider
	{
		AssemblyName GetName(Type serviceType,GenerateOptions options);
	}
}

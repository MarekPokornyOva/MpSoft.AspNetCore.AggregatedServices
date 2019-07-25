#region using
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
#endregion using

namespace MpSoft.AspNetCore.AggregatedServices
{
	public interface IAssemblyRepository
	{
		Task<Stream> GetAsync(AssemblyName name);
		Stream Get(AssemblyName name);
		Task<Stream> CreateAsync(AssemblyName name);
		Stream Create(AssemblyName name);
		void DisposeStream(Stream stream);
	}
}

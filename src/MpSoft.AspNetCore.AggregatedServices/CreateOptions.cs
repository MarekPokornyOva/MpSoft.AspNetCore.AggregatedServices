namespace MpSoft.AspNetCore.AggregatedServices
{
	public class CreateOptions
	{
		public IAssemblyRepository AssemblyRepository { get; set; }
		public IAssemblyNameProvider AssemblyNameProvider { get; set; }
	}
}

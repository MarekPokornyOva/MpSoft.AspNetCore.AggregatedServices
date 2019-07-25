namespace Web
{
	public interface IMyService1
	{
		string DoSomething();
	}

	public interface IMyService2
	{
		string DoSomethingElse();
	}

	public interface IMyAggregatedService
	{
		IMyService1 Service1 { get; }
		IMyService2 Service2 { get; }
	}

	class MyService1:IMyService1
	{
		public string DoSomething()
			=> "Service is up & running.";
	}

	class MyService2:IMyService2
	{
		public string DoSomethingElse()
			=> "Service is up & running.";
	}
}

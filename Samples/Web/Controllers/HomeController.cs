using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
	public class HomeController:Controller
	{
		IMyAggregatedService _myAggregatedService;
		public HomeController(IMyAggregatedService myAggregatedService)
			=> _myAggregatedService=myAggregatedService;

		public IActionResult Index()
		{
			ViewData["$result1"]="Service #1: "+(_myAggregatedService.Service1?.DoSomething()??"Unavailable. Please uncomment IService1 registration in Startup.");
			ViewData["$result2"]="Service #2: "+(_myAggregatedService.Service2?.DoSomethingElse()??"Unavailable. Please uncomment IService2 registration in Startup.");
			return View();
		}
	}
}

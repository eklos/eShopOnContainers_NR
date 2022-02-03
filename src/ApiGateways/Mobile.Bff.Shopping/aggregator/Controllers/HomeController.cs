namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Controllers;

[Route("")]
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);
 
        return new RedirectResult("~/swagger");
    }
}

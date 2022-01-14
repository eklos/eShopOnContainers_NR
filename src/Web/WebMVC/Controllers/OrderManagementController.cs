namespace WebMVC.Controllers;

[Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
public class OrderManagementController : Controller
{
    private IOrderingService _orderSvc;
    private readonly IIdentityParser<ApplicationUser> _appUserParser;
    public OrderManagementController(IOrderingService orderSvc, IIdentityParser<ApplicationUser> appUserParser)
    {
        _appUserParser = appUserParser;
        _orderSvc = orderSvc;
    }

    public async Task<IActionResult> Index()
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        Log.Information("WebMVC OrderManagementController.Index");

        var user = _appUserParser.Parse(HttpContext.User);
        var vm = await _orderSvc.GetMyOrders(user);

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> OrderProcess(string orderId, string actionCode)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        Log.Information("WebMVC OrderManagementController.OrderProcess - orderId: {0} actionCode: {1}", orderId, actionCode);

        if (OrderProcessAction.Ship.Code == actionCode)
        {
            await _orderSvc.ShipOrder(orderId);
        }

        return RedirectToAction("Index");
    }
}

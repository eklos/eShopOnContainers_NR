namespace Microsoft.eShopOnContainers.WebMVC.Controllers;

using Microsoft.eShopOnContainers.WebMVC.ViewModels;

[Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
public class OrderController : Controller
{
    private IOrderingService _orderSvc;
    private IBasketService _basketSvc;
    private readonly IIdentityParser<ApplicationUser> _appUserParser;
    public OrderController(IOrderingService orderSvc, IBasketService basketSvc, IIdentityParser<ApplicationUser> appUserParser)
    {
        _appUserParser = appUserParser;
        _orderSvc = orderSvc;
        _basketSvc = basketSvc;
    }

    public async Task<IActionResult> Create()
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        Log.Information("WebMVC OrderController.Create");

        var user = _appUserParser.Parse(HttpContext.User);
        var order = await _basketSvc.GetOrderDraft(user.Id);
        var vm = _orderSvc.MapUserInfoIntoOrder(user, order);
        vm.CardExpirationShortFormat();

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(Order model)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        Log.Information("WebMVC OrderController.Checkout");

        try
        {
            if (ModelState.IsValid)
            {
                var user = _appUserParser.Parse(HttpContext.User);
                var basket = _orderSvc.MapOrderToBasket(model);

                await _basketSvc.Checkout(basket);

                //Redirect to historic list.
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("Error", $"It was not possible to create a new order, please try later on ({ex.GetType().Name} - {ex.Message})");
        }

        return View("Create", model);
    }

    public async Task<IActionResult> Cancel(string orderId)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        Log.Information("WebMVC OrderController.Cancel");

        await _orderSvc.CancelOrder(orderId);

        //Redirect to historic list.
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Detail(string orderId)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        Log.Information("WebMVC OrderController.Detail - orderId: {0}", orderId);

        var user = _appUserParser.Parse(HttpContext.User);

        var order = await _orderSvc.GetOrder(user, orderId);
        return View(order);
    }

    public async Task<IActionResult> Index(Order item)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        Log.Information("WebMVC OrderController.Index");

        var user = _appUserParser.Parse(HttpContext.User);
        var vm = await _orderSvc.GetMyOrders(user);
        return View(vm);
    }
}

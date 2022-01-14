namespace Microsoft.eShopOnContainers.WebMVC.Controllers;

[Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
public class CartController : Controller
{
    private readonly IBasketService _basketSvc;
    private readonly ICatalogService _catalogSvc;
    private readonly IIdentityParser<ApplicationUser> _appUserParser;

    public CartController(IBasketService basketSvc, ICatalogService catalogSvc, IIdentityParser<ApplicationUser> appUserParser)
    {
        _basketSvc = basketSvc;
        _catalogSvc = catalogSvc;
        _appUserParser = appUserParser;
    }

    public async Task<IActionResult> Index()
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        try
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var vm = await _basketSvc.GetBasket(user);

            return View(vm);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Index(Dictionary<string, int> quantities, string action)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        Log.Information("WebMVC CartController.Index...");

        try
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var basket = await _basketSvc.SetQuantities(user, quantities);
            if (action == "[ Checkout ]")
            {
                return RedirectToAction("Create", "Order");
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        return View();
    }

    public async Task<IActionResult> AddToCart(CatalogItem productDetails)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        Log.Information("WebMVC CartController.AdddToCart - productDetails: {0}", productDetails.ToString());

        try
        {
            if (productDetails?.Id != null)
            {
                var user = _appUserParser.Parse(HttpContext.User);
                await _basketSvc.AddItemToBasket(user, productDetails.Id);
            }
            return RedirectToAction("Index", "Catalog");
        }
        catch (Exception ex)
        {
            // Catch error when Basket.api is in circuit-opened mode                 
            HandleException(ex);
        }

        return RedirectToAction("Index", "Catalog", new { errorMsg = ViewBag.BasketInoperativeMsg });
    }

    private void HandleException(Exception ex)
    {
        ViewBag.BasketInoperativeMsg = $"Basket Service is inoperative {ex.GetType().Name} - {ex.Message}";
    }
}

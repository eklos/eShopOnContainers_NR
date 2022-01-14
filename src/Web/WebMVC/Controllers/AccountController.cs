namespace Microsoft.eShopOnContainers.WebMVC.Controllers;

[Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public async Task<IActionResult> SignIn(string returnUrl)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        var user = User as ClaimsPrincipal;
        var token = await HttpContext.GetTokenAsync("access_token");

        _logger.LogInformation("----- User {@User} authenticated into {AppName}", user, Program.AppName);

        if (token != null)
        {
            ViewData["access_token"] = token;
        }

        // "Catalog" because UrlHelper doesn't support nameof() for controllers
        // https://github.com/aspnet/Mvc/issues/5853
        return RedirectToAction(nameof(CatalogController.Index), "Catalog");
    }

    public async Task<IActionResult> Signout()
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        _logger.LogInformation("----- User Signout");

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        // "Catalog" because UrlHelper doesn't support nameof() for controllers
        // https://github.com/aspnet/Mvc/issues/5853
        var homeUrl = Url.Action(nameof(CatalogController.Index), "Catalog");
        return new SignOutResult(OpenIdConnectDefaults.AuthenticationScheme,
            new AspNetCore.Authentication.AuthenticationProperties { RedirectUri = homeUrl });
    }
}

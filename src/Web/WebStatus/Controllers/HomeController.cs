namespace WebStatus.Controllers;

public class HomeController : Controller
{
    private IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        var basePath = _configuration["PATH_BASE"];
        return Redirect($"{basePath}/hc-ui");
    }

    [HttpGet("/Config")]
    public IActionResult Config()
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        var configurationValues = _configuration.GetSection("HealthChecksUI:HealthChecks")
            .GetChildren()
            .SelectMany(cs => cs.GetChildren())
            .Union(_configuration.GetSection("HealthChecks-UI:HealthChecks")
            .GetChildren()
            .SelectMany(cs => cs.GetChildren()))
            .ToDictionary(v => v.Path, v => v.Value);

        return View(configurationValues);
    }

    public IActionResult Error()
    {
        return View();
    }
}

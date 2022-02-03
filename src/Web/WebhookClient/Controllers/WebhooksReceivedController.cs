﻿namespace WebhookClient.Controllers;

[ApiController]
[Route("webhook-received")]
public class WebhooksReceivedController : Controller
{

    private readonly Settings _settings;
    private readonly ILogger _logger;
    private readonly IHooksRepository _hooksRepository;

    public WebhooksReceivedController(IOptions<Settings> settings, ILogger<WebhooksReceivedController> logger, IHooksRepository hooksRepository)
    {
        _settings = settings.Value;
        _logger = logger;
        _hooksRepository = hooksRepository;
    }

    [HttpPost]
    public async Task<IActionResult> NewWebhook(WebhookData hook)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);
 
        var header = Request.Headers[HeaderNames.WebHookCheckHeader];
        var token = header.FirstOrDefault();

        _logger.LogInformation("Received hook with token {Token}. My token is {MyToken}. Token validation is set to {ValidateToken}", token, _settings.Token, _settings.ValidateToken);

        if (!_settings.ValidateToken || _settings.Token == token)
        {
            _logger.LogInformation("Received hook is going to be processed");
            var newHook = new WebHookReceived()
            {
                Data = hook.Payload,
                When = hook.When,
                Token = token
            };
            await _hooksRepository.AddNew(newHook);
            _logger.LogInformation("Received hook was processed.");
            return Ok(newHook);
        }

        _logger.LogInformation("Received hook is NOT processed - Bad Request returned.");
        return BadRequest();
    }
}

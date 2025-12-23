namespace Morsley.UK.Mobile;

public class SmsSender : ISmsSender
{
    private readonly IOptionsMonitor<TwilioSettings> _options;
    private readonly ILogger<SmsSender> _logger;

    public SmsSender(IOptionsMonitor<TwilioSettings> options, ILogger<SmsSender> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task SendAsync(string toNumber, string fromNumber, string message)
    {
        var settings = _options.CurrentValue;

        try
        {
            _logger.LogInformation("Sending SMS via Twilio. To={To}; From={From}", toNumber, fromNumber);
            TwilioClient.Init(settings.AccountSid, settings.AuthToken);

            _logger.LogInformation("Twilio client initialised. Creating message...");

            var createTask = MessageResource.CreateAsync(
                to: new PhoneNumber(toNumber),
                from: new PhoneNumber(fromNumber),
                body: message);

            var completed = await Task.WhenAny(createTask, Task.Delay(TimeSpan.FromSeconds(20)));
            if (completed != createTask)
            {
                throw new TimeoutException("Timed out waiting for Twilio MessageResource.CreateAsync");
            }

            var sms = await createTask;
            _logger.LogInformation("Twilio message created. Sid={Sid}; Status={Status}", sms.Sid, sms.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via Twilio");
            throw;
        }
    }
}
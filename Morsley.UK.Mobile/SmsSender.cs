namespace Morsley.UK.Mobile;

public class SmsSender : ISmsSender
{
    private readonly IOptionsMonitor<TwilioSettings> _options;

    public SmsSender(IOptionsMonitor<TwilioSettings> options)
    {
        _options = options;
    }

    public async Task SendAsync(string fromNumber, string toNumber, string message)
    {
        var settings = _options.CurrentValue;

        try
        {
            TwilioClient.Init(settings.AccountSid, settings.AuthToken);

            var sms = await MessageResource.CreateAsync(
                to: new PhoneNumber(toNumber),
                from: new PhoneNumber(fromNumber),
                body: message);
        }
        catch (Exception)
        {
            throw;
        }

    }
}
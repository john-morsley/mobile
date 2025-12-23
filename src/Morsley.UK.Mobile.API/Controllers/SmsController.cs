using Morsley.UK.Mobile.Common.Models;
using Twilio.Security;

namespace Morsley.UK.Mobile.API.Controllers;

[ApiController]
[Route("api/sms")]
public class SmsController(
    ILogger<SmsController> logger,
    ISmsReader smsReader,
    ISmsSender smsSender,
    ISmsPersistenceService smsPersistenceService,
    IOptions<TwilioSettings> twilioOptions) : ControllerBase
{
    private readonly TwilioSettings _twilio = twilioOptions.Value;

    [HttpGet]
    [Route("get-by-id")]
    public IActionResult GetSmsById(string id)
    {
        // ToDo --> Log call

        // ToDo --> Get from DB

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendSms(SendableSmsMessage sendable)
    {
        if (sendable == null) return BadRequest("SMS");
        if (string.IsNullOrEmpty(sendable.To)) return BadRequest("SMS To cannoy be empty");
        if (string.IsNullOrEmpty(sendable.Message)) return BadRequest("SMS Message cannot be empty");

        // ToDo --> Log call
        logger.LogInformation("Sending SMS to: {To}", sendable!.To);
        logger.LogInformation("Sending SMS to: {Message}", sendable!.Message);

        // ToDo --> Send SMS
        await smsSender.SendAsync(
            toNumber: sendable.To,
            fromNumber: _twilio.PrimaryMobileNumber,
            message: sendable.Message);
        logger.LogInformation("smsSender.SendAsync completed");

        // ToDo --> Save to DB
        var sent = sendable.ToSmsMessage();

        await smsPersistenceService.SaveSmsAsync(sent);

        logger.LogInformation("Exiting SendSms");

        return Ok();
    }

    [HttpPost]
    [Route("twilio-callback")]
    // Test using ngrok. See ReadMe.md
    public async Task<IActionResult> TwilioCallback([FromForm] TwilioSmsCallbackRequest callback, CancellationToken cancellationToken)
    {
        if (callback is null) return BadRequest();

        logger.LogInformation(
            "Twilio callback received. MessageSid={MessageSid}; SmsSid={SmsSid}; MessageStatus={MessageStatus}; SmsStatus={SmsStatus}; To={To}; From={From}",
            callback.MessageSid,
            callback.SmsSid,
            callback.MessageStatus,
            callback.SmsStatus,
            callback.To,
            callback.From);

        if (!string.IsNullOrWhiteSpace(callback.Body))
        {
            var inbound = new SmsMessage
            {
                To = callback.To ?? string.Empty,
                From = callback.From ?? string.Empty,
                Message = callback.Body
            };

            await smsPersistenceService.SaveSmsAsync(inbound, cancellationToken);
        }

        return Ok();
    }
}

[BindProperties]
public sealed class TwilioSmsCallbackRequest
{
    [FromForm(Name = "MessageSid")]
    public string? MessageSid { get; set; }

    [FromForm(Name = "MessageStatus")]
    public string? MessageStatus { get; set; }

    [FromForm(Name = "SmsSid")]
    public string? SmsSid { get; set; }

    [FromForm(Name = "SmsStatus")]
    public string? SmsStatus { get; set; }

    [FromForm(Name = "To")]
    public string? To { get; set; }

    [FromForm(Name = "From")]
    public string? From { get; set; }

    [FromForm(Name = "Body")]
    public string? Body { get; set; }
}
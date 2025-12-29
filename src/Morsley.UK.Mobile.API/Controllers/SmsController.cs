namespace Morsley.UK.Mobile.API.Controllers;

[ApiController]
[Route("api/sms")]
public class SmsController(
    ILogger<SmsController> logger,
    ISmsReader smsReader,
    ISmsSender smsSender,
    IReceivedSmsPersistenceService receivedPersistenceService,
    ISentSmsPersistenceService sentPersistenceService,
    IOptions<TwilioSettings> twilioOptions) : ControllerBase
{
    private readonly TwilioSettings _twilio = twilioOptions.Value;

    [HttpGet]
    [Route("received/id")]
    public async Task<IActionResult> GetReceivedByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("SMS ID cannot be null or empty");
        }

        logger.LogInformation("Getting SMS with ID: {SmsId}", id);

        try
        {
            var sms = await receivedPersistenceService.GetByIdAsync(id);

            if (sms == null)
            {
                return NotFound($"SMS with ID {id} not found");
            }

            return Ok(sms);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving sms with ID: {SmsId}", id);
            return StatusCode(500, "An error occurred while retrieving the sms");
        }
    }

    [HttpGet]
    [Route("sent/id")]
    public async Task<IActionResult> GetSentByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("SMS ID cannot be null or empty");
        }

        logger.LogInformation("Getting sms with ID: {SmsId}", id);

        try
        {
            var sms = await sentPersistenceService.GetByIdAsync(id);

            if (sms == null)
            {
                return NotFound($"SMS with ID {id} not found");
            }

            return Ok(sms);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving sms with ID: {SmsId}", id);
            return StatusCode(500, "An error occurred while retrieving the sms");
        }
    }

    [HttpGet]
    [Route("received/page")]
    public async Task<IActionResult> GetReceivedPage([FromQuery] PaginationRequest? pagination = null)
    {
        pagination ??= new PaginationRequest();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        logger.LogInformation("Getting SMS messages with pagination - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

        try
        {
            var paginated = await receivedPersistenceService.GetPageAsync(pagination);

            logger.LogInformation(
                "Retrieved {Count} SMS messages (Page {Page}/{TotalPages})",
                paginated.Count, 
                paginated.Page, 
                paginated.TotalPages);

            return Ok(paginated);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving SMS messages");
            return StatusCode(500, "An error occurred while retrieving SMS messages");
        }
    }

    [HttpGet]
    [Route("sent/page")]
    public async Task<IActionResult> GetPage([FromQuery] PaginationRequest? pagination = null)
    {
        pagination ??= new PaginationRequest();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        logger.LogInformation("Getting SMS messages with pagination - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

        try
        {
            var paginated = await sentPersistenceService.GetPageAsync(pagination);

            logger.LogInformation(
                "Retrieved {Count} SMS messages (Page {Page}/{TotalPages})",
                paginated.Count,
                paginated.Page,
                paginated.TotalPages);

            return Ok(paginated);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving SMS messages");
            return StatusCode(500, "An error occurred while retrieving SMS messages");
        }
    }


    [HttpPost]
    [Route("send-message")]
    public async Task<IActionResult> Send(SendableSmsMessage sendable)
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
            fromNumber: _twilio.SecondaryMobileNumber,
            message: sendable.Message);
        logger.LogInformation("smsSender.SendAsync completed");

        // ToDo --> Save to DB
        var sent = sendable.ToSmsMessage();
        sent.From = _twilio.SecondaryMobileNumber;

        await sentPersistenceService.SaveAsync(sent);

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

            await receivedPersistenceService.SaveAsync(inbound, cancellationToken);
        }

        return Ok();
    }
}
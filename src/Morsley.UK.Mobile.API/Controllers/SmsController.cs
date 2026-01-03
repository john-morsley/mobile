namespace Morsley.UK.Mobile.API.Controllers;

[ApiController]
[Route("api/sms")]
public class SmsController(
    ILogger<SmsController> logger,
    ISmsSender smsSender,
    ISmsReceivedPersistenceService receivedPersistenceService,
    ISmsSentPersistenceService sentPersistenceService,
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
    public async Task<IActionResult> GetSentPage([FromQuery] PaginationRequest? pagination = null)
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

    [HttpDelete]
    [Route("received/id")]
    public async Task<IActionResult> DeleteReceivedById(string id)
    {
        logger.LogInformation("Deleting SMS with ID: {SmsId}", id);

        try
        {
            var deleted = await receivedPersistenceService.DeleteByIdAsync(id);

            if (!deleted)
            {
                return NotFound($"SMS with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting SMS with ID: {SmsId}", id);
            return StatusCode(500, "An error occurred while deleting the SMS");
        }
    }

    [HttpDelete]
    [Route("sent/id")]
    public async Task<IActionResult> DeleteSentById(string id)
    {
        logger.LogInformation("Deleting SMS with ID: {SmsId}", id);

        try
        {
            var deleted = await sentPersistenceService.DeleteByIdAsync(id);

            if (!deleted)
            {
                return NotFound($"SMS with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting SMS with ID: {SmsId}", id);
            return StatusCode(500, "An error occurred while deleting the SMS");
        }
    }

    [HttpDelete]
    [Route("received/all")]
    public async Task<IActionResult> DeleteAllReceived()
    {
        logger.LogInformation("Deleting all received SMS");

        try
        {
            await receivedPersistenceService.DeleteAllAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting all recieved SMS");
            return StatusCode(500, "An error occurred while deleting all recieved SMS");
        }
    }

    [HttpDelete]
    [Route("sent/all")]
    public async Task<IActionResult> DeleteAllSent()
    {
        logger.LogInformation("Deleting all sent SMS");

        try
        {
            await sentPersistenceService.DeleteAllAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting all sent SMS");
            return StatusCode(500, "An error occurred while deleting all sent SMS");
        }
    }

    [HttpPost]
    [Route("send")]
    public async Task<IActionResult> Send(SendableSmsMessage sendable)
    {
        if (sendable == null) return BadRequest("SMS");
        if (string.IsNullOrEmpty(sendable.To)) return BadRequest("SMS To cannoy be empty");
        if (string.IsNullOrEmpty(sendable.Message)) return BadRequest("SMS Message cannot be empty");

        logger.LogInformation("Sending SMS to: {To}", sendable!.To);
        logger.LogInformation("Sending SMS to: {Message}", sendable!.Message);

        await smsSender.SendAsync(
            toNumber: sendable.To,
            fromNumber: _twilio.SecondaryMobileNumber,
            message: sendable.Message);
        logger.LogInformation("smsSender.SendAsync completed");

        var sent = sendable.ToSmsMessage();
        sent.From = _twilio.SecondaryMobileNumber;

        await sentPersistenceService.SaveAsync(sent);

        logger.LogInformation("Exiting SendSms");

        return Ok();
    }

    [HttpPost]
    [Route("twilio-callback")]
    // Testable locally using ngrok. See ReadMe.md
    // Twilio Primary Number from https://mobile.morsley.uk/api/sms/twilio-callback
    // Twilio Secondary Number from https://[URL obtained from ngrok]/api/sms/twilio-callback
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
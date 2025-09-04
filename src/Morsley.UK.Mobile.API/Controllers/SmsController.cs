namespace Morsley.UK.Mobile.API.Controllers;

[ApiController]
[Route("api/sms")]
public class SmsController(ILogger<SmsController> logger) : ControllerBase
{
    [HttpGet]
    [Route("get-by-id")]
    public IActionResult Get(string id)
    {
        // ToDo --> Log call

        // ToDo --> Get from DB

        return Ok();
    }

    [HttpPost]
    [Route("twilio-callback")]
    public IActionResult TwilioCallback(
        [FromForm] string MessageSid, 
        [FromForm] string MessageStatus, 
        [FromForm] string To, 
        [FromForm] string From, 
        [FromForm] string Body)
    {
        // ToDo --> Log call

        // ToDo --> Save to DB

        return Ok();
    }
}
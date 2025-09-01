using Microsoft.AspNetCore.Mvc;

namespace Morsley.UK.Mobile.API.Controllers
{
    [ApiController]
    [Route("api/sms")]
    //[Route("[controller]")]
    public class SmsController : ControllerBase
    {
        //private static readonly string[] Summaries = new[]
        //{
        //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        //};

        private readonly ILogger<SmsController> _logger;

        public SmsController(ILogger<SmsController> logger)
        {
            _logger = logger;
        }

        //[HttpGet(Name = "GetWeatherForecast")]
        //public Result Get()
        //{
        //    //return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    //{
        //    //    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //    //    TemperatureC = Random.Shared.Next(-20, 55),
        //    //    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    //})
        //    //.ToArray();
        //    return OkResult();
        //}

        //[HttpGet]
        //[Route("GetSms")]
        //public IActionResult GetSms(string messageSid)
        //{
        //    // Get from DB...

        //    return Ok();
        //}

        [HttpPost]
        [Route("twilio-callback")]
        public IActionResult TwilioCallback(
            [FromForm] string MessageSid, 
            [FromForm] string MessageStatus, 
            [FromForm] string To, 
            [FromForm] string From, 
            [FromForm] string Body)
        {
            // Save to DB...
            return Ok();
        }

        [HttpGet]
        [Route("get-by-id")]
        public IActionResult Get(string id)
        {
            return Ok();
        }
    }
}

namespace Morsley.UK.Mobile.API.Requests;

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

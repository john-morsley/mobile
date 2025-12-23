namespace Morsley.UK.Mobile.API.Extensions;

public static class SmsMappingExtensions
{
    public static SmsMessage ToSmsMessage(this SendableSmsMessage sendable)
    {
        return new SmsMessage
        {
            To = sendable.To,
            Message = sendable.Message
        };
    }
     public static IEnumerable<SmsMessage> ToSmsMessages(this IEnumerable<SendableSmsMessage> sendableMessages)
    {
        return sendableMessages.Select(x => x.ToSmsMessage());
    }
}
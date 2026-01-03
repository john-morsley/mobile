namespace Morsley.UK.Mobile.Persistence.Extensions;

public static class MobileMappingExtensions
{
    public static SmsDocument ToDocument(this SmsMessage message)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        var document = new SmsDocument
        {
            
            To = message.To,
            From = message.From,
            Message = message.Message
        };

        if (!string.IsNullOrEmpty(message.Id)) document.Id = message.Id;

        if (message.CreatedUtc is not null) document.CreatedUtc = (DateTime)message.CreatedUtc;

        return document;
    }

    public static SmsMessage ToSentSmsMessage(this SmsDocument document)
    {
        return new SmsMessage
        {
            Id = document.Id,
            To = document.To,
            From = document.From,
            Message = document.Message,
            CreatedUtc = document.CreatedUtc
        };
    }

    /// <summary>
    /// Converts a collection of EmailDocuments to EmailMessages
    /// </summary>
    public static IEnumerable<SmsMessage> ToSentSmsMessages(this IEnumerable<SmsDocument> documents)
    {
        return documents.Select(x => x.ToSentSmsMessage());
    }
}
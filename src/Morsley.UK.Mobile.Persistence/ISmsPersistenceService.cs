namespace Morsley.UK.Mobile.Persistence;

public interface ISmsPersistenceService
{
    Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<SmsMessage?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    //Task<PaginatedResponse<SmsMessage>> GetPageAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);

    Task<string> SaveAsync(SmsMessage sms, CancellationToken cancellationToken = default);
}
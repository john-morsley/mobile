namespace Morsley.UK.Mobile.Persistence;

public interface ISmsPersistenceService
{
    Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken);

    Task<SmsMessage?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<PaginatedResponse<SmsMessage>> GetPageAsync(PaginationRequest pagination, CancellationToken cancellationToken);

    Task<string> SaveAsync(SmsMessage sms, CancellationToken cancellationToken);

    Task DeleteAllAsync(CancellationToken cancellationToken);
}
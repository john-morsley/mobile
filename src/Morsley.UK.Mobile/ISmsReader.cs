namespace Morsley.UK.Mobile;

public interface ISmsReader
{
    Task ReadAsync(string toNumber, string fromNumber, string message);
}
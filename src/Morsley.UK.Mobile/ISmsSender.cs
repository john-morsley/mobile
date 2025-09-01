namespace Morsley.UK.Mobile;

public interface ISmsSender
{
    Task SendAsync(string toNumber, string fromNumber, string message);
}
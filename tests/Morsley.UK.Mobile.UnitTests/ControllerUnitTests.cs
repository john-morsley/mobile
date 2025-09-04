namespace Morsley.UK.Mobile.UnitTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void SmsTest()
    {
        // Arrange
        var logger = Substitute.For<ILogger<SmsController>>();
        var controller = new Morsley.UK.Mobile.API.Controllers.SmsController(logger);

        var id = Guid.NewGuid().ToString();

        // Act
        var result = controller.Get(id);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<OkResult>();
    }
}
using GreenGenius.Api.Services.Interfaces;
using Moq;

namespace GreenGenius.App.IntegrationTests.Mocks;

public class CurrentUserMock : ICurrentUserService
{
    private readonly Mock<ICurrentUserService>  _currentUserServiceMock = new();

    public void SetCurrentUser(Guid currentUserId)
    {
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(currentUserId);
    }

    public Guid GetCurrentUserId()
    {
        return _currentUserServiceMock.Object.GetCurrentUserId();
    }
}
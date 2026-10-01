using GreenGenius.App.Security;
using Shouldly;

namespace GreenGenius.Api.Tests.Services;

public class CurrentUserServiceTest
{
    private readonly CurrentUserService _service = new();
    
    [Fact]
    public void GetCurrentUserId_ShouldReturnGuid()
    {
        var id = _service.GetCurrentUserId();
        
        id.ShouldNotBe(Guid.Empty);
    }
}

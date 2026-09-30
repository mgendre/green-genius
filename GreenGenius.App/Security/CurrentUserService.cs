using GreenGenius.Common.Data.Security;

namespace GreenGenius.App.Security;

public class CurrentUserService : ICurrentUserService
{
    public Guid GetCurrentUserId()
    {
        return Guid.Parse("1e18f624-02ae-4c14-b941-c7329132a239");
    }
}
using GreenGenius.Common.Domain.Services.Interfaces;

namespace GreenGenius.Common.Domain.Services;

public class CurrentUserService : ICurrentUserService
{
    public Guid GetCurrentUserId()
    {
        return Guid.Parse("1e18f624-02ae-4c14-b941-c7329132a239");
    }
}
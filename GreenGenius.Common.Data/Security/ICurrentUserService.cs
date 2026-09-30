namespace GreenGenius.Common.Data.Security;

public interface ICurrentUserService
{
    public Guid GetCurrentUserId();
}
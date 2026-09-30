namespace GreenGenius.Common.Data.Security;

public interface IHasOwner
{
    public Guid OwnerId { get; }
}
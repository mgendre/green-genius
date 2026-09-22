namespace GreenGenius.Common.Domain.Security;

public interface IHasOwner
{
    public Guid OwnerId { get; }
}
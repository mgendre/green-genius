using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Security;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GreenGenius.Api.Tests.Infra;

internal static class InMemoryDbContextFactory
{
    public static ApplicationDbContext Create(string databaseName, Guid ownerId)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(service => service.GetCurrentUserId()).Returns(ownerId);

        return new ApplicationDbContext(options, currentUser.Object);
    }
}
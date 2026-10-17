using ArchUnitNET.Loader;
using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.App.Extensions;
using GreenGenius.Common.Data;
using GreenGenius.Infra.Hosting.Handlers;

namespace GreenGenius.ArchitectureTests;

public static class Architecture
{
    public static readonly ArchUnitNET.Domain.Architecture Loaded = new ArchLoader()
        .LoadAssemblies(
            typeof(ApplicationDbContext).Assembly,
            typeof(IApiHandler).Assembly,
            typeof(GardenDto).Assembly,
            typeof(StartupExtensions).Assembly)
        .Build();
}
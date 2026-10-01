namespace GreenGenius.App.IntegrationTests.Extensions;

public sealed record ApiResponse<TR>(HttpResponseMessage Response, TR? Value);

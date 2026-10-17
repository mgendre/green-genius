using GreenGenius.Common.Data.Exceptions;
using GreenGenius.Infra.Hosting.Exceptions;
using Microsoft.AspNetCore.Http;
using Moq;
using Shouldly;

namespace GreenGenius.Api.Tests.Exceptions;

public class GlobalExceptionHandlerTest
{
    private readonly DefaultHttpContext _httpContext = new();
    private readonly Mock<IProblemDetailsService> _problemDetailsService = new();

    private GlobalExceptionHandler CreateHandler() => new(_problemDetailsService.Object);

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsNotDataNotFound_ShouldReturnFalseAndWriteNothing()
    {
        var handler = CreateHandler();

        var handled = await handler.TryHandleAsync(_httpContext,
            new InvalidOperationException("Not a persistence error"), CancellationToken.None);

        handled.ShouldBeFalse();
        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        _problemDetailsService.Verify(p => p.TryWriteAsync(It.IsAny<ProblemDetailsContext>()), Times.Never);
    }

    [Fact]
    public async Task TryHandleAsync_WhenDataNotFound_ShouldWriteNotFoundProblem()
    {
        ProblemDetailsContext? written = null;
        _problemDetailsService
            .Setup(p => p.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(context => written = context)
            .ReturnsAsync(true);
        var exception = new DataNotFoundException("Garden 42 not found");
        var handler = CreateHandler();

        var handled = await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        handled.ShouldBeTrue();
        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        written.ShouldNotBeNull();
        written.HttpContext.ShouldBe(_httpContext);
        written.Exception.ShouldBe(exception);
        written.ProblemDetails.Title.ShouldBe("Resource not found");
        written.ProblemDetails.Detail.ShouldBe("Garden 42 not found");
    }

    [Fact]
    public async Task TryHandleAsync_WhenProblemDetailsNotWritten_ShouldReturnFalse()
    {
        _problemDetailsService
            .Setup(p => p.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .ReturnsAsync(false);
        var handler = CreateHandler();

        var handled = await handler.TryHandleAsync(_httpContext, new DataNotFoundException("Bed 7 not found"),
            CancellationToken.None);

        handled.ShouldBeFalse();
    }

    [Fact]
    public async Task TryHandleAsync_WhenProblemDetailsWritten_ShouldReturnTrue()
    {
        _problemDetailsService
            .Setup(p => p.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .ReturnsAsync(true);
        var handler = CreateHandler();

        var handled = await handler.TryHandleAsync(_httpContext, new DataNotFoundException("Plant 3 not found"),
            CancellationToken.None);

        handled.ShouldBeTrue();
    }
}
using SharedInfrastructure.Http;
using SharedKernel.Results;

namespace Simble.UnitTests;

public class DomainErrorExtensionsTests
{
    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Problem, 400)]
    [InlineData(ErrorType.Unauthorized, 401)]
    [InlineData(ErrorType.Forbidden, 403)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Failure, 500)]
    public void ToStatusCode_MapsEveryErrorType(ErrorType type, int expected) =>
        Assert.Equal(expected, type.ToStatusCode());

    [Fact]
    public void ToProblemDetails_ExposesCodeAndDetail()
    {
        var problem = DomainError
            .NotFound("Sample.NotFound", "Sample was not found.")
            .ToProblemDetails();

        Assert.Equal(404, problem.Status);
        Assert.Equal("Sample was not found.", problem.Detail);
        Assert.Equal("Sample.NotFound", problem.Extensions["code"]);
    }

    [Fact]
    public void ToProblemDetails_HonoursExplicitStatusCode()
    {
        var problem = DomainError
            .Problem("RateLimit.Exceeded", "Too many requests.")
            .ToProblemDetails(429);

        Assert.Equal(429, problem.Status);
    }
}

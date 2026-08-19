using SharedKernel.Results;

namespace Simble.UnitTests;

public class ResultTests
{
    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(DomainError.None, result.Error);
    }

    [Fact]
    public void Failure_CarriesError()
    {
        var error = DomainError.NotFound("Sample.NotFound", "missing");

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ProducesSuccess()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_ProducesFailure()
    {
        Result<int> result = DomainError.Conflict("Sample.Conflict", "taken");

        Assert.True(result.IsFailure);
        Assert.Equal("Sample.Conflict", result.Error.Code);
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        Result<int> result = DomainError.Failure("Error", "boom");

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}

using Microsoft.AspNetCore.Mvc;
using SharedInfrastructure.Http;
using SharedKernel.Results;
using Wolverine.Http;

namespace SharedApi;

public static class ApiProblems
{
    public static ProblemDetails None => WolverineContinue.NoProblems;

    public static ProblemDetails From(DomainError error) => error.ToProblemDetails();

    public static ProblemDetails When(bool failed, DomainError error) =>
        failed ? From(error) : None;
}

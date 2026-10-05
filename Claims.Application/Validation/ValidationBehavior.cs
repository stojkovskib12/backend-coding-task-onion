using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Claims.Application.Validation;

/// <summary>Runs all validators registered for a MediatR message before its handler.</summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken);
            failures.AddRange(result.Errors.Where(error => error is not null));
        }

        if (failures.Count > 0) throw new ValidationException(failures);
        return await next();
    }
}

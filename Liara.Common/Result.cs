namespace Liara.Common;

public sealed class Result : IResult
{    
    public List<Exception> Errors { get; private set; } = new List<Exception>();
    public List<ValidationError> ValidationErrors { get; private set; } = new List<ValidationError>();
    public bool AnyErrors => Errors.Any();
    public bool AnyValidationErrors => ValidationErrors.Any();
    public bool AnyErrorsOrValidationFailures { get; set; }

    public static IResult Success()
    {
        return new Result();
    }

    public static IResult Fail(IEnumerable<ValidationError> validationErrors, IEnumerable<Exception> errors)
    {
        var result = new Result();

        result.Errors.AddRange(errors ?? new List<Exception>());
        result.ValidationErrors.AddRange(validationErrors ?? new List<ValidationError>());

        return result;
    }
}
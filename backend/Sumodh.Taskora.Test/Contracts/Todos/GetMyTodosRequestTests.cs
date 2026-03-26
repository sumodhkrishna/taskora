using System.ComponentModel.DataAnnotations;
using Sumodh.Taskora.Api.Contracts.Todos;

namespace Sumodh.Taskora.Test.Contracts.Todos;

public class GetMyTodosRequestTests
{
    [Fact]
    public void Validate_WhenStatusIsInvalid_ReturnsValidationError()
    {
        var request = new GetMyTodosRequest
        {
            Status = "later",
            Priority = 2,
            Page = 1,
            PageSize = 20
        };

        var validationResults = Validate(request);

        Assert.Contains(validationResults, result =>
            result.MemberNames.Contains(nameof(GetMyTodosRequest.Status)) &&
            result.ErrorMessage == "Status must be one of: all, pending, completed.");
    }

    [Fact]
    public void Validate_WhenStatusIsRecognizedWithDifferentCasing_ReturnsNoErrors()
    {
        var request = new GetMyTodosRequest
        {
            Status = " Completed ",
            Priority = 3,
            Page = 1,
            PageSize = 20
        };

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validate_WhenPriorityIsOutsideAllowedRange_ReturnsValidationError()
    {
        var request = new GetMyTodosRequest
        {
            Priority = 9,
            Page = 1,
            PageSize = 20
        };

        var validationResults = Validate(request);

        Assert.Contains(validationResults, result =>
            result.MemberNames.Contains(nameof(GetMyTodosRequest.Priority)));
    }

    [Fact]
    public void Validate_WhenPageIsZero_ReturnsValidationError()
    {
        var request = new GetMyTodosRequest
        {
            Page = 0,
            PageSize = 20
        };

        var validationResults = Validate(request);

        Assert.Contains(validationResults, result =>
            result.MemberNames.Contains(nameof(GetMyTodosRequest.Page)));
    }

    [Fact]
    public void Validate_WhenPageSizeExceedsLimit_ReturnsValidationError()
    {
        var request = new GetMyTodosRequest
        {
            Page = 1,
            PageSize = 101
        };

        var validationResults = Validate(request);

        Assert.Contains(validationResults, result =>
            result.MemberNames.Contains(nameof(GetMyTodosRequest.PageSize)));
    }

    [Fact]
    public void Validate_WhenSearchExceedsMaxLength_ReturnsValidationError()
    {
        var request = new GetMyTodosRequest
        {
            Search = new string('a', 201),
            Page = 1,
            PageSize = 20
        };

        var validationResults = Validate(request);

        Assert.Contains(validationResults, result =>
            result.MemberNames.Contains(nameof(GetMyTodosRequest.Search)));
    }

    private static List<ValidationResult> Validate(GetMyTodosRequest request)
    {
        var validationResults = new List<ValidationResult>();

        Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        return validationResults;
    }
}

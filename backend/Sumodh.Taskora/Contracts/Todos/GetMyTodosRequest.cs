using System.ComponentModel.DataAnnotations;

namespace Sumodh.Taskora.Api.Contracts.Todos
{
    public class GetMyTodosRequest : IValidatableObject
    {
        public string? Status { get; init; }

        [Range(1, 3)]
        public int? Priority { get; init; }

        [MaxLength(200)]
        public string? Search { get; init; }

        public DateTime? DueBefore { get; init; }

        [Range(1, int.MaxValue)]
        public int Page { get; init; } = 1;

        [Range(1, 100)]
        public int PageSize { get; init; } = 20;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Status))
            {
                yield break;
            }

            var normalizedStatus = Status.Trim().ToLowerInvariant();
            if (normalizedStatus is not ("all" or "pending" or "completed"))
            {
                yield return new ValidationResult(
                    "Status must be one of: all, pending, completed.",
                    [nameof(Status)]);
            }
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Sumodh.Taskora.Api.Contracts.Todos
{
    public class CreateTodoRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        [Range(1, 3)]
        public int Priority { get; init; }
        public DateTime? ToBeCompletedByDateUtc { get; init; }
    }
}

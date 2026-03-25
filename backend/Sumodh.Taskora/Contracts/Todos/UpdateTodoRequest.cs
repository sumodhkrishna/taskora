using System.ComponentModel.DataAnnotations;

namespace Sumodh.Taskora.Api.Contracts.Todos
{
    public class UpdateTodoRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; init; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; init; }
        [Range(1, 3)]
        public int Priority { get; init; }
        public DateTime? ToBeCompletedByDateUtc { get; init; }
    }
}

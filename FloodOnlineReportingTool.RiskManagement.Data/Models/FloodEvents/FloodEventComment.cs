using FloodOnlineReportingTool.Contracts.Shared;
namespace FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;

public record FloodEventComment
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    // Values from Contracts.Shared.CommentType
    public CommentType CommentType { get; init; } = CommentType.ImpactComment;

    public string CommentText { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset? RepliedAt { get; init; }
    public string RepliedBy { get; init; } = string.Empty;
    public DateTimeOffset? ReadAt { get; init; }
    public string ReadBy { get; init; } = string.Empty;

    // Foreign key
    public Guid FloodEventId { get; init; }
    public FloodEvent FloodEvent { get; init; }
}

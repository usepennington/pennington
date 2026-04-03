namespace Penn.Pipeline;

public record SocialMetadata(
    string? Description,
    string? ImageUrl,
    string? Type,
    DateTime? PublishedTime,
    string? Author
);

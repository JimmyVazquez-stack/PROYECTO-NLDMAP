namespace NLDMAP.Application.Users.Administration;

public sealed record UserProfileChangeResponse(
    long Id,
    string UserId,
    string ActorUserId,
    string ActorDisplayName,
    DateTimeOffset OccurredAtUtc,
    string PreviousFirstName,
    string PreviousLastName,
    string NewFirstName,
    string NewLastName);

public sealed record UserProfileHistoryResponse(
    UserProfileChangeResponse[] Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

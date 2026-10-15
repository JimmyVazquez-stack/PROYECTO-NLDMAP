namespace NLDMAP.Application.Users.Administration;

public sealed record UserAccountStatusChangeResponse(
    long Id, string UserId, string ActorUserId, string ActorDisplayName,
    DateTimeOffset OccurredAtUtc, bool PreviousIsActive, bool NewIsActive, string Reason);

public sealed record UserAccountStatusHistoryResponse(
    UserAccountStatusChangeResponse[] Items,
    int PageNumber, int PageSize, int TotalCount, int TotalPages);

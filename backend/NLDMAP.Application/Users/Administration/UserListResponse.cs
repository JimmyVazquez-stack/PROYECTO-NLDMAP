namespace NLDMAP.Application.Users.Administration;

public sealed record UserListResponse(
    UserListItemResponse[] Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);


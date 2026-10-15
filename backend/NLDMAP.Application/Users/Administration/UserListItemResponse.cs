namespace NLDMAP.Application.Users.Administration;

public sealed record UserListItemResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string[] Roles,
    string[] RetiredRoles);


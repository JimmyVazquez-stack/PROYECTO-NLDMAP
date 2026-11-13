namespace NLDMAP.Application.Accounts;

public interface IAccountLifecycle
{
    Task AcceptInvitationAsync(string token, string password);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(string email, string token, string password);
}

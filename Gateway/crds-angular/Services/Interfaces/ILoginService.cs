
namespace crds_angular.Services.Interfaces
{
    public interface ILoginService
    {
        bool PasswordResetRequest(string email, bool isMobile);
        bool ResetPassword(string password, string token);
        bool ClearResetToken(string email);
        bool ClearResetToken(int userId); 
        bool VerifyResetToken(string token);
    }
}

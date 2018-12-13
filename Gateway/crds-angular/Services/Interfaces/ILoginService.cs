
namespace crds_angular.Services.Interfaces
{
    public struct OktaMigrationUser
    {
        public string firstName;
        public string lastName;
        public string email;
        public string login;
        public string password;
        public string mpContactId;
    }

    public interface ILoginService
    {
        bool PasswordResetRequest(string email, bool isMobile);
        bool ResetPassword(string password, string token);
        bool ClearResetToken(string email);
        bool ClearResetToken(int userId); 
        bool VerifyResetToken(string token);
        bool IsValidPassword(string token, string password);
        void CreateOrUpdateOktaAccount(OktaMigrationUser oktaMigrationUser);
    }
}

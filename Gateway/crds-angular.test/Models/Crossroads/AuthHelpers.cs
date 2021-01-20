using Crossroads.Web.Auth.Models;

namespace crds_angular.test.Models.Crossroads
{
    class AuthHelpers
    {
        public static AuthDTO fakeAuthDTO(int contactId, int householdId = 23, string emailAddress = "")
        {
            AuthDTO token = new AuthDTO();
            token.UserInfo = new UserInfo();
            token.UserInfo.Mp = new MpUserInfo();
            token.UserInfo.Mp.ContactId = contactId;
            token.UserInfo.Mp.HouseholdId = new int?(householdId);
            token.UserInfo.Mp.Email = emailAddress;

            return token;
        }
    }
}

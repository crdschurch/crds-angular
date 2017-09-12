using System.Collections.Generic;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IUserRepository
    {
        MpUser GetByUserId(string userId);
        MpUser GetByAuthenticationToken(string authToken);
        MpUser GetByUserName(string userName);
        void UpdateUser(Dictionary<string, object> userUpdateValues);
        void UpdateUserRest(Dictionary<string, object> userUpdateValues, int user_id);
        void UpdateUser(MpUser user);
        int GetUserIdByUsername(string username);
        int GetContactIdByUserId(int userId);
        MpUser GetUserByResetToken(string resetToken);
        List<MpRoleDto> GetUserRoles(int userId);
        List<MpRoleDto> GetUserRolesRest(int userId);

        MpUser GetUserByRecordId(int recordId);
    }
}

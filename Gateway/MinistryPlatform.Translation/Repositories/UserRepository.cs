using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MinistryPlatform.Translation.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly int _usersApiLookupPageViewId;
        private readonly int _usersPageId;

        public UserRepository(
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IMinistryPlatformService ministryPlatformService,
            IMinistryPlatformRestRepository ministryPlatformRest,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRest = ministryPlatformRest;
            _usersApiLookupPageViewId = _configurationWrapper.GetConfigIntValue("UsersApiLookupPageView");
            _usersPageId = _configurationWrapper.GetConfigIntValue("Users");
        }

        public MpUser GetByUserId(string userId)
        {
            var searchString = string.Format("\"{0}\",", userId);
            return (GetUser(searchString));
        }

        public MpUser GetUserByRecordId(int recordId)
        {
            var record = _ministryPlatformService.GetRecordDict(_usersPageId, recordId, ApiLogin());

            var user = new MpUser
            {
                CanImpersonate = record["Can_Impersonate"] as bool? ?? false,
                Guid = record.ContainsKey("User_GUID") ? record["User_GUID"].ToString() : null,
                UserId = record["User_Name"] as string,
                UserEmail = record["User_Email"] as string,
                UserRecordId = Int32.Parse(record["User_ID"].ToString())
            };

            return (user);
        }

        public MpUser GetByAuthenticationToken(string authToken)
        {
            var userId = _ministryPlatformService.GetContactInfo(authToken).UserId;

            string columns = "User_ID, User_Name, User_Email, User_GUID, COALESCE(Can_Impersonate, 0) AS Can_Impersonate";
            MpUser user = _ministryPlatformRest.UsingAuthenticationToken(ApiLogin()).Get<MpUser>(userId, columns);

            return user;
        }

        public string HelperApiLogin()
        {
            return ApiLogin();
        }

        public MpUser GetByUserName(string userName, string apiToken=null)
        {
            if (String.IsNullOrEmpty(apiToken))
                apiToken = ApiLogin();
            string userNameClean = userName.Replace("'", "''");
            string searchUser = $"dp_Users.User_Name='{userNameClean}'";
            string columns = "User_Name,User_GUID, ISNULL(Can_Impersonate, 0) AS Can_Impersonate, User_Email,User_ID";
            var users = _ministryPlatformRest.UsingAuthenticationToken(apiToken).Search<MpUser>(searchUser, columns, null, true);
            return users.FirstOrDefault();
        }

        public MpUser GetByContactId(int contactId, string apiToken)
        {
            if (String.IsNullOrEmpty(apiToken))
                apiToken = ApiLogin();
            string searchUser = $"dp_Users.Contact_ID={contactId}";
            string columns = "User_Name,User_GUID, ISNULL(Can_Impersonate, 0) AS Can_Impersonate, User_Email,User_ID";
            var users = _ministryPlatformRest.UsingAuthenticationToken(apiToken).Search<MpUser>(searchUser, columns, null, true);
            return users.FirstOrDefault();
        }

        public MpUser GetUserByResetToken(string resetToken)
        {
            var searchString = string.Format(",,,,,\"{0}\"", resetToken);
            return GetUser(searchString);
        }

        private MpUser GetUser(string searchString)
        {
            var records = _ministryPlatformService.GetPageViewRecords(_usersApiLookupPageViewId, ApiLogin(), searchString);
            if (records == null || !records.Any())
            {
                return (null);
            }

            var record = records.First();
            var user = new MpUser
            {
                CanImpersonate = record["Can_Impersonate"] as bool? ?? false,
                Guid = record.ContainsKey("User_GUID") ? record["User_GUID"].ToString() : null,
                UserId = record["User_Name"] as string,
                UserEmail = record["User_Email"] as string,
                UserRecordId = Int32.Parse(record["dp_RecordID"].ToString())
            };

            return (user);
        }

        public List<MpRoleDto> GetUserRoles(int userId)
        {
            var records = _ministryPlatformService.GetSubpageViewRecords("User_Roles_With_ID", userId, ApiLogin());
            if (records == null || !records.Any())
            {
                return (null);
            }

            return records.Select(record => new MpRoleDto
            {
                Id = record.ToInt("Role_ID"), Name = record.ToString("Role_Name")
            }).ToList();
        }



        public List<MpRoleDto> GetUserRolesRest(int userId, string apiToken=null)
        {
            if (string.IsNullOrEmpty(apiToken))
                apiToken = ApiLogin();
            string searchStr = $"User_ID={userId}";
            string columns = "dp_User_Roles.Role_ID, Role_ID_Table.Role_Name";
            var records = _ministryPlatformRest.UsingAuthenticationToken(apiToken).SearchTable<Dictionary<string,object>>("dp_User_Roles",searchStr, columns);
            if (records == null || !records.Any())
            {
                return (null);
            }

            return records.Select(record => new MpRoleDto
            {
                Id = record.ToInt("Role_ID"),
                Name = record.ToString("Role_Name")
            }).ToList();

        }


        public void UpdateUser(Dictionary<string, object> userUpdateValues)
        {
            MinistryPlatformService.UpdateRecord(Convert.ToInt32(ConfigurationManager.AppSettings["Users"]), userUpdateValues, ApiLogin());
        }

        public void UpdateUserRest(Dictionary<string, object> userUpdateValues)
        {
            _ministryPlatformRest.UsingAuthenticationToken(ApiLogin()).UpdateRecord("dp_Users", -1 , userUpdateValues); //The second parameter is not used, you must include the PK in the dictionary
        }

        public void UpdateUser(MpUser user)
        {
            var userDict = new Dictionary<string, object>()
            {               
                {"User_Name", user.UserId },
                {"Display_Name", user.DisplayName },
                {"User_Email", user.UserEmail },
                {"User_ID", user.UserRecordId },
                {"Can_Impersonate", user.CanImpersonate },
                {"User_GUID", user.Guid }
            };
            UpdateUser(userDict);
        }

        public int GetUserIdByUsername(string email)
        {
            var records = _ministryPlatformService.GetRecordsDict(Convert.ToInt32(ConfigurationManager.AppSettings["Users"]), ApiLogin(), (email));
             if (records.Count > 1)
            {
                // Given "email" may be a substring of the User_Name of the records returned (ex. "tester@gmail.com" and "cool_tester@gmail.com")
                // Filter again to include only exact email matches
                records = records.FindAll(r => r.ToString("User_Name").Equals(email));
            }

            if (records.Count != 1)
            {
                throw new ApplicationException("User email did not return exactly one user record");
            }

            var record = records[0];
            return record.ToInt("dp_RecordID");      
        }

        public int GetContactIdByUserId(int userId)
        {
            var records = _ministryPlatformService.GetPageViewRecords(2194, ApiLogin(), ("\"" + userId + "\",")); //
            if (records.Count != 1)
            {
                throw new Exception("User ID did not return exactly one user record");
            }

            var record = records[0];
            return record.ToInt("Contact ID");
        }

    }
}

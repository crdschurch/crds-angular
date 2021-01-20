using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using RoleDTO = MinistryPlatform.Translation.Models.DTO.MpRoleDto;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Repositories
{
    public class GetMyRecordsRepository : BaseRepository
    {
        public GetMyRecordsRepository(
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            
        }

        public static List<RoleDTO> GetMyRoles(string token)
        {
            var pageId = Convert.ToInt32(ConfigurationManager.AppSettings["MyRoles"]);
            var pageRecords = MinistryPlatformService.GetRecordsDict(pageId, token);
            return pageRecords.Select(record => new RoleDTO
            {
                Id = (int) record["Role_ID"], Name = (string) record["Role_Name"]
            }).ToList();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class SelectionRepository : BaseRepository, ISelectionRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;

        public SelectionRepository(
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IMinistryPlatformService ministryPlatformService,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
        }

        public IList<int> GetSelectionRecordIds(string authToken, int selectionId)
        {
            var result = _ministryPlatformService.GetSelectionsDict(selectionId, authToken);
            if (result == null || !result.Any())
            {
                return (null);
            }

            var selectedIds = result.Where(next => next.ContainsKey("dp_RecordID")).Select(next => next["dp_RecordID"] as int? ?? -1).ToList();
            return (selectedIds);
        }
    }
}

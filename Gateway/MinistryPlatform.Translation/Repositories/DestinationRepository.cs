using System.Collections.Generic;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Repositories
{
    public class DestinationRepository : BaseRepository, IDestinationRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;

        public DestinationRepository(
            IMinistryPlatformService ministryPlatformService,
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
        }

        public List<MpTripDocuments> DocumentsForDestination(int destinationId)
        {
            var token = ApiLogin();
            var searchString = string.Format(",{0}", destinationId);
            var records = _ministryPlatformService.GetPageViewRecords("TripDestinationDocuments", token, searchString);

            var documents = new List<MpTripDocuments>();
            foreach (var record in records)
            {
                var d = new MpTripDocuments();
                d.Description = record.ToString("Description");
                d.Document = record.ToString("Document");
                d.DocumentId = record.ToInt("Document_ID");
                documents.Add(d);
            }

            return documents;
        }
    }
}
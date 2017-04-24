using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using log4net;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Models.Finder;
using System.Device.Location;

namespace MinistryPlatform.Translation.Repositories
{
    public class FinderRepository : BaseRepository, IFinderRepository
    {
        private const int SearchRadius = 6380; 

        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly ILog _logger = LogManager.GetLogger(typeof(CampRepository));

        public FinderRepository(IConfigurationWrapper configuration,
                                IMinistryPlatformRestRepository ministryPlatformRest,
                                IApiUserRepository apiUserRepository,
                                IAuthenticationRepository authenticationService)
            : base(authenticationService, configuration)
        {
            _ministryPlatformRest = ministryPlatformRest;
            _apiUserRepository = apiUserRepository;
        }

        public FinderPinDto GetPinDetails(int participantId)
        {
            var token = _apiUserRepository.GetToken();

            const string pinSearch = "Email_Address, Nickname as FirstName, Last_Name as LastName, Participant_Record_Table.*, Household_ID";
            string filter = $"Participant_Record = {participantId}";

            var myPin = _ministryPlatformRest.UsingAuthenticationToken(token).Search<FinderPinDto>(filter, pinSearch);
            FinderPinDto pinDetails;

            if (myPin != null && myPin.Count > 0)
            {
                pinDetails = myPin.First();
                pinDetails.Address = GetPinAddress(participantId);

            }
            else
            {
                pinDetails = null;
            }

            return pinDetails;
        }

        public MpAddress GetPinAddress(int participantId)
        {
            string filter = $"Participant_Record = {participantId}";
            const string addressSearch = "Household_ID_Table_Address_ID_Table.*";
            return _ministryPlatformRest.UsingAuthenticationToken(_apiUserRepository.GetToken()).Search<MpAddress>(filter, addressSearch)?.First();
        }
        
        public void EnablePin(int participantId)
        {
            var dict = new Dictionary<string, object> { { "Participant_ID", participantId }, { "Show_On_Map", true } };

            var update = new List<Dictionary<string, object>> { dict };

            var apiToken = _apiUserRepository.GetToken();
            _ministryPlatformRest.UsingAuthenticationToken(apiToken).Put("Participants", update);
        }

        public List<SpPinDto> GetPinsInRadius(GeoCoordinate originCoords)
        {
            var apiToken = _apiUserRepository.GetToken();

            var parms = new Dictionary<string, object>()
            {
                {"@Latitude", originCoords.Latitude },
                {"@Longitude", originCoords.Longitude },
                {"@RadiusInKilometers", SearchRadius }
            };

            const string spName = "api_crds_get_Pins_Within_Range"; 

            try
            {
                var storedProcReturn = _ministryPlatformRest.UsingAuthenticationToken(apiToken).GetFromStoredProc<SpPinDto>(spName, parms);
                var pinsFromSp = storedProcReturn.FirstOrDefault();

                return pinsFromSp; 
            }
            catch (Exception)
            {
                return new List<SpPinDto>();
            }
        }

        public List<MpConnectAws> GetAllPinsForAws()
        {
            var apiToken = _apiUserRepository.GetToken();
            const string spName = "api_crds_Get_Connect_AWS_Data";

            try
            {
                var storedProcReturn = _ministryPlatformRest.UsingAuthenticationToken(apiToken).GetFromStoredProc<MpConnectAws>(spName);
                var pinsFromSp = storedProcReturn.FirstOrDefault();

                return pinsFromSp;
            }
            catch (Exception ex)
            {
                _logger.Error("GetAllPinsForAws error" + ex);
                return new List<MpConnectAws>();
            }
        }

        public void RecordConnection(MpConnectCommunication connection)
        {
            var apiToken = _apiUserRepository.GetToken();

            try
            {
                _ministryPlatformRest.UsingAuthenticationToken(apiToken).Create(connection);
            }
            catch (Exception ex)
            {
                _logger.Error("RecordConnection error" + ex);
            }
        }
    }
}

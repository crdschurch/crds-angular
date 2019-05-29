using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Childcare;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Exceptions;

namespace MinistryPlatform.Translation.Repositories
{
    public class CampaignRepository : BaseRepository, ICampaignRepository
    {
        public const string CampaignSummaryProcName = "api_crds_Get_Pledge_Campaign_Summary";

        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;

        public CampaignRepository(
            IMinistryPlatformService ministryPlatformService,
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IMinistryPlatformRestRepository ministryPlatformRest,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRest = ministryPlatformRest;
        }

        public MpPledgeCampaign GetPledgeCampaign(int campaignId)
        {            
            return WithApiLogin<MpPledgeCampaign>(token =>
            {
                // NOTE: This page view excludes trips whose Event.Event_Start_Date >= today
                var results = _ministryPlatformService.GetPageViewRecords(_configurationWrapper.GetConfigIntValue("GoTripsWithForms"), token, campaignId.ToString());
                var campaigns = new List<MpPledgeCampaign>();
                foreach (var result in results)
                {
                    var ageExceptions = _ministryPlatformService.GetSubPageRecords(_configurationWrapper.GetConfigIntValue("GoTripAgeExceptions"), campaignId, token);
                    var exceptions = ageExceptions.Select(ae => ae.ToInt("Contact_ID")).ToList();
                    var campaign = new MpPledgeCampaign()
                    {
                        Id = result.ToInt("Pledge_Campaign_ID"),
                        Name = result.ToString("Campaign_Name"),
                        Type = result.ToString("Campaign_Type"),
                        StartDate = result.ToDate("Start_Date"),
                        EndDate = result.ToDate("End_Date"),
                        Goal = result.ToInt("Campaign_Goal"),
                        FormId = result.ToInt("Form_ID"),
                        Nickname = result.ToString("Nickname"),
                        YoungestAgeAllowed = result.ToInt("Youngest_Age_Allowed"),
                        RegistrationEnd = result.ToDate("Registration_End"),
                        RegistrationStart = result.ToDate("Registration_Start"),
                        RegistrationDeposit = result.ToString("Registration_Deposit"),
                        AgeExceptions = exceptions,
                        EventId = result.ToInt("Event_ID"),
                        ProgramId = result.ToInt("Program_ID")
                    };
                    campaigns.Add(campaign);
                }
                
                return campaigns.FirstOrDefault();
            });
           
        }

        public MpPledgeCampaign GetPledgeCampaign(int campaignId, string token)
        {
            var columnList = new List<string>
            {
                "Pledge_Campaigns.Pledge_Campaign_ID",
                "Pledge_Campaigns.Campaign_Name",
                "Pledge_Campaign_Type_ID_Table.Campaign_Type",
                "Pledge_Campaigns.Start_Date",
                "Pledge_Campaigns.[End_Date]",
                "Pledge_Campaigns.[Campaign_Goal]",
                "Registration_Form_Table.[Form_ID]",
                "Registration_Form_Table.[Form_Title]",
                "Pledge_Campaigns.[Registration_Start]",
                "Pledge_Campaigns.[Registration_End]",
                "Pledge_Campaigns.[Registration_Deposit]",
                "Pledge_Campaigns.[Youngest_Age_Allowed]",
                "Event_ID_Table.[Event_Start_Date]",
                "Pledge_Campaigns.[Nickname]",
                "Event_ID_Table.[Event_ID]",
                "Pledge_Campaigns.[Program_ID]",
                "Pledge_Campaigns.Maximum_Registrants"
            };
            var pledgeCampaigns = _ministryPlatformRest.UsingAuthenticationToken(token).Search<MpPledgeCampaign>($"Pledge_Campaigns.[Pledge_Campaign_ID] = {campaignId}", columnList);
            if (pledgeCampaigns.Count == 0)
            {
                throw new Exception("Pledge Campaign not found");
            }
            var pledgeCampaign = pledgeCampaigns.Single();

            //TODO: update to use new rest API 
            var ageExceptions = _ministryPlatformService.GetSubPageRecords(_configurationWrapper.GetConfigIntValue("GoTripAgeExceptions"), campaignId, token);
            var exceptions = ageExceptions.Select(ae => ae.ToInt("Contact_ID")).ToList();

            pledgeCampaign.AgeExceptions = exceptions;
            return pledgeCampaign;
        }

        public List<MpTripRecord> GetGoTripDetailsByCampaign(int pledgeCampaignId)
        {
            var parms = new Dictionary<string, object> { { "@Pledge_Campaign_ID", pledgeCampaignId } };
            var tripRecords = _ministryPlatformRest.UsingAuthenticationToken(ApiLogin()).GetFromStoredProc<MpTripRecord>(_configurationWrapper.GetConfigValue("TripRecordProc"), parms);
            List<MpTripRecord> tripRecord = tripRecords.FirstOrDefault()?? new List<MpTripRecord>();
            return tripRecord;
        }

        // Retrieve overall summary info for the whole campaign, or throw if the campaign does not exist
        public List<MpPledgeCampaignSummaryDto> GetPledgeCampaignSummary(string token, int pledgeCampaignId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Pledge_Campaign_ID", pledgeCampaignId }
            };

            var storedProcReturn = _ministryPlatformRest.UsingAuthenticationToken(token).GetFromStoredProc<MpPledgeCampaignSummaryDto>(CampaignSummaryProcName, parameters);

            List<MpPledgeCampaignSummaryDto> summary = storedProcReturn.FirstOrDefault() ?? new List<MpPledgeCampaignSummaryDto>();
            if (summary == null || summary.Count == 0)
                throw new PledgeCampaignNotFoundException(pledgeCampaignId);

            return summary;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class PledgeRepository : BaseRepository, IPledgeRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        private readonly int _pledgePageId;
        private readonly int _myHouseholdPledges;

        public PledgeRepository(
            IMinistryPlatformService ministryPlatformService,
            IMinistryPlatformRestRepository ministryPlatformRestRepository,
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;

            _pledgePageId = configurationWrapper.GetConfigIntValue("Pledges");
            _myHouseholdPledges = configurationWrapper.GetConfigIntValue("MyHouseholdPledges");
        }

        public int CreatePledge(int donorId, int pledgeCampaignId, decimal totalPledge)
        {
            var values = new Dictionary<string, object>
            {
                {"Donor_ID", donorId},
                {"Pledge_Campaign_ID", pledgeCampaignId},
                {"Pledge_Status_ID", 1},
                {"Total_Pledge", totalPledge},
                {"Installments_Planned", 0},
                {"Installments_Per_Year", 0},
                {"First_Installment_Date", DateTime.Now}
            };

            int pledgeId;

            try
            {
                pledgeId = WithApiLogin<int>(apiToken => (_ministryPlatformService.CreateRecord(_pledgePageId, values, apiToken, true)));
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("CreatePledge failed.  Donor Id: {0}", donorId), e);
            }
            return pledgeId;
        }

        public bool DonorHasPledge(int pledgeCampaignId, int donorId)
        {
            var searchString = string.Format(",{0},{1}", pledgeCampaignId, donorId);
            var records = _ministryPlatformService.GetPageViewRecords("PledgesByDonorId", ApiLogin(), searchString);
            return records.Count != 0;
        }

        public MpPledge GetPledgeByCampaignAndDonor(int pledgeCampaignId, int donorId)
        {
            var searchString = string.Format(",{0},{1}", pledgeCampaignId, donorId);
            var records = _ministryPlatformService.GetPageViewRecords("PledgesByDonorId", ApiLogin(), searchString);
            switch (records.Count)
            {
                case 1:
                    var record = records.First();
                    var pledge = new MpPledge();
                    pledge.DonorId = record.ToInt("Donor_ID");
                    pledge.PledgeCampaignId = record.ToInt("Pledge_Campaign_ID");
                    pledge.PledgeId = record.ToInt("Pledge_ID");
                    pledge.PledgeStatusId = record.ToInt("Pledge_Status_ID");
                    pledge.PledgeTotal = record["Total_Pledge"] as decimal? ?? 0;
                    pledge.CampaignStartDate = record.ToDate("Start_Date");
                    pledge.CampaignEndDate = record.ToDate("End_Date");
                    return pledge;
                case 0:
                    return null;
                default:
                    throw new ApplicationException(string.Format("GetPledgeByCampaignAndDonor returned multiple records. CampaignId: {0}, DonorId: {1}", pledgeCampaignId, donorId));
            }
        }

        public MpPledge GetPledgeByCampaignAndContact(int pledgeCampaignId, int contactId)
        {
            var authToken = ApiLogin();
            var columnList = new List<string>
            {
                "Pledges.Pledge_ID",
                "Donor_ID_Table.Donor_ID",
                "Pledge_Campaign_ID_Table.Pledge_Campaign_ID",
                "Pledge_Campaign_ID_Table.Campaign_Name",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Pledge_Campaign_Type_ID",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Campaign_Type",
                "Pledge_Campaign_ID_Table.Start_Date",
                "Pledge_Campaign_ID_Table.End_Date",
                "Pledge_Status_ID_Table.Pledge_Status_ID",
                "Pledge_Status_ID_Table.Pledge_Status",
                "Pledges.Total_Pledge"
            };
            return _ministryPlatformRestRepository.UsingAuthenticationToken(authToken).Search<MpPledge>("Donor_ID_Table_Contact_ID_Table.Contact_ID=" + contactId + " AND Pledge_Campaign_ID_Table.Pledge_Campaign_ID=" + pledgeCampaignId + " AND Pledge_Status_ID_Table.Pledge_Status_ID=1", columnList).FirstOrDefault();
        }

        public List<MpPledge> GetPledgesByCampaign(int pledgeCampaignId, string token)
        {
            var columnList = new List<string>
            {
                "Pledges.Pledge_ID",
                "Donor_ID_Table.Donor_ID",
                "Pledge_Campaign_ID_Table.Pledge_Campaign_ID",
                "Pledge_Campaign_ID_Table.Campaign_Name",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Pledge_Campaign_Type_ID",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Campaign_Type",
                "Pledge_Campaign_ID_Table.Start_Date",
                "Pledge_Campaign_ID_Table.End_Date",
                "Pledge_Status_ID_Table.Pledge_Status_ID",
                "Pledge_Status_ID_Table.Pledge_Status",
                "Pledges.Total_Pledge"
            };

            return _ministryPlatformRestRepository.UsingAuthenticationToken(token)
                .Search<MpPledge>("Pledge_Campaign_ID_Table.Pledge_Campaign_ID=" + pledgeCampaignId + " AND Pledge_Status_ID_Table.Pledge_Status_ID=1", columnList);
        }

        public int GetDonorForPledge(int pledgeId)
        {
            var record = _ministryPlatformService.GetRecordDict(_pledgePageId, pledgeId, ApiLogin());
            return record.ToInt("Donor_ID");
        }
        
        public List<MpPledge> GetPledgesForAuthUser(string userToken, int[] pledgeTypeIds = null)
        {
            string search;
            if (pledgeTypeIds != null && pledgeTypeIds.Any())
            {
                search = string.Format(",,,,,,,,,,,\"{0}\"", string.Join("\" or \"", pledgeTypeIds));
            }
            else
            {
                search = string.Empty;
            }

            var records = _ministryPlatformService.GetRecordsDict(_myHouseholdPledges, userToken, search);
            return records.Select(MapRecordToPledge).ToList();
        }

        private MpPledge MapRecordToPledge(Dictionary<string, object> record)
        {
            return new MpPledge()
            {
                PledgeId = record.ToInt("Pledge_ID"),
                PledgeCampaignId = record.ToInt("Pledge_Campaign_ID"),
                DonorId = record.ToInt("Donor_ID"),
                PledgeStatus = record.ToString("Pledge_Status"),
                CampaignName = record.ToString("Campaign_Name"),   
                PledgeTotal = record["Total_Pledge"] as decimal? ?? 0,
                PledgeDonations = record["Donation_Total"] as decimal? ?? 0,
                CampaignStartDate = record.ToDate("Start_Date"),
                CampaignEndDate = record.ToDate("End_Date"),
                CampaignTypeId = record.ToInt("Pledge_Campaign_Type_ID"),
                CampaignTypeName = record.ToString("Campaign_Type")
            };
        }
    }
}
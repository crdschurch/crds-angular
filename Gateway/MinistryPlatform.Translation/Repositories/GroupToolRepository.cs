using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Helpers;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class GroupToolRepository : BaseRepository, IGroupToolRepository
    {
        public const string SearchGroupsProcName = "api_crds_SearchGroups";
        private readonly int _invitationPageId;
        private readonly int _smallGroupTypeId;
        private readonly int _gatheringTypeId;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _mpRestRepository;
        private const int JourneyCategoryID = 51;

        public GroupToolRepository(IMinistryPlatformService ministryPlatformService,
                               IConfigurationWrapper configurationWrapper,
                               IAuthenticationRepository authenticationService,
                               IMinistryPlatformRestRepository mpRestRepository,
                               IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _invitationPageId = _configurationWrapper.GetConfigIntValue("InvitationPageID");
            _smallGroupTypeId = _configurationWrapper.GetConfigIntValue("SmallGroupTypeId");
            _gatheringTypeId = _configurationWrapper.GetConfigIntValue("AnywhereGroupTypeId");
            _mpRestRepository = mpRestRepository;
        }

        public string GetCurrentJourney()
        {
            var filter = $"ATTRIBUTE_CATEGORY_ID_TABLE.ATTRIBUTE_CATEGORY_ID = {JourneyCategoryID} AND GETDATE() BETWEEN START_DATE AND ISNULL(END_DATE, GETDATE())";
            var attribute = _mpRestRepository.UsingAuthenticationToken(ApiLogin()).Search<MpAttribute>(filter, "Attribute_Name").FirstOrDefault();
            return attribute?.Name;
        }

        public List<MpInvitation> GetInvitations(int sourceId, int invitationTypeId)
        {
            var mpInvitations = new List<MpInvitation>();
            try
            {
                var searchString = $",,,\"{invitationTypeId}\",\"{sourceId}\",,false";
                var mpResults = _ministryPlatformService.GetRecords(_invitationPageId, ApiLogin(), searchString, string.Empty);
                var invitations = MPFormatConversion.MPFormatToList(mpResults);

                // Translate object format from MP to an MpInvitaion object
                if (invitations != null && invitations.Count > 0)
                {
                    mpInvitations.AddRange(
                        invitations.Select(
                            p =>
                                new MpInvitation
                                {
                                    SourceId = p.ToInt("Source_ID"),
                                    EmailAddress = p.ToString("Email_address"),
                                    GroupRoleId = p.ToInt("Group_Role_ID"),
                                    InvitationType = p.ToInt("Invitation_Type_ID"),
                                    RecipientName = p.ToString("Recipient_Name"),
                                    RequestDate = p.ToDate("Invitation_Date")
                                }));
                }
                else
                {
                    _logger.Debug($"No pending invitations found for SourceId = {sourceId}, InvitationTypeId = {invitationTypeId} ");
                }
            }
            catch (Exception exception)
            {
                _logger.Debug($"Exception thrown while retrieving invitations for SourceId = {sourceId}, InvitationTypeId = {invitationTypeId} ");
                _logger.Debug($"Exception message:  {exception.Message} ");
            }
            return mpInvitations;
        }

        public void ArchivePendingGroupInquiriesOlderThan90Days()
        {
            try
            {
                const string spName = "api_crds_Archive_Pending_Group_Inquiries_Older_Than_90_Days";
                _mpRestRepository.GetFromStoredProc<bool>(spName);

            }
            catch (Exception e)
            {
                _logger.Error("Failed to execute stored proc to archive groups");
            }
        }

        public List<MpInquiry> GetInquiries(int? groupId = null)
        {
            var mpInquiries = new List<MpInquiry>();
            try
            {
                var filter = $"Group_ID_Table.Group_Type_ID in ({_smallGroupTypeId}, {_gatheringTypeId}) AND Placed is null";

                if (groupId.HasValue)
                {
                    filter += $" AND Group_Inquiries.Group_ID = {groupId.Value}";
                }

               mpInquiries = _mpRestRepository.UsingAuthenticationToken(ApiLogin())
                    .Search<MpInquiry>(filter, "Group_Inquiries.*").ToList();

                if (mpInquiries.Count < 1)
                {
                    _logger.Info("No pending inquires found" + (groupId == null ? string.Empty : $" for GroupId = {groupId}"));
                }
            }
            catch (Exception e)
            {
                _logger.Error("Exception thrown while retrieving inquiries" + (groupId == null ? string.Empty : $" for GroupId = {groupId}"), e);
            }
            return mpInquiries;

        }
    }
}
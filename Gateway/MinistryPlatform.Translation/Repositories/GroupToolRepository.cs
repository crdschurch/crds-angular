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
        private readonly int _groupInquiresSubPageId;
        private readonly int _groupInquiriesNotPlacedPageViewId;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _mpRestRepository;
        private readonly IApiUserRepository _apiUserRepository;
        private const int JourneyCategoryID = 51;

        public GroupToolRepository(IMinistryPlatformService ministryPlatformService,
                               IConfigurationWrapper configurationWrapper,
                               IAuthenticationRepository authenticationService,
                               IMinistryPlatformRestRepository mpRestRepository,
                               IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper)
        {
            _ministryPlatformService = ministryPlatformService;
            _invitationPageId = _configurationWrapper.GetConfigIntValue("InvitationPageID");
            _groupInquiresSubPageId = _configurationWrapper.GetConfigIntValue("GroupInquiresSubPage");
            _groupInquiriesNotPlacedPageViewId = _configurationWrapper.GetConfigIntValue("GroupInquiriesNotPlacedPageView");
            _mpRestRepository = mpRestRepository;
            _apiUserRepository = apiUserRepository;
        }

        public string GetCurrentJourney()
        {
            var token = _apiUserRepository.GetDefaultApiUserToken();
            var filter = $"ATTRIBUTE_CATEGORY_ID_TABLE.ATTRIBUTE_CATEGORY_ID = {JourneyCategoryID} AND GETDATE() BETWEEN START_DATE AND ISNULL(END_DATE, GETDATE())";
            var attribute = _mpRestRepository.UsingAuthenticationToken(token).Search<MpAttribute>(filter, "Attribute_Name").FirstOrDefault();
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
                var inquiries = groupId.HasValue
                    ? _ministryPlatformService.GetSubPageRecords(_groupInquiresSubPageId, groupId.Value, _apiUserRepository.GetToken())
                    : _ministryPlatformService.GetPageViewRecords(_groupInquiriesNotPlacedPageViewId, _apiUserRepository.GetToken());

                // Translate object format from MP to an MpInquiry object
                if (inquiries != null && inquiries.Count > 0)
                {
                    mpInquiries.AddRange(
                        inquiries.Select(
                            p =>
                                new MpInquiry
                                {
                                    InquiryId = p.ToInt("dp_RecordID"),
                                    GroupId = groupId ?? p.ToInt("Group_ID"),
                                    EmailAddress = p.ToString("Email"),
                                    PhoneNumber = p.ToString("Phone"),
                                    FirstName = p.ToString("First_Name"),
                                    LastName = p.ToString("Last_Name"),
                                    RequestDate = p.ToDate("Inquiry_Date"),
                                    Placed = p.ToNullableBool("Placed"),
                                    ContactId = p.ToInt("Contact_ID")
                                }));
                }
                else
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
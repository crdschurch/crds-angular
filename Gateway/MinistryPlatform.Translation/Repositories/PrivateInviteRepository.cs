using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Repositories
{
    public class PrivateInviteRepository : BaseRepository, IPrivateInviteRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly int _tripPrivateInviteId;
        private readonly int _tripInvitationsPageId;

        public PrivateInviteRepository(
            IMinistryPlatformService ministryPlatformService,
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _configurationWrapper = configurationWrapper;
            _tripPrivateInviteId = _configurationWrapper.GetConfigIntValue("TripPrivateInvite");
            _tripInvitationsPageId = _configurationWrapper.GetConfigIntValue("TripInvitations");
        }

        public MpPrivateInvite Create(int pledgeCampaignId, string emailAddress, string recipientName, string token)
        {
            var values = new Dictionary<string, object>
            {
                {"Pledge_Campaign_ID", pledgeCampaignId},
                {"Email_Address", emailAddress},
                {"Recipient_Name", recipientName}
            };

            try
            {
                var privateInviteId = _ministryPlatformService.CreateRecord(_tripInvitationsPageId, values, token, true);
                var record = _ministryPlatformService.GetRecordDict(_tripInvitationsPageId, privateInviteId, token, false);
                var invite = new MpPrivateInvite();
                invite.EmailAddress = record.ToString("Email_Address");
                invite.InvitationGuid = record.ToString("Invitation_GUID");
                invite.InvitationUsed = record.ToBool("Invitation_Used");
                invite.PledgeCampaignId = record.ToInt("Pledge_Campaign_ID");
                invite.PledgeCampaignIdText = record.ToString("Pledge_Campaign_ID_Text");
                invite.PrivateInvitationId = record.ToInt("Private_Invitation_ID");
                invite.RecipientName = record.ToString("Recipient_Name");

                return invite;
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("Create Private Invite failed.  Pledge Campaign Id: {0}", pledgeCampaignId), e);
            }
        }

        public bool PrivateInviteValid(int pledgeCampaignId, string guid, string emailAddress)
        {
            var token = ApiLogin();
            var searchString = string.Format(@",,""{0}"", ""{1}"", ""{2}""", guid, false, emailAddress);
            var records = _ministryPlatformService.GetSubpageViewRecords("TripPrivateInviteValid", pledgeCampaignId, token, searchString);
            return records.Count == 1;
        }

        public void MarkAsUsed(int pledgeCampaignId, string inviteGuid)
        {
            try
            {
                var apiLogin = ApiLogin();
                //Find invite by GUID
                var invites = _ministryPlatformService.GetSubpageViewRecords("TripPrivateInviteValid", pledgeCampaignId, apiLogin, inviteGuid);
                if (invites.Count != 1)
                {
                    throw new ApplicationException(string.Format("Error finding invite for {0}", inviteGuid));
                }

                //Mark it dude
                var dict = new Dictionary<string, object>
                {
                    {"Pledge_Campaign_ID", pledgeCampaignId},
                    {"Private_Invitation_ID", invites.First()["Private_Invitation_ID"]},
                    {"Invitation_Used", true}
                };

                _ministryPlatformService.UpdateRecord(_tripInvitationsPageId, dict, apiLogin);
            }
            catch (Exception ex)
            {
                throw  new ApplicationException("Private Invite - Mark as Used Failed",ex);
            }
        }
    }
}
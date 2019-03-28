using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Trip;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Extensions;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using IDonationRepository = MinistryPlatform.Translation.Repositories.Interfaces.IDonationRepository;
using IDonorRepository = MinistryPlatform.Translation.Repositories.Interfaces.IDonorRepository;
using IGroupRepository = MinistryPlatform.Translation.Repositories.Interfaces.IGroupRepository;
using PledgeCampaign = crds_angular.Models.Crossroads.Stewardship.PledgeCampaign;
using Newtonsoft.Json;
using Crossroads.Web.Auth.Models;

namespace crds_angular.Services
{
    public class TripService : MinistryPlatformBaseService, ITripService
    {
        private readonly IEventParticipantRepository _eventParticipantService;
        private readonly IDonationRepository _donationService;
        private readonly IGroupRepository _groupService;
        private readonly IFormSubmissionRepository _formSubmissionService;
        private readonly IEventRepository _mpEventService;
        private readonly IPledgeRepository _mpPledgeService;
        private readonly ICampaignRepository _campaignService;
        private readonly IPrivateInviteRepository _privateInviteService;
        private readonly ICommunicationRepository _communicationService;
        private readonly IContactRepository _contactService;
        private readonly IContactRelationshipRepository _contactRelationshipService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IPersonService _personService;
        private readonly IServeService _serveService;
        private readonly IProgramRepository _programRepository;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IDonorRepository _mpDonorService;

        private readonly ILog _logger = LogManager.GetLogger(typeof (TripService));

        public TripService(IEventParticipantRepository eventParticipant,
                           IDonationRepository donationService,
                           IGroupRepository groupService,
                           IFormSubmissionRepository formSubmissionService,
                           IEventRepository eventService,
                           IPledgeRepository pledgeService,
                           ICampaignRepository campaignService,
                           IPrivateInviteRepository privateInviteService,
                           ICommunicationRepository communicationService,
                           IContactRepository contactService,
                           IContactRelationshipRepository contactRelationshipService,
                           IConfigurationWrapper configurationWrapper,
                           IPersonService personService,
                           IServeService serveService,
                           IProgramRepository programRepository,
                           IApiUserRepository apiUserRepository,
                           ITripRepository tripRepository,
                           IDonorRepository mpDonorService)
        {
            _eventParticipantService = eventParticipant;
            _donationService = donationService;
            _groupService = groupService;
            _formSubmissionService = formSubmissionService;
            _mpEventService = eventService;
            _mpPledgeService = pledgeService;
            _campaignService = campaignService;
            _privateInviteService = privateInviteService;
            _communicationService = communicationService;
            _contactService = contactService;
            _contactRelationshipService = contactRelationshipService;
            _configurationWrapper = configurationWrapper;
            _personService = personService;
            _serveService = serveService;
            _programRepository = programRepository;
            _apiUserRepository = apiUserRepository;
            _tripRepository = tripRepository;
            _mpDonorService = mpDonorService;
        }

        public List<TripGroupDto> GetGroupsByEventId(int eventId)
        {
            var mpGroups = _groupService.GetGroupsForEvent(eventId);

            return mpGroups.Select(record => new TripGroupDto
            {
                GroupId = record.GroupId,
                GroupName = record.Name
            }).ToList();
        }

        public TripFormResponseDto GetFormResponses(int selectionId, int selectionCount, int formResponseId)
        {
            var tripApplicantResponses = GetTripAplicants(selectionId, selectionCount, formResponseId);

            var dto = new TripFormResponseDto();
            if (tripApplicantResponses.Errors != null)
            {
                if (tripApplicantResponses.Errors.Count != 0)
                {
                    dto.Errors = tripApplicantResponses.Errors;
                    return dto;
                }
            }

            //get groups for event, type = go trip
            var eventId = tripApplicantResponses.TripInfo.EventId;
            var eventGroups = _mpEventService.GetGroupsForEvent(eventId); //need to add check for group type?  TM 8/17

            dto.Applicants = tripApplicantResponses.Applicants;
            dto.Groups = eventGroups.Select(s => new TripGroupDto {GroupId = s.GroupId, GroupName = s.Name}).ToList();
            dto.Campaign = new PledgeCampaign
            {
                FundraisingGoal = tripApplicantResponses.TripInfo.FundraisingGoal,
                PledgeCampaignId = tripApplicantResponses.TripInfo.PledgeCampaignId,
                DestinationId = tripApplicantResponses.TripInfo.DestinationId
            };

            return dto;
        }

        public TripCampaignDto GetTripCampaign(int pledgeCampaignId)
        {
            var token = _apiUserRepository.GetDefaultApiClientToken();
            var campaign = _campaignService.GetPledgeCampaign(pledgeCampaignId, token);
            var response = new TripCampaignDto()
            {
                Id = campaign.Id,
                Name = campaign.Name,
                FormId = campaign.FormId,
                Nickname = campaign.Nickname,
                YoungestAgeAllowed = campaign.YoungestAgeAllowed,
                RegistrationEnd = campaign.RegistrationEnd,
                RegistrationStart = campaign.RegistrationStart,
                RegistrationDeposit = campaign.RegistrationDeposit,
                AgeExceptions = campaign.AgeExceptions,
                IsFull = false,
                EventId = campaign.EventId,
                EventStart = campaign.EventStart
            };

            if (campaign.MaximumRegistrants != null)
            {
                var pledges = _mpPledgeService.GetPledgesByCampaign(pledgeCampaignId, token);
                if (pledges.Count >= campaign.MaximumRegistrants)
                    response.IsFull = true;
            }

            return response;
        }

        private TripApplicantResponse GetTripAplicants(int selectionId, int selectionCount, int formResponseId)
        {
            var responses = formResponseId == -1
                ? _formSubmissionService.GetTripFormResponsesBySelectionId(selectionId)
                : _formSubmissionService.GetTripFormResponsesByRecordId(formResponseId);

            var messages = ValidateResponse(selectionCount, formResponseId, responses);
            return messages.Count > 0
                ? new TripApplicantResponse {Errors = messages.Select(m => new TripToolError {Message = m}).ToList()}
                : FormatTripResponse(responses);
        }

        private static TripApplicantResponse FormatTripResponse(List<MpTripFormResponse> responses)
        {
            var tripInfo = responses
                .Select(r =>
                            r.EventId != null
                                ? (r.PledgeCampaignId != null
                                    ? new TripInfo
                                    {
                                        EventId = (int) r.EventId,
                                        FundraisingGoal = r.FundraisingGoal,
                                        PledgeCampaignId = (int) r.PledgeCampaignId,
                                        DestinationId = r.DestinationId
                                    }
                                    : null)
                                : null);

            var applicants = responses.Select(record => new TripApplicant
            {
                ContactId = record.ContactId,
                DonorId = record.DonorId,
                ParticipantId = record.ParticipantId
            }).ToList();

            var resp = new TripApplicantResponse
            {
                Applicants = applicants,
                TripInfo = tripInfo.First()
            };
            return resp;
        }

        public List<FamilyMemberTripDto> GetFamilyMembers(int pledgeId, AuthDTO token)
        {
            var family = _serveService.GetImmediateFamilyParticipants(token.UserInfo.Mp.ContactId);
            var fam = new List<FamilyMemberTripDto>();
            foreach (var f in family)
            {
                // get status of family member on trip
                var signedUpDate = _formSubmissionService.GetTripFormResponseByContactId(f.ContactId, pledgeId);
                var fm = new FamilyMemberTripDto()
                {
                    Age = f.Age,
                    ContactId = f.ContactId,
                    Email = f.Email,
                    LastName = f.LastName,
                    LoggedInUser = f.LoggedInUser,
                    ParticipantId = f.ParticipantId,
                    PreferredName = f.PreferredName,
                    RelationshipId = f.RelationshipId,
                    SignedUpDate = signedUpDate,
                    SignedUp = (signedUpDate != null)
                };
                fam.Add(fm);
            }
            return fam;
        }

        private static List<string> ValidateResponse(int selectionCount, int formResponseId, List<MpTripFormResponse> responses)
        {
            var messages = new List<string>();
            if (responses.Count == 0)
            {
                messages.Add("Could not retrieve records from Ministry Platform");
            }

            if (formResponseId == -1 && responses.Count != selectionCount)
            {
                messages.Add("Error Retrieving Selection");
                messages.Add(string.Format("You selected {0} records in Ministry Platform, but only {1} were retrieved.", selectionCount, responses.Count));
                messages.Add("Please verify records you selected.");
            }

            var campaignCount = responses.GroupBy(x => x.PledgeCampaignId)
                .Select(x => new {Date = x.Key, Values = x.Distinct().Count()}).Count();
            if (campaignCount > 1)
            {
                messages.Add("Invalid Trip Selection - Multiple Campaigns Selected");
            }
            return messages;
        }

        public List<TripParticipantDto> Search(string search)
        {
            var results = _eventParticipantService.TripParticipants(search);

            var participants = results.GroupBy(r =>
                                                   new
                                                   {
                                                       r.ParticipantId,
                                                       r.ContactId,
                                                       r.EmailAddress,
                                                       r.Lastname,
                                                       r.Nickname
                                                   }).Select(x => new TripParticipantDto()
                                                   {
                                                       ParticipantId = x.Key.ParticipantId,
                                                       ContactId = x.Key.ContactId,
                                                       Email = x.Key.EmailAddress,
                                                       Lastname = x.Key.Lastname,
                                                       Nickname = x.Key.Nickname,
                                                       ShowGiveButton = true,
                                                       ShowShareButtons = false
                                                   }).ToDictionary(y => y.ParticipantId);

            foreach (var result in results)
            {
                // check status of pledge for campaign
                var pledge = _mpPledgeService.GetPledgeByCampaignAndDonor(result.CampaignId, result.DonorId);
                if (pledge == null || pledge.PledgeStatusId == 3)
                {
                    continue;
                }
                var tp = new TripDto
                {
                    EventParticipantId = result.EventParticipantId,
                    EventEnd = result.EventEndDate.ToString("MMM dd, yyyy"),
                    EventId = result.EventId,
                    EventStartDate = result.EventStartDate.ToUnixTime(),
                    EventStart = result.EventStartDate.ToString("MMM dd, yyyy"),
                    EventTitle = result.EventTitle,
                    EventType = result.EventType,
                    ProgramId = result.ProgramId,
                    ProgramName = result.ProgramName,
                    CampaignId = result.CampaignId,
                    CampaignName = result.CampaignName,
                    PledgeDonorId = result.DonorId
                };
                var participant = participants[result.ParticipantId];
                participant.Trips.Add(tp);
            }
            return participants.Values.Where(x => x.Trips.Count > 0).OrderBy(o => o.Lastname).ThenBy(o => o.Nickname).ToList();
        }

        public MyTripsDto GetMyTrips(AuthDTO token)
        {
            var family = _serveService.GetImmediateFamilyParticipants(token.UserInfo.Mp.ContactId);
            var familyTrips = new MyTripsDto();

            foreach (var member in family)
            {
                var trips = TripForContact(member.ContactId);
                familyTrips.MyTrips.AddRange(trips.MyTrips);
            }

            return familyTrips;
        }

        private MyTripsDto TripForContact(int contactId)
        {
            var searchString = ",,,,,,,,,,,,," + contactId;
            var trips = _eventParticipantService.TripParticipants(searchString);

            var distributions = _donationService.GetMyTripDistributions(contactId).OrderBy(t => t.EventStartDate);
            var myTrips = new MyTripsDto();

            var events = new List<Trip>();
            var eventIds = new List<int>();
            foreach (var trip in trips.Where(trip => !eventIds.Contains(trip.EventId)))
            {
                var tripParticipant = _eventParticipantService.TripParticipants("," + trip.EventId + ",,,,,,,,,,,," + contactId).FirstOrDefault();
                if (tripParticipant == null)
                {
                    continue;
                }
                eventIds.Add(trip.EventId);

                var pledge = _mpPledgeService.GetPledgeByCampaignAndDonor(trip.CampaignId, trip.DonorId);
                if (pledge == null)
                {
                    throw new ApplicationException(string.Format("Pledge not found for {0},{1}", trip.CampaignId, trip.DonorId));
                }
                if (pledge.PledgeStatusId != AppSetting("PledgeStatusDiscontinued"))
                {
                    var t = new Trip();
                    t.EventId = trip.EventId;
                    t.EventEndDate = trip.EventEndDate.ToString("MMM dd, yyyy");
                    t.EventStartDate = trip.EventStartDate.ToString("MMM dd, yyyy");
                    t.EventTitle = trip.EventTitle;
                    t.EventType = trip.EventType;
                    t.FundraisingDaysLeft = Math.Max(0, (pledge.CampaignEndDate - DateTime.Today).Days);
                    t.FundraisingGoal = Convert.ToInt32(pledge.PledgeTotal);
                    t.EventParticipantId = tripParticipant.EventParticipantId;
                    t.EventParticipantFirstName = tripParticipant.Nickname;
                    t.EventParticipantLastName = tripParticipant.Lastname;                    
                    t.IPromiseSigned = GetIPromise(tripParticipant.EventParticipantId);
                    t.ContactId = contactId;

                    events.Add(t);
                }
            }

            foreach (var e in events)
            {
                var donations = distributions.Where(d => d.EventId == e.EventId).OrderByDescending(d => d.DonationDate).ToList();
                foreach (var donation in donations)
                {
                    var gift = new TripGift();
                    if (donation.AnonymousGift)
                    {
                        gift.DonorNickname = "Anonymous";
                        gift.DonorLastName = "";
                    }
                    else
                    {
                        gift.DonorNickname = donation.DonorNickname ?? donation.DonorFirstName;
                        gift.DonorLastName = donation.DonorLastName;
                    }
                    gift.DonationDistributionId = donation.DonationDistributionId;
                    gift.DonorId = donation.DonorId;
                    gift.DonorEmail = donation.DonorEmail;
                    gift.DonationDate = donation.DonationDate.ToShortDateString();
                    gift.DonationAmount = donation.DonationAmount;
                    gift.PaymentTypeId = donation.PaymentTypeId;
                    gift.RegisteredDonor = donation.RegisteredDonor;
                    gift.MessageSent = donation.MessageSent;
                    gift.Anonymous = donation.AnonymousGift;
                    e.TripGifts.Add(gift);
                    e.TotalRaised += donation.DonationAmount;
                }
                myTrips.MyTrips.Add(e);
            }
            return myTrips;
        }

        public TripParticipantPledgeDto CreateTripParticipant(int contactId, int pledgeCampaignId)
        {
            var token = _apiUserRepository.GetDefaultApiClientToken();
            var result = _tripRepository.AddAsTripParticipant(contactId, pledgeCampaignId, token);
            if (!result.Status)
            {
                _logger.Error($"The trip was full when trying to create trip participant for contact {contactId}");
                throw new TripFullException();                
            }
            var tripParticipantPledgeInfo = new TripParticipantPledgeDto
            {
                CampaignName = result.Value.CampaignName,
                DonorId = result.Value.DonorId
            };
            return tripParticipantPledgeInfo;
        }

        public TripParticipantPledgeDto GetCampaignPledgeInfo(int contactId, int pledgeCampaignId)
        {
            var tripParticipantPledgeInfo = new TripParticipantPledgeDto();      
            var tripRecord = _campaignService.GetGoTripDetailsByCampaign(pledgeCampaignId).FirstOrDefault();          
            var tripDonor = _mpDonorService.GetContactDonor(contactId);          
            var campaign = _campaignService.GetPledgeCampaign(pledgeCampaignId);
      
            if (campaign == null)
                throw new ApplicationException($"Pledge campaign Id {pledgeCampaignId} not found or expired");

            tripParticipantPledgeInfo.PledgeAmount = tripRecord != null ? (int)tripRecord.CampaignFundRaisingGoal : 0;
            tripParticipantPledgeInfo.CampaignNickname = campaign.Nickname;
            tripParticipantPledgeInfo.CampaignName = campaign.Name;
            tripParticipantPledgeInfo.Deposit = tripRecord != null ? (int)tripRecord.RegistrationDeposit : 0;
            tripParticipantPledgeInfo.DonorId = tripDonor.DonorId;
            tripParticipantPledgeInfo.ProgramId = campaign.ProgramId;
            tripParticipantPledgeInfo.ProgramName = _programRepository.GetProgramById(tripParticipantPledgeInfo.ProgramId).Name;

            return tripParticipantPledgeInfo;
        }

        public bool HasScholarship(int contactId, int campaignId)
        {
            var campaign = _campaignService.GetPledgeCampaign(campaignId);
            var distributions = _donationService.GetMyTripDistributions(contactId);
            var scholarshipDollars = distributions.Where(d => d.EventId == campaign.EventId).Sum(d => d.DonationAmount);
            var pledgeTotal = distributions.FirstOrDefault(d => d.EventId == campaign.EventId)?.TotalPledge;
            return scholarshipDollars == pledgeTotal;
        }

        public void SendTripIsFullMessage(int campaignId)
        {
            var campaign = GetTripCampaign(campaignId);
            if (!campaign.IsFull) return;
            var templateId = _configurationWrapper.GetConfigIntValue("TripIsFullTemplateId");
            var fromReplyToContactId = _configurationWrapper.GetConfigIntValue("TripIsFullFromContactId");
            var fromReplyToEmailAddress = _configurationWrapper.GetConfigValue("TripIsFullFromEmailAddress");

            var eventDeets = _mpEventService.GetEvent(campaign.EventId);

            var mergeData = new Dictionary<String, Object>
            {
                {"Pledge_Campaign", campaign.Name}
            };

            var communication = _communicationService.GetTemplateAsCommunication(templateId,
                                                                                 fromReplyToContactId,
                                                                                 fromReplyToEmailAddress,
                                                                                 fromReplyToContactId,
                                                                                 fromReplyToEmailAddress,
                                                                                 eventDeets.PrimaryContact.ContactId,
                                                                                 eventDeets.PrimaryContact.EmailAddress,
                                                                                 mergeData);
            _communicationService.SendMessage(communication);
        }

        public int GeneratePrivateInvite(PrivateInviteDto dto)
        {
            var token = _apiUserRepository.GetDefaultApiClientToken();
            var invite = _privateInviteService.Create(dto.PledgeCampaignId, dto.EmailAddress, dto.RecipientName, token);
            var communication = PrivateInviteCommunication(invite);
            _communicationService.SendMessage(communication);

            return invite.PrivateInvitationId;
        }

        private MinistryPlatform.Translation.Models.MpCommunication PrivateInviteCommunication(MpPrivateInvite invite)
        {
            var templateId = _configurationWrapper.GetConfigIntValue("PrivateInviteTemplate");
            var template = _communicationService.GetTemplate(templateId);
            var fromContact = _contactService.GetContactById(_configurationWrapper.GetConfigIntValue("DefaultContactEmailId"));
            var replyToContact = _contactService.GetContactById(_configurationWrapper.GetConfigIntValue("GoTripsReplyToContactId"));
            var mergeData = SetMergeData(invite.PledgeCampaignIdText, invite.PledgeCampaignId, invite.InvitationGuid, invite.RecipientName);

            return new MinistryPlatform.Translation.Models.MpCommunication
            {
                AuthorUserId = 5,
                DomainId = 1,
                EmailBody = template.Body,
                EmailSubject = template.Subject,
                FromContact = new MpContact {ContactId = fromContact.Contact_ID, EmailAddress = fromContact.Email_Address},
                ReplyToContact = new MpContact { ContactId = replyToContact.Contact_ID, EmailAddress = replyToContact.Email_Address },
                ToContacts = new List<MpContact> {new MpContact{ContactId = fromContact.Contact_ID, EmailAddress = invite.EmailAddress}},
                MergeData = mergeData
            };
        }

        private Dictionary<string, object> SetMergeData(string tripTitle, int pledgeCampaignId, string inviteGuid, string participantName)
        {
            var mergeData = new Dictionary<string, object>
            {
                {"TripTitle", tripTitle},
                {"PledgeCampaignID", pledgeCampaignId},
                {"InviteGUID", inviteGuid},
                {"ParticipantName", participantName},
                {"BaseUrl", _configurationWrapper.GetConfigValue("BaseUrl")}
            };
            return mergeData;
        }

        public bool ValidatePrivateInvite(int pledgeCampaignId, string guid, AuthDTO token)
        {
            return _privateInviteService.PrivateInviteValid(pledgeCampaignId, guid, token.UserInfo.Mp.Email);
        }

        public int SaveApplication(TripApplicationDto dto)
        {
            try
            {
                UpdateChildSponsorship(dto);               
                var formResponse = new MpFormResponse
                {
                    ContactId = dto.ContactId, //contact id of the person the application is for
                    FormId = _configurationWrapper.GetConfigIntValue("TripApplicationFormId"),
                    PledgeCampaignId = dto.PledgeCampaignId,
                    FormAnswers = new List<MpFormAnswer>(FormatFormAnswers(dto))
                };

                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Error = (serializer, err) => err.ErrorContext.Handled = true
                };
                string json = JsonConvert.SerializeObject(dto, settings);                
                _logger.Warn($"Logging form data for contact {dto.ContactId}, PledgeCampaignId = {dto.PledgeCampaignId}: {json}");

                var formResponseId = _formSubmissionService.SubmitFormResponse(formResponse);
             
                if (dto.InviteGUID != null)
                {
                    _privateInviteService.MarkAsUsed(dto.PledgeCampaignId, dto.InviteGUID);
                }

                if (HasScholarship(dto.ContactId, dto.PledgeCampaignId))
                {
                    SendTripApplicantSuccessMessage(dto.ContactId);
                }
                else
                {
                    SendTripApplicantDonationComboMessage(dto);
                }

                _logger.Info($"SaveApplication success: ContactId = {dto.ContactId}, PledgeCampaignId = {dto.PledgeCampaignId}");

                return formResponseId;
            }
            catch (Exception ex)
            {
                // add exception to error log
                _logger.Error($"SaveApplication exception: ContactId = {dto.ContactId}, PledgeCampaignId = {dto.PledgeCampaignId}", ex);
               
                // include form data in error log (serialized json); ignore exceptions during serialization
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Error = (serializer, err) => err.ErrorContext.Handled = true;
                string json = JsonConvert.SerializeObject(dto, settings);
                _logger.Error($"SaveApplication data {json}");

                // send applicant message
                SendApplicantErrorMessage(dto.ContactId);

                // send trip admin message
                SendTripAdminErrorMessage(dto.PledgeCampaignId);

                //then re-throw or eat it?
                return 0;
            }
        }

        private void SendMessage(string templateKey, int toContactId, Dictionary<string, object> mergeData = null)
        {
            var templateId = _configurationWrapper.GetConfigIntValue(templateKey);
            var fromContactId = _configurationWrapper.GetConfigIntValue("DefaultContactEmailId");
            var fromContact = _contactService.GetContactById(fromContactId);
            var replyToContactId = _configurationWrapper.GetConfigIntValue("GoTripsReplyToContactId");
            var replyToContact = _contactService.GetContactById(replyToContactId);
            var toContact = _contactService.GetContactById(toContactId);
            var template = _communicationService.GetTemplateAsCommunication(templateId,
                                                                            fromContact.Contact_ID,
                                                                            fromContact.Email_Address,
                                                                            replyToContact.Contact_ID,
                                                                            replyToContact.Email_Address,
                                                                            toContact.Contact_ID,
                                                                            toContact.Email_Address,
                                                                            mergeData);
            _communicationService.SendMessage(template);
        }

        private void SendTripApplicantDonationComboMessage(TripApplicationDto dto)
        {
            var pledgeCampaign = _campaignService.GetPledgeCampaign(dto.PledgeCampaignId);
            var program = _programRepository.GetProgramById(pledgeCampaign.ProgramId);

            var mergeData = new Dictionary<string, object>
            {
                {"Destination_Name", pledgeCampaign.Nickname },
                {"Program_Name", program.Name},
                {"Donation_Amount", dto.DepositInformation.DonationAmount ?? ""},
                {"Donation_Date", dto.DepositInformation.DonationDate ?? ""},
                {"Payment_Method", dto.DepositInformation.PaymentMethod }
            };

            SendMessage("TripAppAndDonationComboMessageTemplateId", dto.ContactId, mergeData);
        }

        private void SendTripApplicantSuccessMessage(int contactId)
        {
            SendMessage("TripApplicantSuccessTemplate", contactId);
        }

        private void SendTripAdminErrorMessage(int campaignId)
        {
            var campaign = _campaignService.GetPledgeCampaign(campaignId);
            var tripEvent = _mpEventService.GetEvent(campaign.EventId);
            SendMessage("TripAdminErrorTemplate", tripEvent.PrimaryContact.ContactId);
        }

        private void SendApplicantErrorMessage(int contactId)
        {
            SendMessage("TripApplicantErrorTemplate", contactId);
        }

        private void UpdateChildSponsorship(TripApplicationDto dto)
        {
            if (!RequireSponsoredChild(dto.PageFive))
            {
                return;
            }
            var childId = GetChildId(dto,
                                     (child) =>
                                     {
                                         try
                                         {
                                             return _contactService.CreateContactForSponsoredChild(child.FirstName, child.LastName, child.Town, child.IdNumber);
                                         }
                                         catch (ApplicationException e)
                                         {
                                             _logger.Error("Unable to create the sponsored child: " + e.Message);
                                             return -1;
                                         }
                                     });
            if (childId == -1)
            {
                return;
            }

            // Check if relationship exists...
            var myRelationships = _contactRelationshipService.GetMyCurrentRelationships(dto.ContactId);
            var rel = myRelationships.Where(r => r.RelationshipID == _configurationWrapper.GetConfigIntValue("SponsoredChild") && r.RelatedContactID == childId);
            if (rel.Any())
            {
                return;
            }
            // Update the relationship
            var relationship = new MpRelationship
            {
                RelationshipID = _configurationWrapper.GetConfigIntValue("SponsoredChild"),
                RelatedContactID = childId,
                StartDate = DateTime.Today
            };
            _contactRelationshipService.AddRelationship(relationship, dto.ContactId);
        }

        private static bool RequireSponsoredChild(TripApplicationDto.ApplicationPageFive page5)
        {
            return page5.SponsorChildFirstName != null || page5.SponsorChildLastName != null || page5.SponsorChildNumber != null;
        }

        private int GetChildId(TripApplicationDto dto, Func<SponsoredChild, int> createChild)
        {
            var child = _contactService.GetContactByIdCard(dto.PageFive.SponsorChildNumber);
            if (child != null)
            {
                return child.Contact_ID;
            }
            var sponsoredChild = new SponsoredChild
            {
                FirstName = dto.PageFive.SponsorChildFirstName,
                LastName = dto.PageFive.SponsorChildLastName,
                IdNumber = dto.PageFive.SponsorChildNumber,
                Town = dto.PageFive.SponsorChildTown
            };
            return createChild(sponsoredChild);
        }

        private IEnumerable<MpFormAnswer> FormatFormAnswers(TripApplicationDto applicationData)
        {
            var answers = new List<MpFormAnswer>();

            var page2 = applicationData.PageTwo;

            answers.Add(new MpFormAnswer {Response = page2.Allergies, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.Allergies")});            
            answers.Add(new MpFormAnswer {Response = page2.GuardianFirstName, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.GuardianFirstName")});
            answers.Add(new MpFormAnswer {Response = page2.GuardianLastName, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.GuardianLastName")});
            answers.Add(new MpFormAnswer {Response = page2.Referral, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.Referral")});
            answers.Add(new MpFormAnswer {Response = page2.Why, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.Why")});

            var page3 = applicationData.PageThree;
            answers.Add(new MpFormAnswer {Response = page3.Conditions, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.Conditions") });
            answers.Add(new MpFormAnswer {Response = page3.EmergencyContactEmail, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.EmergencyContactEmail")});
            answers.Add(new MpFormAnswer {Response = page3.EmergencyContactFirstName, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.EmergencyContactFirstName")});
            answers.Add(new MpFormAnswer {Response = page3.EmergencyContactLastName, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.EmergencyContactLastName")});
            answers.Add(new MpFormAnswer {Response = page3.EmergencyContactPrimaryPhone, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.EmergencyContactPrimaryPhone")});
            answers.Add(new MpFormAnswer {Response = page3.EmergencyContactRelationship, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.EmergencyContactRelationship")});
            answers.Add(new MpFormAnswer {Response = page3.EmergencyContactSecondaryPhone, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.EmergencyContactSecondaryPhone")});

            var page4 = applicationData.PageFour;
            answers.Add(new MpFormAnswer {Response = page4.GroupCommonName, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.GroupCommonName")});
            answers.Add(new MpFormAnswer {Response = page4.InterestedInGroupLeader, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.InterestedInGroupLeader")});
            answers.Add(new MpFormAnswer {Response = page4.RoommateFirstChoice, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.RoommateFirstChoice")});
            answers.Add(new MpFormAnswer {Response = page4.RoommateSecondChoice, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.RoommateSecondChoice")});
            answers.Add(new MpFormAnswer {Response = page4.SupportPersonEmail, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.SupportPersonEmail")});
            answers.Add(new MpFormAnswer {Response = page4.WhyGroupLeader, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.WhyGroupLeader")});

            var page5 = applicationData.PageFive;
            answers.Add(new MpFormAnswer {Response = page5.NolaFirstChoiceWorkTeam, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.FirstChoiceWorkTeam")});
            answers.Add(new MpFormAnswer {Response = page5.NolaFirstChoiceExperience, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.FirstChoiceWorkTeamExperience")});
            answers.Add(new MpFormAnswer {Response = page5.NolaSecondChoiceWorkTeam, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.SecondChoiceWorkTeam")});

            var page6 = applicationData.PageSix;
            answers.Add(new MpFormAnswer {Response = page6.DescribeExperienceAbroad, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.DescribeExperienceAbroad")});
            answers.Add(new MpFormAnswer {Response = page6.ExperienceAbroad, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.ExperienceAbroad")});
            answers.Add(new MpFormAnswer
            {
                Response = page6.InternationalTravelExpericence,
                FieldId = _configurationWrapper.GetConfigIntValue("TripForm.InternationalTravelExpericence")
            });
            answers.Add(new MpFormAnswer {Response = page6.PassportNumber, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.PassportNumber")});
            answers.Add(new MpFormAnswer {Response = page6.PassportCountry, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.PassportCountry")});
            answers.Add(new MpFormAnswer {Response = page6.PassportExpirationDate, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.PassportExpirationDate")});
            answers.Add(new MpFormAnswer {Response = page6.PassportFirstName, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.PassportFirstName")});
            answers.Add(new MpFormAnswer {Response = page6.PassportLastName, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.PassportLastName")});
            answers.Add(new MpFormAnswer {Response = page6.PassportMiddleName, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.PassportMiddleName")});
            answers.Add(new MpFormAnswer {Response = page6.PastAbuseHistory, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.PastAbuseHistory")});
            answers.Add(new MpFormAnswer {Response = page6.ValidPassport, FieldId = _configurationWrapper.GetConfigIntValue("TripForm.ValidPassport")});

            return answers;
        }

        public bool GetIPromise(int eventParticipantId)
        {
            var token = _apiUserRepository.GetDefaultApiClientToken();
            var iPromiseDocId = _configurationWrapper.GetConfigIntValue("IPromiseDocumentId");
            var docs = _tripRepository.GetTripDocuments(eventParticipantId, token);
            return docs.Any(d => d.DocumentId == iPromiseDocId && d.Received);
        }

        public TripDocument GetIPromiseDocument(int eventParticipantId)
        {
            var token = _apiUserRepository.GetDefaultApiClientToken();
            var iPromiseDocId = _configurationWrapper.GetConfigIntValue("IPromiseDocumentId");
            var docs = _tripRepository.GetTripDocuments(eventParticipantId, token);
            return docs.Where(d => d.DocumentId == iPromiseDocId).Select(d => new TripDocument{ DocumentId = d.DocumentId, EventParticipantId = d.EventParticipantId, EventParticipantDocumentId = d.EventParticipantDocumentId, Received = d.Received, Notes = d.Notes, TripName = d.EventTitle}).FirstOrDefault();
        }

        public void ReceiveIPromiseDocument(TripDocument iPromiseDoc)
        {
            var token = _apiUserRepository.GetDefaultApiClientToken();
            _tripRepository.ReceiveTripDocument(new MpEventParticipantDocument
            {
                DocumentId = iPromiseDoc.DocumentId,
                EventParticipantId = iPromiseDoc.EventParticipantId,
                EventParticipantDocumentId = iPromiseDoc.EventParticipantDocumentId,
                EventTitle = iPromiseDoc.TripName,
                Notes = iPromiseDoc.Notes,
                Received = iPromiseDoc.Received
            }, token);
        }
    }
}

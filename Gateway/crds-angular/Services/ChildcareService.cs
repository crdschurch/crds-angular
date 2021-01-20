using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using crds_angular.Exceptions;
using crds_angular.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Childcare;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Services.Interfaces;
using crds_angular.Util.Interfaces;
using Crossroads.Utilities.Services;
using log4net;
using Microsoft.Ajax.Utilities;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Childcare;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class ChildcareService : IChildcareService
    {
        private readonly IChildcareRepository _childcareRepository;
        private readonly IChildcareRequestRepository _childcareRequestService;
        private readonly ICommunicationRepository _communicationService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IContactRepository _contactService;
        private readonly IEventParticipantRepository _eventParticipantService;
        private readonly IEventRepository _eventService;
        private readonly IEventService _crdsEventService;
        private readonly IGroupService _groupService;
        private readonly IParticipantRepository _participantService;
        private readonly IServeService _serveService;
        private readonly IDateTime _dateTimeWrapper;
        private readonly IApiUserRepository _apiUserService;
        private readonly IGroupParticipantRepository _groupParticipantRepository;
        private readonly int _childcareGroupType;

        private readonly ILog _logger = LogManager.GetLogger(typeof (ChildcareService));

        public ChildcareService(IEventParticipantRepository eventParticipantService,
                                ICommunicationRepository communicationService,
                                IConfigurationWrapper configurationWrapper,
                                IContactRepository contactService,
                                IEventRepository eventService,
                                IParticipantRepository participantService,
                                IServeService serveService,
                                IDateTime dateTimeWrapper,
                                IApiUserRepository apiUserService, 
                                IEventService crdsEventService, 
                                IChildcareRequestRepository childcareRequestService,
                                IGroupService groupService,
                                IChildcareRepository childcareRepository,
                                IGroupParticipantRepository groupParticipantRepository)
        {
            _childcareRequestService = childcareRequestService;
            _eventParticipantService = eventParticipantService;
            _communicationService = communicationService;
            _configurationWrapper = configurationWrapper;
            _contactService = contactService;
            _crdsEventService = crdsEventService;
            _eventService = eventService;
            _participantService = participantService;
            _serveService = serveService;
            _dateTimeWrapper = dateTimeWrapper;
            _apiUserService = apiUserService;
            _groupService = groupService;
            _childcareRepository = childcareRepository;
            _groupParticipantRepository = groupParticipantRepository;

            _childcareGroupType = _configurationWrapper.GetConfigIntValue("ChildcareGroupType");
        }

        public List<FamilyMember> MyChildren(int contactId)
        {
            var family = _serveService.GetImmediateFamilyParticipants(contactId);
            var myChildren = new List<FamilyMember>();

            foreach (var member in family)
            {
                var schoolGrade = SchoolGrade(member.HighSchoolGraduationYear);
                var maxAgeWithoutGrade = _configurationWrapper.GetConfigIntValue("MaxAgeWithoutGrade");
                var maxGradeForChildcare = _configurationWrapper.GetConfigIntValue("MaxGradeForChildcare");
                if (member.Age == 0)
                {
                    continue;
                }
                if (schoolGrade == 0 && member.Age <= maxAgeWithoutGrade)
                {
                    myChildren.Add(member);
                }
                else if (schoolGrade > 0 && schoolGrade <= maxGradeForChildcare)
                {
                    myChildren.Add(member);
                }
            }
            return myChildren;
        }

        public void SaveRsvp(ChildcareRsvpDto saveRsvp)
        {
            var participant = _participantService.GetParticipant(saveRsvp.ChildContactId);

            try
            {
                var participantSignup = new ParticipantSignup
                {
                    particpantId = participant.ParticipantId,
                    groupRoleId = _configurationWrapper.GetConfigIntValue("Group_Role_Default_ID"),
                    capacityNeeded = 1,
                    EnrolledBy = saveRsvp.EnrolledBy
                };

                _groupService.addParticipantToGroupNoEvents(saveRsvp.GroupId, participantSignup);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Save RSVP failed for group ({0}), contact ({1})", saveRsvp.GroupId, saveRsvp.ChildContactId), ex);
                throw;
            }
        }

        public void CancelRsvp(ChildcareRsvpDto cancelRsvp)
        {
            try
            {
                // get participant here, as we don't get the participant id passed down from the frontend
                var participant = _participantService.GetParticipant(cancelRsvp.ChildContactId);

                // get all group participant records that are not end dated for this participant
                var groupParticipants =
                    _groupParticipantRepository.GetGroupParticipantsRecordsForParticipant(participant.ParticipantId,
                        cancelRsvp.GroupId);

                foreach (var groupParticipant in groupParticipants)
                {
                    groupParticipant.EndDate = System.DateTime.Now;
                }

                _groupParticipantRepository.EndDateGroupParticipantRecords(groupParticipants);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Cancel RSVP failed for group ({0}), contact ({1})", cancelRsvp.GroupId, cancelRsvp.ChildContactId), ex);
                throw;
            }
        }

        public void CreateChildcareRequest(ChildcareRequestDto request)
        {
            var token = _apiUserService.GetDefaultApiClientToken();
            var mpRequest = request.ToMPChildcareRequest();
            var childcareRequestId = _childcareRequestService.CreateChildcareRequest(mpRequest);
            _childcareRequestService.CreateChildcareRequestDates(childcareRequestId, mpRequest, token);
            try
            {
                var childcareRequest = _childcareRequestService.GetChildcareRequest(childcareRequestId, token);
                SendChildcareRequestNotification(childcareRequest);
            }
           catch (Exception ex)
            {
                _logger.Error(string.Format("Save Request failed"), ex);
            }

        }

        public void UpdateChildcareRequest(ChildcareRequestDto request)
        {
            var token = _apiUserService.GetDefaultApiClientToken();
            var mpRequest = request.ToMPChildcareRequest();
            _childcareRequestService.UpdateChildcareRequest(mpRequest);
            //delete the current childcare request dates
            _childcareRequestService.DeleteAllChildcareRequestDates(request.ChildcareRequestId);

            //add the new dates
            _childcareRequestService.CreateChildcareRequestDates(request.ChildcareRequestId, mpRequest, token);
            try
            {
                var childcareRequest = _childcareRequestService.GetChildcareRequest(request.ChildcareRequestId, token);
                SendChildcareRequestNotification(childcareRequest);
            }
            catch (Exception ex)
            {
                _logger.Error("Update Request failed", ex);
            }

        }

        // TODO: Should we merge childcareRequestId into the childcareRequestDto?
        public void ApproveChildcareRequest(int childcareRequestId, ChildcareRequestDto childcareRequest)
        {
            try
            {
                var request = GetChildcareRequestForReview(childcareRequestId);
                var datesFromRequest = _childcareRequestService.GetChildcareRequestDates(childcareRequestId);
                var requestedDates = childcareRequest.DatesList.Select(date => GetChildcareDateFromList(datesFromRequest, date)).ToList();
                if (requestedDates.Count == 0)
                {
                    throw new ChildcareDatesMissingException(childcareRequestId);
                }
                var events = _childcareRequestService.FindChildcareEvents(childcareRequestId, requestedDates, request);

                var childcareEvents = GetChildcareEventsfortheDates(events, requestedDates, request);
                var missingDates = requestedDates.Where(childcareRequestDate => !childcareEvents.ContainsKey(childcareRequestDate.ChildcareRequestDateId)).ToList();
                if (missingDates.Count > 0)
                {
                    var dateList = missingDates.Aggregate("", (current, date) => current + ", " + date.RequestDate.ToShortDateString());
                    _logger.Debug("Missing events for dates: ${dateList}");
                    var dateMap = missingDates.Select(c => c.RequestDate).ToList();
                    throw new EventMissingException(dateMap);
                }

                //set the approved column for dates to true
                foreach (var ccareDates in requestedDates)
                {
                    _childcareRequestService.DecisionChildcareRequestDate(ccareDates.ChildcareRequestDateId, true);
                    var eventId = childcareEvents.Where((ev) => ev.Key == ccareDates.ChildcareRequestDateId).Select( (ev) => ev.Value).SingleOrDefault();
                    var eventGroup = new MpEventGroup() {Closed = false, Created = true, EventId = eventId, GroupId = request.GroupId};
                    var currentGroups = _eventService.GetGroupsForEvent(eventId).Select((g) => g.GroupId).ToList();
                    if (!currentGroups.Contains(request.GroupId))
                    {
                        _eventService.CreateEventGroup(eventGroup, _apiUserService.GetDefaultApiClientToken());
                    }
                }

                var requestStatusId = GetApprovalStatus(datesFromRequest, requestedDates);
                _childcareRequestService.DecisionChildcareRequest(childcareRequestId, requestStatusId, childcareRequest.ToMPChildcareRequest());
                var templateId = GetApprovalEmailTemplate(requestStatusId);
                SendChildcareRequestDecisionNotification(childcareRequestId, requestedDates, childcareRequest, templateId);
            }
            catch (EventMissingException ex)
            {
                throw;
            }
            catch (ChildcareDatesMissingException ex)
            {
                throw;
            }
            catch (DuplicateChildcareEventsException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Update Request failed"), ex);
                throw new Exception("Approve Childcare failed", ex);
            }
        }

        public void SendChildcareReminders()
        {
            var token = _apiUserService.GetDefaultApiClientToken();                     
            var toEmails = _childcareRepository.GetChildcareReminderEmails(token);
            var threeDaysOut = DateTime.Now.AddDays(3);

            foreach (var toContact in toEmails.Where((contact) => contact.EmailAddress != null))
            {
                var mergeData = SetMergeDataForChildcareReminder(toContact, threeDaysOut);
                var communication = SetupChilcareReminderCommunication(toContact, mergeData);               
                _communicationService.SendMessage(communication, false);
            };                                   
        }

        public MinistryPlatform.Translation.Models.MpCommunication SetupChilcareReminderCommunication(MpContact recipient, Dictionary<string,object> mergeData)
        {
            var templateId = _configurationWrapper.GetConfigIntValue("ChildcareReminderTemplateId");
            var template = _communicationService.GetTemplate(templateId);

            var fromId = _configurationWrapper.GetConfigIntValue("DefaultContactEmailId");
            var from = _contactService.GetContactEmail(fromId);
            var fromContact = new MpContact { ContactId = fromId, EmailAddress = from };

            return _communicationService.GetTemplateAsCommunication(templateId, 
                                                                fromId, 
                                                                from, 
                                                                fromId, 
                                                                from, 
                                                                recipient.ContactId, 
                                                                recipient.EmailAddress, 
                                                                mergeData);
        }

        public Dictionary<string, object> SetMergeDataForChildcareReminder(MpContact toContact, DateTime threeDaysOut)
        {
            var person = _contactService.GetContactById(toContact.ContactId);
            var url = _configurationWrapper.GetConfigValue("BaseUrl");

            return new Dictionary<string, object>()
            {
                {"Nickname", person.Nickname},
                {"Childcare_Date", threeDaysOut.ToString("MM/dd/yyyy")},
                {"Childcare_Day", threeDaysOut.ToString("dddd, MMMM dd")},
                {"Base_URL", $"https://{url}" }
            };
        }


        private Dictionary<int, int> GetChildcareEventsfortheDates(List<MpEvent> events, List<MpChildcareRequestDate> requestedDates, MpChildcareRequest request)
        {
            var prefTime = request.PreferredTime.Substring(request.PreferredTime.IndexOf(',') + 1).Split('-');
            var requestStartTime = DateTime.ParseExact(prefTime[0].Trim(), "h:mmtt", CultureInfo.InvariantCulture);
            var requestEndTime = DateTime.ParseExact(prefTime[1].Trim(), "h:mmtt", CultureInfo.InvariantCulture);
            var childcareEvents = new Dictionary<int, int>();
                foreach (var date in requestedDates)
                {
                    var foundEvent = events.FindAll(
                        e => (e.EventStartDate == date.RequestDate.Date.Add(requestStartTime.TimeOfDay) &&
                        (e.EventEndDate == date.RequestDate.Date.Add(requestEndTime.TimeOfDay)) &&
                        (e.CongregationId == request.LocationId)));
                    if (foundEvent.Count > 1)
                    {
                        throw new DuplicateChildcareEventsException(date.RequestDate);
                    }                    
                    if (foundEvent != null)
                    {
                        foreach (var eachEvent in foundEvent)
                         {
                            childcareEvents.Add(date.ChildcareRequestDateId, eachEvent.EventId);
                         }                   
                    }
                }
           return childcareEvents;
        }
        
        private int GetApprovalEmailTemplate(int requestStatusId)
        {
            if (requestStatusId == 1)
            {
                return _configurationWrapper.GetConfigIntValue("ChildcareRequestApprovalNotificationTemplate");
            }
            else
            {
                return _configurationWrapper.GetConfigIntValue("ChildcareRequestConditionalApprovalNotificationTemplate");
            }
        }

        private MpChildcareRequestDate GetChildcareDateFromList(List<MpChildcareRequestDate> allDates, DateTime date)
        {
            var requestedDate = new MpChildcareRequestDate();
            return allDates.SingleOrDefault(d => date.Date == d.RequestDate.Date); 
        }

        private int GetApprovalStatus(List<MpChildcareRequestDate> datesFromMP, List<MpChildcareRequestDate> datesApproving)
        {
            if (datesFromMP.Count > datesApproving.Count)
            {
                return _configurationWrapper.GetConfigIntValue("ChildcareRequestConditionallyApproved");
            }
            return _configurationWrapper.GetConfigIntValue("ChildcareRequestApproved");
        }

        public void RejectChildcareRequest(int childcareRequestId, ChildcareRequestDto childcareRequest)
        {
            try
            {
                //set the approved column for dates to false
                var childcareDates = _childcareRequestService.GetChildcareRequestDates(childcareRequestId);
                foreach (var ccareDates in childcareDates)
                {
                    _childcareRequestService.DecisionChildcareRequestDate(ccareDates.ChildcareRequestDateId, false);
                }

                _childcareRequestService.DecisionChildcareRequest(childcareRequestId, _configurationWrapper.GetConfigIntValue("ChildcareRequestRejected"), childcareRequest.ToMPChildcareRequest());
                var templateId = _configurationWrapper.GetConfigIntValue("ChildcareRequestRejectionNotificationTemplate");
                SendChildcareRequestDecisionNotification(childcareRequestId, childcareDates, childcareRequest, templateId);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Update Request failed"), ex);
                throw new Exception("Reject Childcare failed", ex);
            }
        }

        //TODO: SPLIT OUT INTO SMALLER METHODS AND WRITE TESTS!!!!!!!!!!!!!
        public ChildcareDashboardDto GetChildcareDashboard(int contactId, int householdId, HouseHoldData householdData)
        {

            var dashboard = new ChildcareDashboardDto();
            var token = _apiUserService.GetDefaultApiClientToken();

            // Add members of other household(s)
            // Doesn't this really belong in the getHouseholds method?
            householdData.AllMembers.AddRange(_contactService.GetOtherHouseholdMembers(householdId));

            var dashboardData = _childcareRepository.GetChildcareDashboard(contactId);
            foreach (var childcareDashboard in dashboardData)
            {
                // Add the Date if it doesn't already exist in the dashboard
                dashboard.AvailableChildcareDates = UpdateAvailableChildCareDates(dashboard.AvailableChildcareDates,
                                                                                  childcareDashboard.EventStartDate.Date,
                                                                                  childcareDashboard.Cancelled);
             
                var eligibleChildren = new List<ChildcareRsvp>();

                foreach (var member in householdData.AllMembers)
                {
                    if (member.HouseholdPosition != null && !member.HouseholdPosition.ToUpper().StartsWith("HEAD") && member.HouseholdPosition.StartsWith("Minor") && eligibleChildren.All(c => c.ContactId != member.ContactId)) //TODO: Get rid of magic string. Household Position
                    {
                        var echild = new ChildcareRsvp
                        {
                            ContactId = member.ContactId,
                            DisplayName = member.Nickname + ' ' + member.LastName,
                            ChildEligible = (member.Age <= childcareDashboard.ChildcareMaxAge),
                            ChildHasRsvp = _childcareRepository.IsChildRsvpd(member.ContactId, childcareDashboard.ChildcareGroupID, token)
                        };
                        eligibleChildren.Add(echild);
                    }                       
                }

                var ccEvent = dashboard.AvailableChildcareDates.First(d => d.EventDate.Date == childcareDashboard.EventStartDate.Date);
                ccEvent.Groups.Add(new ChildcareGroup
                {
                    GroupName = childcareDashboard.GroupName,
                    EventStartTime = childcareDashboard.EventStartDate,
                    EventEndTime = childcareDashboard.EventEndDate,
                    CongregationId = childcareDashboard.CongregationID,
                    GroupMemberName = childcareDashboard.Nickname + ' ' + childcareDashboard.LastName,
                    MaximumAge = childcareDashboard.ChildcareMaxAge,                        
                    EligibleChildren = eligibleChildren,
                    ChildcareGroupId = childcareDashboard.ChildcareGroupID,
                    GroupParticipantId = childcareDashboard.GroupParticipantID
                });
            }                       
            return dashboard;
        }

        /// <summary>
        /// Add a new childcare date to the list of current childcare dates only if the list doesn't contain 
        /// an entry with the same date as is being requested. It also orders the list of dates ascending. 
        /// </summary>
        /// <param name="currentDates"> The current list of ChildCareDates </param>
        /// <param name="dateToAdd"> The date to add to the list </param>
        /// <param name="hasBeenCancelled"> whether or not the childcare event being requested is cancelled. </param>
        /// <returns> a new list that is ordered by date ascending. </returns>
        public List<ChildCareDate> UpdateAvailableChildCareDates(List<ChildCareDate> currentDates,  DateTime dateToAdd, bool hasBeenCancelled )
        {
            if (currentDates.Any(d => d.EventDate.Date == dateToAdd)) return currentDates.OrderBy(x => x.EventDate).ToList();

            var childcareDate = new ChildCareDate
            {
                EventDate = dateToAdd,
                Cancelled = hasBeenCancelled
            };
            var newDates = currentDates.Select(ch => ch).ToList();
            newDates.Add(childcareDate);
            return newDates.OrderBy(x => x.EventDate).ToList();
        }

        /// <summary>
        /// Determine who is the head of household in a household.
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="householdId"></param>
        /// <returns> 
        ///     A HouseholdData object 
        ///     Throws a NotHeadOfHouseholdException if the contactId passed in is not a head of household
        /// </returns>
        public HouseHoldData GetHeadsOfHousehold(int contactId, int householdId)
        {
            var household = _contactService.GetHouseholdFamilyMembers(householdId);
            var houseHeads = household.Where(h => h.HouseholdPosition != null && h.HouseholdPosition.ToUpper().StartsWith("HEAD")).ToList(); //TODO: Get rid of magic string. Household Position
            if (houseHeads.All(h => h.ContactId != contactId))
            {
                throw new NotHeadOfHouseholdException(contactId);
            }
            return new HouseHoldData() {AllMembers = household, HeadsOfHousehold = houseHeads};
        }

        public MpChildcareRequest GetChildcareRequestForReview(int childcareRequestId)
        {
            try
            {
                return _childcareRequestService.GetChildcareRequestForReview(childcareRequestId);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("GetChildcareRequestForReview failed"), ex);
            }
            return null;
        }

        private void SendChildcareRequestDecisionNotification(int requestId, List<MpChildcareRequestDate> childcareRequestDates, ChildcareRequestDto childcareRequest, int templateId)
        {
            var childcareRequestEmail = _childcareRequestService.GetChildcareRequest(requestId, _apiUserService.GetDefaultApiClientToken());;
            var template = _communicationService.GetTemplate(templateId);

            var decisionNotes = childcareRequest.DecisionNotes ?? "N/A";

           
            var authorUserId = _configurationWrapper.GetConfigIntValue("DefaultUserAuthorId");
            var datesList = childcareRequestDates.Select(dateRec => dateRec.RequestDate).Select(requestDate => BuildParagraph("", requestDate.ToShortDateString())).ToList();
            var styles = Styles();
            var htmlCell = new HtmlElement("td", styles).Append(datesList);
            var htmlRow = new HtmlElement("tr", styles).Append(htmlCell);
            var htmlTBody = new HtmlElement("tbody", styles).Append(htmlRow);
            var htmlTable = new HtmlElement("table", styles).Append(htmlTBody);

            var mergeData = new Dictionary<string, object>
            {
                {"Group", childcareRequestEmail.GroupName},
                {"ChildcareSession", childcareRequestEmail.ChildcareSession},
                {"DecisionNotes", decisionNotes },
                {"Frequency", childcareRequest.Frequency},
                {"Dates", htmlTable.Build() },
                {"RequestId", childcareRequestEmail.RequestId },
                {"Base_Url", _configurationWrapper.GetConfigValue("BaseMPUrl")},
                {"Congregation", childcareRequestEmail.CongregationName }
            };
            var toContactsList = new List<MpContact> {new MpContact {ContactId = childcareRequestEmail.RequesterId, EmailAddress = childcareRequestEmail.RequesterEmail}};


            var communication = new MinistryPlatform.Translation.Models.MpCommunication
            {
                AuthorUserId = authorUserId,
                EmailBody = template.Body,
                EmailSubject = template.Subject,
                FromContact = new MpContact { ContactId = childcareRequestEmail.ChildcareContactId, EmailAddress = childcareRequestEmail.ChildcareContactEmail},
                ReplyToContact = new MpContact { ContactId = childcareRequestEmail.ChildcareContactId, EmailAddress = childcareRequestEmail.ChildcareContactEmail },
                ToContacts = toContactsList,
                MergeData = mergeData
            };

            try
            {
                _communicationService.SendMessage(communication);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Send Childcare request approval notification email failed"), ex);
            }

        }

        public void SendChildcareRequestNotification( MpChildcareRequestEmail request)
        {
            var templateId = _configurationWrapper.GetConfigIntValue("ChildcareRequestNotificationTemplate");
            var authorUserId = _configurationWrapper.GetConfigIntValue("DefaultUserAuthorId");          
            var template = _communicationService.GetTemplate(templateId);           

            var mergeData = new Dictionary<string, object>
            {
                {"Requester", request.Requester},
                {"Nickname", request.RequesterNickname },
                {"LastName", request.RequesterLastName },
                {"Group", request.GroupName },
                {"Site", request.CongregationName },
                {"StartDate", (request.StartDate).ToShortDateString() },
                {"EndDate", (request.EndDate).ToShortDateString() },
                {"ChildcareSession", request.ChildcareSession },
                {"RequestId", request.RequestId },
                {"Base_Url", _configurationWrapper.GetConfigValue("BaseMPUrl")}
            };

            var communication = new MinistryPlatform.Translation.Models.MpCommunication
             {
                AuthorUserId = authorUserId,
                EmailBody = template.Body,
                EmailSubject = template.Subject,
                FromContact = new MpContact {ContactId = request.RequesterId, EmailAddress = request.RequesterEmail},
                ReplyToContact = new MpContact { ContactId = request.RequesterId, EmailAddress = request.RequesterEmail},
                ToContacts = new List<MpContact> {new MpContact {ContactId = request.ChildcareContactId, EmailAddress = request.ChildcareContactEmail } },
                MergeData = mergeData
             };

            try
            {
                _communicationService.SendMessage(communication);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Send Childcare request notification email failed"), ex);
            }


        }

        public int SchoolGrade(int graduationYear)
        {
            if (graduationYear == 0)
            {
                return 0;
            }
            var today = _dateTimeWrapper.Today;
            var todayMonth = today.Month;
            var yearForCalc = today.Year;
            if (todayMonth > 7)
            {
                yearForCalc = today.Year + 1;
            }

            var grade = 12 - (graduationYear - yearForCalc);
            if (grade <= 12 && grade >= 0)
            {
                return grade;
            }
            return 0;
        }

        public void SendRequestForRsvp()
        {
            var daysBeforeEvent = _configurationWrapper.GetConfigIntValue("NumberOfDaysBeforeEventToSend");
            var templateId = _configurationWrapper.GetConfigIntValue("ChildcareRequestTemplate");
            var authorUserId = _configurationWrapper.GetConfigIntValue("DefaultUserAuthorId"); ;
            var template = _communicationService.GetTemplate(templateId);
            var fromContact = _contactService.GetContactById(_configurationWrapper.GetConfigIntValue("DefaultContactEmailId"));
            const int domainId = 1;
            var token = _apiUserService.GetDefaultApiClientToken();

            var participants = _eventParticipantService.GetChildCareParticipants(daysBeforeEvent);
            foreach (var participant in participants)
            {
                var childEvent = _crdsEventService.GetChildcareEvent(participant.EventId);
                var childcareParticipants = _crdsEventService.EventParticpants(childEvent.EventId, token);
                var mine = _crdsEventService.MyChildrenParticipants(participant.ContactId, childcareParticipants, token);

                if (mine!=null && mine.Any())
                {
                    // i have kids already signed up for childcare!
                    continue;
                }
                var mergeData = SetMergeData(participant.GroupName, participant.EventStartDateTime, participant.EventId);
                var replyToContact = ReplyToContact(childEvent);
                var communication = FormatCommunication(authorUserId,
                                                        domainId,
                                                        template,
                                                        fromContact,
                                                        replyToContact,
                                                        participant.ContactId,
                                                        participant.ParticipantEmail,
                                                        mergeData);
                try
                {
                    _communicationService.SendMessage(communication);
                }
                catch (Exception ex)
                {
                    LogError(participant, ex);
                }
            }
        }

        private static MpMyContact ReplyToContact(MpEvent childEvent)
        {
            var contact = childEvent.PrimaryContact;
            var replyToContact = new MpMyContact
            {
                Contact_ID = contact.ContactId,
                Email_Address = contact.EmailAddress
            };
            return replyToContact;
        }

        private static MinistryPlatform.Translation.Models.MpCommunication FormatCommunication(int authorUserId,
                                                         int domainId,
                                                         MpMessageTemplate template,
                                                         MpMyContact fromContact,
                                                         MpMyContact replyToContact,
                                                         int participantContactId,
                                                         string participantEmail,
                                                         Dictionary<string, object> mergeData)
        {
            var communication = new MinistryPlatform.Translation.Models.MpCommunication
            {
                AuthorUserId = authorUserId,
                DomainId = domainId,
                EmailBody = template.Body,
                EmailSubject = template.Subject,
                FromContact = new MpContact {ContactId = fromContact.Contact_ID, EmailAddress = fromContact.Email_Address},
                ReplyToContact = new MpContact {ContactId = replyToContact.Contact_ID, EmailAddress = replyToContact.Email_Address},
                ToContacts = new List<MpContact> {new MpContact {ContactId = participantContactId, EmailAddress = participantEmail}},
                MergeData = mergeData
            };
            return communication;
        }

        private void LogError(MpEventParticipant participant, Exception ex)
        {
            var participantId = participant.ParticipantId;
            var groupId = participant.GroupId;
            var eventId = participant.EventId;
            _logger.Error(string.Format("Send Childcare RSVP email failed. Participant: {0}, Group: {1}, Event: {2}", participantId, groupId, eventId), ex);
        }

        private Dictionary<string, object> SetMergeData(string groupName, DateTime eventStartDateTime, int eventId)
        {
            var mergeData = new Dictionary<string, object>
            {
                {"GroupName", groupName},
                {"EventStartDate", eventStartDateTime.ToString("g")},
                {"EventId", eventId},
                {"BaseUrl", _configurationWrapper.GetConfigValue("BaseUrl")}
            };
            return mergeData;
        }
        private static HtmlElement BuildParagraph(string label, string value)
        {
            var els = new List<HtmlElement>()
            {
                new HtmlElement("strong", label),
                new HtmlElement("span", value)
            }
                ;
            return new HtmlElement("p", els);
        }
        private Dictionary<string, string> Styles()
        {
            return new Dictionary<string, string>()
            {
                {"style", "border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; padding: 0; border:none; border-color:#ffffff;font-size: small; font-weight: normal;" }
            };
        }

        public void SendChildcareCancellationNotification()
        {
            var templateId = _configurationWrapper.GetConfigIntValue("ChildcareCancelledTemplate");
            var template = _communicationService.GetTemplate(templateId);
            var authorUserId = _configurationWrapper.GetConfigIntValue("DefaultUserAuthorId");

            var notificationData = _childcareRepository.GetChildcareCancellations();
            foreach (var participant in notificationData.DistinctBy(p => p.EnrollerContactId))
            {
                var kiddos = notificationData.Where(k => k.EnrollerContactId == participant.EnrollerContactId).Aggregate("", (current, kid) => current + (kid.ChildNickname + " " + kid.ChildLastname + "<br>"));
                var mergeData = new Dictionary<string, object>
                {
                    {"Group_Name", participant.EnrollerGroupName },
                    {"Childcare_Date", participant.ChildcareEventDate.ToString("MM/dd/yyyy") },
                    {"Group_Member_Nickname", participant.EnrollerNickname },
                    {"Childcare_Day", participant.ChildcareEventDate.ToString("dddd, MMMM dd") },
                    {"Child_List", kiddos}
                };
                var comm = new MinistryPlatform.Translation.Models.MpCommunication
                {
                    AuthorUserId = authorUserId,
                    DomainId = 1,
                    EmailBody = template.Body,
                    EmailSubject = template.Subject,
                    FromContact = new MpContact { ContactId = participant.ChildcareContactId, EmailAddress = participant.ChildcareContactEmail },
                    ReplyToContact = new MpContact { ContactId = participant.ChildcareContactId, EmailAddress = participant.ChildcareContactEmail },
                    ToContacts = new List<MpContact> { new MpContact { ContactId = participant.EnrollerContactId, EmailAddress = participant.EnrollerEmail } },
                    MergeData = mergeData
                };
                _communicationService.SendMessage(comm);
            }

            foreach (var participant in notificationData)
            {
                _groupService.endDateGroupParticipant(participant.ChildGroupId, participant.ChildGroupParticipantId);
            }

            foreach (var group in notificationData.DistinctBy(g => g.ChildGroupId))
            {
                _groupService.EndDateGroup(group.ChildGroupId);
            }
        }
    } 
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;



namespace MinistryPlatform.Translation.Repositories
{
    public class GroupRepository : BaseRepository, IGroupRepository
    {
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly ICommunicationRepository _communicationService;
        private readonly IContactRepository _contactService;
        private readonly IContentBlockService _contentBlockService;
        private readonly IAddressRepository _addressRepository;
        private readonly IObjectAttributeRepository _objectAttributeRepository;
        private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int GroupsParticipantsPageId = Convert.ToInt32(AppSettings("GroupsParticipants"));
        private readonly int GroupsPageId = Convert.ToInt32(AppSettings("Groups"));
        private readonly int GroupsSubgroupsPageId = Convert.ToInt32(AppSettings("GroupsSubgroups"));
        private readonly int DefaultEmailContactId = Convert.ToInt32(AppSettings("DefaultContactEmailId"));
        private readonly int GroupSignupRelationsPageId = Convert.ToInt32((AppSettings("GroupSignUpRelations")));
        private readonly int CommunityGroupConfirmationTemplateId = Convert.ToInt32(AppSettings("CommunityGroupConfirmationTemplateId"));
        private readonly int CommunityGroupWaitListConfirmationTemplateId = Convert.ToInt32(AppSettings("CommunityGroupWaitListConfirmationTemplateId"));
        private readonly int MyCurrentGroupsPageView = Convert.ToInt32(AppSettings("MyCurrentGroupsPageView"));
        private readonly int JourneyGroupTypeId = Convert.ToInt32(AppSettings("JourneyGroupTypeId"));
        private readonly int JourneyGroupSearchPageViewId = Convert.ToInt32(AppSettings("JourneyGroupSearchPageViewId"));
        private readonly int GroupLeaderRoleId = Convert.ToInt32(AppSettings("GroupLeaderRoleId"));
        private readonly int MyCurrentGroupParticipationPageId = Convert.ToInt32(AppSettings("MyCurrentGroupParticipationPageId"));
        private readonly int NewStudentMinistryGroupAlertEmailTemplate = Convert.ToInt32(AppSettings("NewStudentMinistryGroupAlertEmailTemplate"));
        private readonly int GroupHasMiddleSchoolStudents = Convert.ToInt32(AppSettings("GroupHasMiddleSchoolStudents"));
        private readonly int GroupHasHighSchoolStudents = Convert.ToInt32(AppSettings("GroupHasHighSchoolStudents"));
       

        private readonly int GroupParticipantQualifiedServerPageView =
            Convert.ToInt32(AppSettings("GroupsParticipantsQualifiedServerPageView"));

        private IMinistryPlatformService ministryPlatformService;
        private IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        public GroupRepository(IMinistryPlatformService ministryPlatformService,
                               IMinistryPlatformRestRepository ministryPlatformRestRepository,
                               IConfigurationWrapper configurationWrapper,
                               IAuthenticationRepository authenticationService,
                               ICommunicationRepository communicationService,
                               IContactRepository contactService,
                               IContentBlockService contentBlockService,
                               IAddressRepository addressRepository,
                               IObjectAttributeRepository objectAttributeRepository
                               )
            : base(authenticationService, configurationWrapper)
        {
            this.ministryPlatformService = ministryPlatformService;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
            _configurationWrapper = configurationWrapper;
            _communicationService = communicationService;
            _contactService = contactService;
            _contentBlockService = contentBlockService;
            _addressRepository = addressRepository;
            _objectAttributeRepository = objectAttributeRepository;
        }

        public bool IsMemberOfEventGroup(int contactId, int eventId, string token)
        {
            var storedProc = _configurationWrapper.GetConfigValue("IsCampEligibleStoredProc");
            var storedProcOpts = new Dictionary<string,object>
            {
                {"@ContactId", contactId },
                {"@EventID", eventId }   
            };
            var result = _ministryPlatformRestRepository.UsingAuthenticationToken(token).GetFromStoredProc<MpStoredProcBool>(storedProc, storedProcOpts);
            if (result.Count <= 0) return false;
            var mpStoredProcBool = result.FirstOrDefault();
            return mpStoredProcBool != null 
                   && mpStoredProcBool.FirstOrDefault() != null 
                   && mpStoredProcBool.FirstOrDefault().isTrue;
        }

        public Result<MpGroupParticipant> GetGradeGroupForContact(int contactId, string apiToken)
        {
            var groupType = _configurationWrapper.GetConfigIntValue("AgeorGradeGroupType");
            var searchString = $"Participant_ID_Table_Contact_ID_Table.[Contact_ID]='{contactId}' AND Group_ID_Table_Group_Type_ID_Table.[Group_Type_ID]='{groupType}' AND Group_Participants.End_Date IS NULL";
            const string selectColumns = "Group_Participant_ID,Group_ID_Table.[Group_Name],Group_ID_Table.[Group_ID]";
            var participant = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpGroupParticipant>(searchString, selectColumns, null, true);
            if (participant.Count > 0)
            {
                if (participant.FirstOrDefault() != null && participant.FirstOrDefault().GroupName != null)
                {
                    return new Result<MpGroupParticipant>(true, participant.FirstOrDefault());
                }
                return new Result<MpGroupParticipant>(false, $"Grade group not found for contact {contactId}");
            }
            return new Result<MpGroupParticipant>(false, $"Grade group not found for contact {contactId}");
        }

        public int CreateGroup(MpGroup group)
        {
            logger.Debug("Adding group");

            var addressId = (group.Address != null) ? group.Address.Address_ID : null;
            var endDate = group.EndDate.HasValue ? (object) group.EndDate.Value : null;

            var values = new Dictionary<string, object>
            {
                {"Group_Name", group.Name},
                {"Group_Type_ID", group.GroupType},
                {"Ministry_ID", group.MinistryId},
                {"Congregation_ID", group.CongregationId},
                {"Primary_Contact", group.ContactId},
                {"Description", group.GroupDescription},
                {"Start_Date", group.StartDate},
                {"End_Date", endDate},
                {"Target_Size", group.TargetSize },
                {"Offsite_Meeting_Address", addressId },
                {"Group_Is_Full", group.Full },
                {"Available_Online", group.AvailableOnline },
                {"Meeting_Time", group.MeetingTime },
                {"Meeting_Day_Id", group.MeetingDayId },
                {"Domain_ID", 1 },
                {"Child_Care_Available", group.ChildCareAvailable },
                {"Remaining_Capacity", group.RemainingCapacity },
                {"Enable_Waiting_List", group.WaitList },
                {"Online_RSVP_Minimum_Age", group.MinimumAge },
                {"Maximum_Age", group.MaximumAge },
                {"Minimum_Participants", group.MinimumParticipants },
                {"Kids_Welcome", group.KidsWelcome },
                {"Meeting_Frequency_ID", group.MeetingFrequencyID }
                
            };

            var groupId =
                WithApiLogin<int>(
                    apiToken => { return (ministryPlatformService.CreateRecord(GroupsPageId, values, apiToken, true)); });

            logger.Debug("Added group " + groupId);
            return (groupId);
        }

        public bool GetDoesUserLeadSomeGroup(int userParticipantId)
        {
            bool doesUserLeadSomeGroup; 
            var token = ApiLogin();
            string filter = $" Group_Role_ID = {GroupLeaderRoleId} AND Participant_ID = {userParticipantId}";
            var columns = new List<string>
            {
                "Group_ID"
            };

            var records = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpGroupParticipant>(filter, columns);

            doesUserLeadSomeGroup = records.Any(); 
             
            return doesUserLeadSomeGroup; 
        }

        public int AddParticipantToGroup(int participantId,
                                         int groupId,
                                         int groupRoleId,
                                         bool childCareNeeded,
                                         bool employeeRole,
                                         DateTime startDate,
                                         DateTime? endDate = null,
                                         int? enrolledBy = null)
        {
            var token = ApiLogin();
            logger.Debug("Adding participant " + participantId + " to group " + groupId);

            var groupParticipant = new MpGroupParticipant
            {
                ParticipantId = participantId,
                GroupId = groupId,
                GroupRoleId = groupRoleId,
                ChildCareRequested = childCareNeeded,
                StartDate = startDate,
                EndDate = endDate,
                EmployeeRole = employeeRole,
                EnrolledBy = enrolledBy
            };

            var newGroupParticipant = _ministryPlatformRestRepository.UsingAuthenticationToken(token).PostWithReturn<MpGroupParticipant, MpGroupParticipant>(new List<MpGroupParticipant>() { groupParticipant }).FirstOrDefault();
            if (newGroupParticipant != null)
            {
                var groupParticipantId = newGroupParticipant.GroupParticipantId;
           

                logger.Debug("Added participant " + participantId + " to group " + groupId + ": record id: " +
                             groupParticipantId);
                return (groupParticipantId);
            }

            throw new ApplicationException("AddParticipantToGroup: Failed to Add Participant");
        }

        public void endDateGroupParticipant(int groupParticipantId, int groupId, DateTime? endDate = null)
        {
            var apiToken = ApiLogin();
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("Group_Participant_ID", groupParticipantId);
            dictionary.Add("End_Date", DateTime.Now);
            dictionary.Add("Group_ID", groupId);
            ministryPlatformService.UpdateSubRecord(_configurationWrapper.GetConfigIntValue("GroupsParticipants"), dictionary, apiToken);
        }

        public void EndDateGroup(int groupId, DateTime? endDate = null, int? reasonEndedId = null)
        {
            var apiToken = ApiLogin();
            var fields = new Dictionary<string, object>
            {
                {"Group_ID", groupId },
                {"End_Date", endDate ?? DateTime.Now }
            };

            if (reasonEndedId != null) 
                fields.Add("Reason_Ended", reasonEndedId);

            _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).UpdateRecord("Groups", groupId, fields);
        }

        public void UpdateGroupInquiry(int groupId, int inquiryId, bool approved)
        {
            var apiToken = ApiLogin();
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("Group_Inquiry_ID", inquiryId);
            dictionary.Add("Placed", approved);
            dictionary.Add("Group_ID", groupId);
            ministryPlatformService.UpdateSubRecord(_configurationWrapper.GetConfigIntValue("GroupInquiresSubPage"), dictionary, apiToken);
        }

        public MpGroup getGroupDetails(int groupId)
        {
            return (WithApiLogin<MpGroup>(apiToken =>
            {
                logger.Debug("Getting group details for group " + groupId);
                var groupDetails = ministryPlatformService.GetRecordDict(GroupsPageId, groupId, apiToken, false);
                if (groupDetails == null)
                {
                    logger.Debug("No group found for group id " + groupId);
                    return (null);
                }
                var g = new MpGroup();
                g.GroupId = groupId;

                object con = null;
                groupDetails.TryGetValue("Congregation_ID", out con);
                if (con != null)
                {
                    g.CongregationId = (int)con;
                }

                object kw = null;
                groupDetails.TryGetValue("Kids_Welcome", out kw);
                if (kw != null)
                {
                    g.KidsWelcome = (Boolean)kw;
                }

                
                object ao = null;
                groupDetails.TryGetValue("Available_Online", out ao);
                if (ao != null)
                {
                    g.AvailableOnline = (Boolean)ao;
                }

                object mt = null;
                groupDetails.TryGetValue("Meeting_Time", out mt);
                if (mt != null)
                {
                    g.MeetingTime = (string)mt.ToString();
                }

                object md = null;
                groupDetails.TryGetValue("Meeting_Day_ID", out md);
                if (md != null)
                {
                    g.MeetingDayId = (int)md;
                }

                object oma = null;
                groupDetails.TryGetValue("Offsite_Meeting_Address", out oma);
                if (oma != null)
                {
                    g.Address = _addressRepository.GetAddressById(apiToken, (int)oma);
                }

                object c = null;
                groupDetails.TryGetValue("Primary_Contact", out c);
                if (c != null)
                {
                    g.ContactId = (int)c;
                    g.PrimaryContact = c.ToString();
                }

                object mf = null;
                groupDetails.TryGetValue("Meeting_Frequency_ID", out mf);
                if (mf != null)
                {
                    g.MeetingFrequencyID = (int) mf;
                }

                object gid = null;
                groupDetails.TryGetValue("Group_ID", out gid);
                if (gid != null)
                {
                    g.GroupId = (int) gid;
                }

                object gg = null;
                groupDetails.TryGetValue("Description", out gg);
                if (gg != null)
                {
                    g.GroupDescription = (string)gg;
                }

                object gn = null;
                groupDetails.TryGetValue("Group_Name", out gn);
                if (gn != null)
                {
                    g.Name = (string) gn;
                }

                object gt = null;
                groupDetails.TryGetValue("Group_Type_ID", out gt);
                if (gt != null)
                {
                    g.GroupType = (int) gt;
                }

                object gsz = null;
                groupDetails.TryGetValue("Target_Size", out gsz);
                if (gsz != null)
                {
                    g.TargetSize = (short) gsz;
                }

                object gf = null;
                groupDetails.TryGetValue("Group_Is_Full", out gf);
                if (gf != null)
                {
                    g.Full = (Boolean) gf;
                }

                object gwl = null;
                groupDetails.TryGetValue("Enable_Waiting_List", out gwl);
                if (gwl != null)
                {
                    g.WaitList = (Boolean) gwl;
                }

                object gcc = null;
                groupDetails.TryGetValue("Child_Care_Available", out gcc);
                if (gcc != null)
                {
                    g.ChildCareAvailable = (Boolean) gcc;
                }

                object gc = null;
                groupDetails.TryGetValue("Congregation_ID_Text", out gc);
                if (gc != null)
                {
                    g.Congregation = (string) gc;
                }

                object ma = null;
                groupDetails.TryGetValue("Online_RSVP_Minimum_Age", out ma);
                if (ma != null)
                {
                    g.MinimumAge = (int) ma;
                }

                object rc = null;
                groupDetails.TryGetValue("Remaining_Capacity", out rc);
                if (rc != null)
                {
                    g.RemainingCapacity = (short) rc;
                }

                object mx = null;
                groupDetails.TryGetValue("Maximum_Age", out mx);
                if (mx != null)
                {
                    g.MaximumAge = (int) mx;
                }

                object mp = null;
                groupDetails.TryGetValue("Minimum_Participants", out mp);
                if (mp != null)
                {
                    g.MinimumParticipants = (int)mp;
                }

                object sd = null;
                groupDetails.TryGetValue("Start_Date", out sd);
                if (sd != null)
                {
                    g.StartDate = (DateTime)sd;
                }

                object mid = null;
                groupDetails.TryGetValue("Ministry_ID", out mid);
                if (mid != null)
                {
                    g.MinistryId = (int)mid;
                }

                if (g.WaitList ?? false)
                {
                    var subGroups = ministryPlatformService.GetSubPageRecords(GroupsSubgroupsPageId,
                                                                              groupId,
                                                                              apiToken);
                    if (subGroups != null)
                    {
                        foreach (var i in subGroups)
                        {
                            if (i.ContainsValue("Wait List"))
                            {
                                object gd = null;
                                i.TryGetValue("dp_RecordID", out gd);
                                g.WaitListGroupId = (int) gd;
                                break;
                            }
                        }
                    }
                    else
                    {
                        logger.Debug("No wait list found for group id " + groupId);
                    }
                }

                g.Participants = LoadGroupParticipants(groupId, apiToken);

                logger.Debug("Group details: " + g);
                return (g);
            }));
        }

        public List<MpGroupParticipant> GetGroupParticipants(int groupId, Boolean active = true)
        {
            return (WithApiLogin(apiToken => { return LoadGroupParticipants(groupId, apiToken, active); }));
        }

        public int GetParticipantIdFromGroup(int groupId, string apiToken)
        {
            const string columns = "Primary_Contact_Table.Participant_Record";
            string filter = $"[Group_ID] = {groupId}";

            logger.Debug("Getting primary contact for groupId: " + groupId);
            var participantIdDict = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpGroup, Dictionary<string, int>>(filter, columns).FirstOrDefault();

            if (participantIdDict != null && participantIdDict.Count > 0)
            {
                return participantIdDict["Participant_Record"];
            }

            throw new ApplicationException("Error finding Primary Contact ParticipantID for group");
        }

        public int UpdateGroupParticipant(List<MpGroupParticipant> participants)
        {
            int retValue = -1;

            foreach (var participant in participants)
            {
                var values = new Dictionary<string, object>
                {
                    {"Participant_ID", participant.ParticipantId},
                    {"Group_Participant_ID", participant.GroupParticipantId },
                    {"Start_Date", participant.StartDate },
                    {"Group_Role_ID", participant.GroupRoleId }
                };

                retValue = WithApiLogin<int>(token =>
                {
                    try
                    {
                        ministryPlatformService.UpdateSubRecord(GroupsParticipantsPageId, values, token);
                        return 1;
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException("Error updating group_participant: " + e.Message);
                    }
                });
            }

            return retValue;
        }

        public List<MpGroup> GetGroupsByGroupType(int groupTypeId)
        {
            var token = ApiLogin();

            const string columns = "Group_ID, Group_Name, Group_Type_ID, Offsite_Meeting_Address";
            string filter = $"Group_Type_ID = {groupTypeId} ";

            var groups = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpGroup>(filter, columns);

            foreach (var group in groups)
            {
                if (@group.OffsiteMeetingAddressId != null) @group.Address = _addressRepository.GetAddressById(token, (int) @group.OffsiteMeetingAddressId);
            }
            return groups;
        }

        public List<MpGroupSearchResult> GetSearchResults(int groupTypeId)
        {
            var apiToken = ApiLogin();
            var pageId = GetSearchPageViewId(groupTypeId);
            var records = ministryPlatformService.GetPageViewRecords(pageId, apiToken);

            return records.Select(record => new MpGroupSearchResult()
            {
                GroupId = record.ToInt("Group_ID"),
                ContactId = record.ToInt("Contact_Id"),
                Name = record.ToString("Group_Name"),
                GroupDescription = record.ToString("Description"),
                StartDate = record.ToDate("Start_Date"),
                EndDate = record.ToNullableDate("End_Date"),
                MeetingTime = record.ToString("Meeting_Time"),
                MeetingDayId = record.ToInt("Meeting_Day_ID"),
                Address = new MpAddress()
                {
                    Address_ID = record.ToInt("Address_ID"),
                    Address_Line_1 = record.ToString("Address_Line_1"),
                    Address_Line_2 = record.ToString("Address_Line_2"),
                    City = record.ToString("City"),
                    State = record.ToString("State/Region"),
                    Postal_Code = record.ToString("Postal_Code")
                },
                RemainingCapacity = record.ToInt("Remaining_Capacity"),
                PrimaryContactName = record.ToString("Last_Name") + ", " + record.ToString("Nickname"),
                // TODO: Do we need email address?
                //PrimaryContactEmail = record.ToString("PrimaryEmail")

                SearchAttributes = new MpGroupSearchAttributes()
                {
                    TypeId = record.ToNullableInt("Group_Type"),
                    GoalId = record.ToNullableInt("Group_Goal"),
                    KidsId = record.ToNullableInt("Kids"),
                    DogId = record.ToNullableInt("Has_Dog"),
                    CatId = record.ToNullableInt("Has_Cat"),
                    MeetingRangeId = record.ToNullableInt("Meeting_Range")
                }
            }).ToList();
        }

        private int GetSearchPageViewId(int groupTypeId)
        {
            if (groupTypeId == JourneyGroupTypeId)
            {
                return JourneyGroupSearchPageViewId;
            }

            var message = string.Format("Could not find matching search page for group type {0}", groupTypeId);
            throw new ArgumentException(message);
        }

        private List<MpGroupParticipant> LoadGroupParticipants(int groupId, string token, bool activeGroups = true)
        {
            const string columns = "Group_Participant_ID, Group_Participants.Group_ID, Group_Participants.Participant_ID, Group_Participants.Group_Role_ID, Group_ID_Table.Group_Name, Participant_ID_Table.Contact_ID, Participant_ID_Table_Contact_ID_Table.Nickname, Participant_ID_Table_Contact_ID_Table.Display_Name, Participant_ID_Table_Contact_ID_Table.Last_Name, Group_Role_ID_Table.Role_Title, Participant_ID_Table_Contact_ID_Table.Email_Address, Group_ID_Table_Congregation_ID_Table.Congregation_Name as Congregation, Group_Participants.Start_Date, CAST(IIF(Participant_ID_Table.Group_Leader_Status_ID = 4, 1, 0) AS bit) AS IsApprovedSmallGroupLeader";
            string filter = $"Group_Participants.Group_ID = {groupId} AND (Group_Participants.End_Date IS NULL OR Group_Participants.End_Date >= GETDATE())";

            if (activeGroups)
            {
                filter += " AND Group_ID_TABLE.Start_Date <= GETDATE() AND (Group_ID_Table.End_Date IS NULL OR Group_ID_TABLE.End_Date >= GETDATE())";
            }

            logger.Debug("Getting participants for group " + groupId);
            var groupParticipants = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpGroupParticipant>(filter, columns);

            if (!groupParticipants.Any())
            {
                logger.Debug("No participants found for group id " + groupId);
            }

            return groupParticipants;
        }

        //DE2903 : Added the date time parameter to filter out past events
        public IList<MpEvent> getAllEventsForGroup(int groupId, DateTime? minEndDate = null, bool includeCancelledEvents = false)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Group_ID", groupId);

            // optionally filter out events whose end date has passed
            if (minEndDate != null)
                parameters.Add("Min_End_Date", minEndDate.Value.Date);

            // optionally filter out cancelled events
            if (!includeCancelledEvents)
                parameters.Add("Cancelled", 0);

            string apiToken = ApiLogin();
            var resultSet = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken)
                        .GetFromStoredProc<MpEvent>("api_crds_Get_Events_For_Group", parameters);

            List<MpEvent> eventList = (resultSet != null && resultSet.Count > 0) ? resultSet[0] : null;
            return eventList;
        }

        public IList<string> GetEventTypesForGroup(int groupId, string token)
        {
            var loginToken = token ?? ApiLogin();
            var records = ministryPlatformService.GetSubpageViewRecords("GroupOpportunitiesEvents", groupId, loginToken);
            return records.Select(e =>
            {
                try
                {
                    return e.ToString("Event Type");
                }
                catch (Exception exception)
                {
                    logger.Debug("tried to parse a Event_Type_ID for a record and failed");
                    return String.Empty;
                }
            }).ToList();
        }

        public bool ParticipantQualifiedServerGroupMember(int groupId, int participantId)
        {
            var searchString = string.Format(",{0},,{1}", groupId, participantId);
            var teams = ministryPlatformService.GetPageViewRecords(GroupParticipantQualifiedServerPageView, ApiLogin(), searchString);
            return teams.Count != 0;
        }

        public bool ParticipantGroupMember(int groupId, int participantId)
        {
            return (GetParticipantGroupMemberId(groupId, participantId) > 0);
        }

        public int GetParticipantGroupMemberId(int groupId, int participantId)
        {
            object gpid = null;
            var searchString = string.Format(",{0},{1}", groupId, participantId);
            var teams = ministryPlatformService.GetPageViewRecords("GroupParticipantsById", ApiLogin(), searchString);
            if (teams.Count!=0 && teams.FirstOrDefault().TryGetValue("Group_Participant_ID", out gpid))
                return (int)gpid;
            else
                return -1;
        }

        public bool checkIfUserInGroup(int participantId, IList<MpGroupParticipant> groupParticipants)
        {
            return groupParticipants.Select(p => p.ParticipantId).Contains(participantId);
        }

        public bool checkIfRelationshipInGroup(int relationshipId, IList<int> currRelationshipsList)
        {
            return currRelationshipsList.Contains(relationshipId);
        }

        public List<MpGroupSignupRelationships> GetGroupSignupRelations(int groupType)
        {
            var response = WithApiLogin<List<MpGroupSignupRelationships>>(
                apiToken =>
                {
                    var relationRecords = ministryPlatformService.GetSubPageRecords(GroupSignupRelationsPageId,
                                                                                    groupType,
                                                                                    apiToken);

                    return relationRecords.Select(relationRecord => new MpGroupSignupRelationships
                    {
                        RelationshipId = relationRecord.ToInt("Relationship_ID"),
                        RelationshipMinAge = relationRecord.ToNullableInt("Min_Age"),
                        RelationshipMaxAge = relationRecord.ToNullableInt("Max_Age")
                    }).ToList();
                });
            return response;
        }

        public List<MpGroup> GetGroupsForEvent(int eventId)
        {
            var searchString = string.Format("\"{0}\",", eventId);
            var pageViewId = _configurationWrapper.GetConfigIntValue("GroupsByEventId");
            var token = ApiLogin();
            var records = ministryPlatformService.GetPageViewRecords(pageViewId, token, searchString);
            if (records == null)
            {
                return null;
            }
            return records.Select(record => new MpGroup
            {
                GroupId = record.ToInt("Group_ID"),
                Name = record.ToString("Group_Name")
            }).ToList();
        }

        public void SendCommunityGroupConfirmationEmail(int participantId, int groupId, bool waitlist, bool childcareNeeded)
        {
            var emailTemplate = _communicationService.GetTemplate(waitlist ? CommunityGroupWaitListConfirmationTemplateId : CommunityGroupConfirmationTemplateId);
            var toContact = _contactService.GetContactIdByParticipantId(participantId);
            var toContactInfo = _contactService.GetContactById(toContact);
            var groupInfo = getGroupDetails(groupId);

            var mergeData = new Dictionary<string, object>
            {
                {"Nickname", toContactInfo.Nickname},
                {"Group_Name", groupInfo.Name},
                {"Congregation_Name", groupInfo.Congregation},
                {"Childcare_Needed", (childcareNeeded) ? _contentBlockService.GetContent("communityGroupChildcare") : ""},
                {"Base_Url", _configurationWrapper.GetConfigValue("BaseUrl")}
            };

            var domainId = Convert.ToInt32(AppSettings("DomainId"));
            var from = new MpContact()
            {
                ContactId = DefaultEmailContactId,
                EmailAddress = _communicationService.GetEmailFromContactId(DefaultEmailContactId)
            };

            var to = new List<MpContact>
            {
                new MpContact
                {
                    ContactId = toContact,
                    EmailAddress = toContactInfo.Email_Address
                }
            };

            var confirmation = new Models.MpCommunication
            {
                EmailBody = emailTemplate.Body,
                EmailSubject = emailTemplate.Subject,
                AuthorUserId = 5,
                DomainId = domainId,
                FromContact = from,
                MergeData = mergeData,
                ReplyToContact = from,
                TemplateId = CommunityGroupConfirmationTemplateId,
                ToContacts = to
            };
            _communicationService.SendMessage(confirmation);
        }


        public void SendNewStudentMinistryGroupAlertEmail(List<MpGroupParticipant> leaders, MpGroup group)
        {
            var emailTemplate = _communicationService.GetTemplate(NewStudentMinistryGroupAlertEmailTemplate);
            const string toEmail = "studentministry@crossroads.net";

            string formattedData = "<ul> ";

            foreach (var participant in leaders)
            {
                formattedData += $"<li>Name: {participant.NickName} {participant.LastName}  Email: {participant.Email} </li>";
            }

            var mergeData = new Dictionary<string, object>
            {
                {"Leaders", formattedData + "</ul>"},
                {"GroupName", group.Name },
                {"GroupDescription", group.GroupDescription }
            };

            var domainId = Convert.ToInt32(AppSettings("DomainId"));
            var from = new MpContact()
            {
                ContactId = DefaultEmailContactId,
                EmailAddress = _communicationService.GetEmailFromContactId(DefaultEmailContactId)
            };

            var to = new List<MpContact>
            {
                new MpContact
                {
                    ContactId = _contactService.GetContactIdByEmail(toEmail),
                    EmailAddress = toEmail
                }
            };

            var newStudentMinistryGroup = new Models.MpCommunication
            {
                EmailBody = emailTemplate.Body,
                EmailSubject = emailTemplate.Subject,
                AuthorUserId = 5,
                DomainId = domainId,
                FromContact = from,
                MergeData = mergeData,
                ReplyToContact = from,
                TemplateId = NewStudentMinistryGroupAlertEmailTemplate,
                ToContacts = to
            };
            _communicationService.SendMessage(newStudentMinistryGroup);
        }

        public List<MpGroupParticipant> getEventParticipantsForGroup(int groupId, int eventId)
        {
            var records = ministryPlatformService.GetPageViewRecords("ParticipantsByGroupAndEvent", ApiLogin(), String.Format("{0},{1}", groupId, eventId));
            return records.Select(rec => new MpGroupParticipant
            {
                ContactId = rec.ToInt("Contact_ID"),
                NickName = rec.ToString("Nickname"),
                LastName = rec.ToString("Last_Name")
            }).ToList();
        }

        public List<MpGroup> GetGroupsForParticipant(string token, int participantId)
        {
            var viewId = _configurationWrapper.GetConfigIntValue("CurrentGroupParticipantsByGroupTypePageView");           
            var searchString = ",\"" + participantId + "\"";

            var records = ministryPlatformService.GetPageViewRecords(viewId, token, searchString);
            return records.Select(details => new MpGroup
            {
                GroupId = details.ToInt("Group_ID"),
                CongregationId = details.ToInt("Congregation_ID"),
                Name = details.ToString("Group_Name"),
                GroupRoleId = details.ToInt("Group_Role_ID"),
                GroupDescription = details.ToString("Description"),
                MinistryId = details.ToInt("Ministry_ID"),
                ContactId = details.ToInt("Primary_Contact"),
                PrimaryContactName = details.ToString("Primary_Contact_Name"),
                PrimaryContactEmail = details.ToString("Primary_Contact_Email"),
                GroupType = details.ToInt("Group_Type_ID"),
                StartDate = details.ToDate("Start_Date"),
                EndDate = details.ToNullableDate("End_Date"),
                MeetingDayId = details.ToInt("Meeting_Day_ID"),
                MeetingTime = details.ToString("Meeting_Time"),
                AvailableOnline = details.ToBool("Available_Online"),
                MaximumAge = details.ToInt("Maximum_Age"),
                RemainingCapacity = details.ToInt("Remaining_Capacity"),
                Address = new MpAddress()
                {
                    Address_ID = details.ToInt("Address_ID"),
                    Address_Line_1 = details.ToString("Address_Line_1"),
                    Address_Line_2 = details.ToString("Address_Line_2"),
                    City = details.ToString("City"),
                    State = details.ToString("State"),
                    Postal_Code = details.ToString("Zip_Code"),
                    Foreign_Country = details.ToString("Foreign_Country")
                }
            }).ToList();            
        }

        public List<MpGroup> GetGroupsByTypeForParticipant(string token, int participantId, int groupTypeId)
        {
            var groupDetails = ministryPlatformService.GetPageViewRecords(MyCurrentGroupsPageView, token, String.Format(",,{0}", groupTypeId));

            if (groupDetails == null || groupDetails.Count == 0)
            {
                return new List<MpGroup>();
            }
            return groupDetails.Select(MapRecordToMpGroup).ToList();
        }

        public List<MpGroup> GetGroupsForParticipantByTypeOrID(int participantId,string token = null, int[] groupTypeIds = null, int? groupId = null)
        {            
            if (token == null) token = ApiLogin();
            // Selecting * from Group_ID_Table_Offsite_Meeting_Address_Table.* due to length limitations on columns (select ) for API
            const string columns =
                "Group_ID_Table.[Primary_Contact], Group_Participants.[Group_Participant_ID], Group_Participants.[Participant_ID], Group_ID_Table.[Group_Name], Group_ID_Table.[Group_ID], Group_ID_Table.[Group_Type_ID], Group_ID_Table_Group_Type_ID_Table.[Group_Type], Group_Role_ID, Group_ID_Table.[Congregation_ID], Group_ID_Table.[Ministry_ID], Group_ID_Table.Primary_Contact, Group_ID_Table_Primary_Contact_Table.[Display_Name] AS [Primary_Contact_Name], Group_ID_Table_Primary_Contact_Table.[Email_Address] AS [Primary_Contact_Email], Group_ID_Table.[Description] AS Group_Description, Group_ID_Table.[Start_Date], Group_ID_Table.[End_Date], Group_ID_Table.[Meeting_Day_ID], Group_ID_Table_Meeting_Day_ID_Table.[Meeting_Day], Group_ID_Table_Meeting_Frequency_ID_Table.[Meeting_Frequency], Group_ID_Table.[Meeting_Time], Group_ID_Table.[Available_Online], Group_ID_Table_Offsite_Meeting_Address_Table.*, Group_ID_Table.[Maximum_Age], Group_ID_Table.[Kids_Welcome], Group_ID_Table.[Remaining_Capacity]";            

            string filter = $"Group_Participants.[Participant_ID] = {participantId} AND (GROUP_ID_TABLE.End_Date IS NULL OR GROUP_ID_TABLE.End_Date >= GetDate()) AND (Group_Participants.End_Date is NULL OR Group_Participants.End_Date >= GetDate())";

            if (groupTypeIds != null)
            {
                filter += $" AND Group_ID_Table.Group_Type_ID IN ({string.Join(",", groupTypeIds)})";
            }

            if (groupId != null)
            {
                filter += $" AND Group_ID_Table.Group_ID = {groupId}";
            }

            var groupDetails = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpGroupParticipant, MpGroup>(filter, columns);

            return groupDetails;
        }

        [Obsolete("This call has been replaced by GetMyGroupsByType() which uses the rest API.")]
        public List<MpGroup> GetMyGroupParticipationByType(string token, int[] groupTypeId = null, int? groupId = null)
        {

            var groupDetails = ministryPlatformService.GetRecordsDict(MyCurrentGroupParticipationPageId,
                                                                      token,
                                                                      string.Format(",,,{0},{1}",
                                                                                    groupId == null ? string.Empty : string.Format("\"{0}\"", groupId),
                                                                                    groupTypeId == null ? string.Empty : string.Format("\"{0}\"", string.Join("\" or \"", groupTypeId))));
            if (groupDetails == null || groupDetails.Count == 0)
            {
                return new List<MpGroup>();
            }
            return groupDetails.Select(MapRecordToMpGroup).ToList();
        }

        public void UpdateGroupRemainingCapacity(MpGroup group)
        {
            logger.Debug("Updating group: " + group.GroupId + " : " + group.Name);

            var values = new Dictionary<string, object>
            {
                {"Group_ID", group.GroupId},
                {"Remaining_Capacity", group.RemainingCapacity},
            };

            var retValue = WithApiLogin<int>(token =>
            {
                try
                {
                    ministryPlatformService.UpdateRecord(GroupsPageId, values, token);
                    return 1;
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Error updating group: " + e.Message);
                }
            });

            logger.Debug("updated group: " + group.GroupId);
        }

        public MpGroup GetSmallGroupDetailsById(int groupId)
        {
            var apiToken = ApiLogin();
            var response = ministryPlatformService.GetRecordDict(GroupsPageId, groupId, apiToken, false);
            var group = MapRecordToMpGroup(response);

            if (response.ContainsKey("Offsite_Meeting_Address") && response.ToNullableInt("Offsite_Meeting_Address") != null)
            {
                group.Address = _addressRepository.GetAddressById(apiToken, response.ToInt("Offsite_Meeting_Address"));
            }

            return group;
        }

        public MpGroup MapRecordToMpGroup(Dictionary<string, object> record)
        {
            return new MpGroup
            {
                GroupId = record.ToInt("Group_ID"),
                CongregationId = record.ToInt("Congregation_ID"),
                KidsWelcome = (record.ContainsKey("Kids_Welcome") ? record["Kids_Welcome"] as bool? : null),
                Name = record.ToString("Group_Name"),
                GroupRoleId = record.ContainsKey("Group_Role_ID") ? record.ToInt("Group_Role_ID") : -1,
                GroupDescription = record.ToString("Description"),
                MinistryId = record.ToInt("Ministry_ID"),
                ContactId = record.ToInt("Primary_Contact"),
                PrimaryContactName = record.ContainsKey("Primary_Contact_Name") ? record.ToString("Primary_Contact_Name") : record.ToString("Primary_Contact_Text"),
                PrimaryContactEmail = record.ContainsKey("Primary_Contact_Email") ? record.ToString("Primary_Contact_Email") : string.Empty,
                GroupType = record.ToInt("Group_Type_ID"),
                GroupTypeName = record.ContainsKey("Group_Type_Name") ? record.ToString("Group_Type_Name") : string.Empty, 
                StartDate = record.ToDate("Start_Date"),
                EndDate = record.ToNullableDate("End_Date"),
                MeetingDayId = record.ToInt("Meeting_Day_ID"),
                MeetingDay = (record.ContainsKey("Meeting_Day") ? record.ToString("Meeting_Day") : (record.ContainsKey("Meeting_Day_ID_Text") ? record.ToString("Meeting_Day_ID_Text") : string.Empty)),
                MeetingTime = !string.IsNullOrEmpty(record.ToString("Meeting_Time")) ? DateTime.Parse(record.ToString("Meeting_Time")).ToShortTimeString() : string.Empty,
                MeetingFrequency = (record.ContainsKey("Meeting_Frequency") ? record.ToString("Meeting_Frequency") : ((record.ContainsKey("Meeting_Frequency_ID_Text") ? record.ToString("Meeting_Frequency_ID_Text") : string.Empty))),
                AvailableOnline = record.ToBool("Available_Online"),
                MaximumAge = (record.ContainsKey("Maximum_Age") ? record["Maximum_Age"] as int? : null),
                RemainingCapacity = (record.ContainsKey("Remaining_Capacity") ? record["Remaining_Capacity"] as int? : null),
                Address = record.ContainsKey("Address_ID") ? new MpAddress()
                {
                    Address_ID = record.ToInt("Address_ID"),
                    Address_Line_1 = record.ToString("Address_Line_1"),
                    Address_Line_2 = record.ToString("Address_Line_2"),
                    City = record.ToString("City"),
                    State = record.ToString("State"),
                    Postal_Code = record.ToString("Zip_Code"),
                    Foreign_Country = record.ToString("Foreign_Country")
                } : new MpAddress()
            };
        }

        public int UpdateGroup(MpGroup group)
        {
            logger.Debug("Updating group");

            var addressId = (group.Address != null) ? group.Address.Address_ID : null;
            var endDate = group.EndDate.HasValue ? (object)group.EndDate.Value : null;

            var values = new Dictionary<string, object>
            {
                {"Group_Name", group.Name},
                {"Group_ID", group.GroupId},
                {"Group_Type_ID", group.GroupType},
                {"Ministry_ID", group.MinistryId},
                {"Congregation_ID", group.CongregationId},
                {"Primary_Contact", group.ContactId},
                {"Description", group.GroupDescription},
                {"Start_Date", group.StartDate},
                {"End_Date", endDate},
                {"Target_Size", group.TargetSize },
                {"Offsite_Meeting_Address", addressId },
                {"Group_Is_Full", group.Full },
                {"Available_Online", group.AvailableOnline },
                {"Meeting_Time", group.MeetingTime },
                {"Meeting_Day_Id", group.MeetingDayId },
                {"Domain_ID", 1 },
                {"Child_Care_Available", group.ChildCareAvailable },
                {"Remaining_Capacity", group.RemainingCapacity },
                {"Enable_Waiting_List", group.WaitList },
                {"Online_RSVP_Minimum_Age", group.MinimumAge },
                {"Maximum_Age", group.MaximumAge },
                {"Minimum_Participants", group.MinimumParticipants },
                {"Kids_Welcome", group.KidsWelcome ?? true},
                {"Meeting_Frequency_ID", group.MeetingFrequencyID }

            };

            var retValue = WithApiLogin<int>(token =>
            {
                try
                {
                    ministryPlatformService.UpdateRecord(GroupsPageId, values, token);
                    return 1;
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Error updating group: " + e.Message);
                }
            });

            logger.Debug("Updated group " + retValue);
            return (retValue);
        }

        public void CreateGroupInquiry(MpInquiry inquiry)
        {
            var token = ApiLogin();
            try
            {
                _ministryPlatformRestRepository.UsingAuthenticationToken(token).Post<MpInquiry>(new List<MpInquiry> {inquiry});
            }
            catch (Exception e)
            {
                throw new ApplicationException("Error creating group inquiry: " + e.Message);
            }
        }

        public MpGroupParticipant GetAuthenticatedUserParticipationByGroupID(string token, int groupId)
        {
            var groupDetails = ministryPlatformService.GetRecordsDict(MyCurrentGroupParticipationPageId,
                                                                      token,
                                                                      $",,,{groupId}");

            if (groupDetails == null || groupDetails.Count == 0)
                return null;

            var record = groupDetails[0];

            return new MpGroupParticipant()
            {
                GroupRoleId = record.ToInt("Group_Role_ID"),
                GroupParticipantId = record.ToInt("Group_Participant_ID"),
                ParticipantId = record.ToInt("Participant_ID")
            };
        }

        public bool ParticipantGroupHasStudents(string token, int participantId, int groupParticipantId)
        {
            var groups = GetGroupsForParticipant(token, participantId);
            var groupId = 0;
            foreach (var group in groups)
            {
                var participants = LoadGroupParticipants(group.GroupId, token);
                if (participants.Exists(x => x.GroupParticipantId == groupParticipantId))
                {
                    groupId = group.GroupId;
                    break;
                }
            }
            var configuration = MpObjectAttributeConfigurationFactory.Group();
            var attributes = _objectAttributeRepository.GetCurrentObjectAttributes(token, groupId, configuration);
            return  attributes.Find(x => x.AttributeId == GroupHasMiddleSchoolStudents
                                        || x.AttributeId == GroupHasHighSchoolStudents) != null
                                        ?true:false;
        }
    }
}

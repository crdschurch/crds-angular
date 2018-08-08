using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using crds_angular.Models.Crossroads.GroupLeader;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class GroupLeaderService : MinistryPlatformBaseService, IGroupLeaderService
    {
        private readonly IPersonService _personService;
        private readonly IUserRepository _userRepository;
        private readonly IConfigurationWrapper _configWrapper;
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IAnalyticsService _analyticsService;

        public GroupLeaderService(IPersonService personService, IUserRepository userRepository, IFormSubmissionRepository formSubmissionRepository, IParticipantRepository participantRepository, IConfigurationWrapper configWrapper, ICommunicationRepository communicationRepository, IContactRepository contactRepository, IAnalyticsService analyticsService)
        {
            _personService = personService;
            _userRepository = userRepository;
            _formSubmissionRepository = formSubmissionRepository;
            _participantRepository = participantRepository;
            _configWrapper = configWrapper;
            _communicationRepository = communicationRepository;
            _contactRepository = contactRepository;
            _analyticsService = analyticsService;
        }

        public IObservable<int> SaveReferences(GroupLeaderProfileDTO leader)
        {
            var form = new MpFormResponse
            {
                ContactId = leader.ContactId,
                FormId = _configWrapper.GetConfigIntValue("GroupLeaderFormId"),
                FormAnswers = new List<MpFormAnswer>
                {
                    new MpFormAnswer
                    {
                        FieldId = _configWrapper.GetConfigIntValue("GroupLeaderReferenceFieldId"),
                        Response = leader.ReferenceContactId
                    },
                    new MpFormAnswer
                    {
                        FieldId = _configWrapper.GetConfigIntValue("GroupLeaderReferenceNameFieldId"),
                        Response = leader.ReferenceDisplayName ?? ""
                    },
                    new MpFormAnswer
                    {
                         FieldId = _configWrapper.GetConfigIntValue("GroupLeaderHuddleFieldId"),
                        Response = leader.HuddleResponse
                    },
                    new MpFormAnswer
                    {
                         FieldId = _configWrapper.GetConfigIntValue("GroupLeaderStudentFieldId"),
                        Response = leader.LeadStudents
                    }
                }                   
            };
            return Observable.Create<int>(observer =>
            {
                var responseId = _formSubmissionRepository.SubmitFormResponse(form);
                if (responseId == 0)
                {
                   observer.OnError(new ApplicationException("Unable to submit form response for Group Leader"));
                }
                observer.OnNext(responseId);
                observer.OnCompleted();
                return Disposable.Create(() => Console.WriteLine("Observable destroyed"));
            });         
        }

        public IObservable<int> GetGroupLeaderStatus(string token)
        {
            return Observable.Create<int>(observer =>
            {
                try
                {
                    var participant = _participantRepository.GetParticipantRecord(token);
                    observer.OnNext(participant.GroupLeaderStatus);
                }
                catch (Exception e)
                {
                    observer.OnError(new ApplicationException("Failed to get Group Leader Status: ", e));
                }

                observer.OnCompleted();
                return Disposable.Empty;
            });
        }

        public IObservable<int> SetApplied(string token)
        {
            return Observable.Create<int>(observer =>
            {
                try
                {
                    var participant = _participantRepository.GetParticipantRecord(token);
                    SetGroupLeaderStatus(participant, _configWrapper.GetConfigIntValue("GroupLeaderApplied"));
                    observer.OnNext(1);
                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    observer.OnError(new ApplicationException("Unable to submit Set the participant as applied", e));
                }                
                return Disposable.Empty;
            });           
        }

        public void SetInterested(string token)
        {
            var participant = _participantRepository.GetParticipantRecord(token);
            SetGroupLeaderStatus(participant, _configWrapper.GetConfigIntValue("GroupLeaderInterested"));
        }

        public IObservable<IList<Unit>> SaveProfile(string token, GroupLeaderProfileDTO leader)
        {
            // get the current contact data....
            var currentPerson = _personService.GetLoggedInUserProfile(token);
            currentPerson.CongregationId = leader.Site;
            currentPerson.NickName = leader.NickName;
            currentPerson.LastName = leader.LastName;
            currentPerson.EmailAddress = leader.Email;
            currentPerson.DateOfBirth = leader.BirthDate.ToShortDateString();
            currentPerson.HouseholdId = leader.HouseholdId;
            currentPerson.MobilePhone = leader.MobilePhone;
            currentPerson.AddressId = leader.AddressId;

            var personDict = getDictionary(currentPerson.GetContact());
            var userUpdates = currentPerson.GetUserUpdateValues();
            var household = new MpHousehold
            {
                Address_ID = currentPerson.AddressId,
                Congregation_ID = currentPerson.CongregationId,
                Home_Phone = currentPerson.HomePhone,
                Household_ID = currentPerson.HouseholdId
            };
            try
            {
                userUpdates["User_ID"] = _userRepository.GetUserIdByUsername(leader.OldEmail);               
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to find the user account for {leader.OldEmail}", e);
            }

            return Observable.Zip(
                Observable.Start(() =>
                {
                    _contactRepository.UpdateContact(currentPerson.ContactId, personDict);                   
                    _contactRepository.UpdateHousehold(household);
                }),
                Observable.Start(() => _userRepository.UpdateUser(userUpdates)));
        }

        private void SetGroupLeaderStatus(MpParticipant participant, int statusId)
        {
            participant.GroupLeaderStatus = statusId;
            _participantRepository.UpdateParticipant(participant);
        }

        public IObservable<int> SaveSpiritualGrowth(SpiritualGrowthDTO spiritualGrowth)
        {
            var form = new MpFormResponse()
            {
                ContactId = spiritualGrowth.ContactId,
                FormId = _configWrapper.GetConfigIntValue("GroupLeaderFormId"),
                FormAnswers = new List<MpFormAnswer>
                {
                    new MpFormAnswer
                    {
                        FieldId = _configWrapper.GetConfigIntValue("GroupLeaderFormOpenResponse1FieldId"),
                        Response = spiritualGrowth.OpenResponse1
                    },
                    new MpFormAnswer
                    {
                        FieldId = _configWrapper.GetConfigIntValue("GroupLeaderFormOpenResponse2FieldId"),
                        Response = spiritualGrowth.OpenResponse2
                    },
                    new MpFormAnswer
                    {
                        FieldId = _configWrapper.GetConfigIntValue("GroupLeaderFormOpenResponse3FieldId"),
                        Response = spiritualGrowth.OpenResponse3
                    },
                    new MpFormAnswer
                    {
                        FieldId = _configWrapper.GetConfigIntValue("GroupLeaderFormOpenResponse4FieldId"),
                        Response = spiritualGrowth.OpenResponse4
                    }
                }
            };

            return Observable.Create<int>(observer =>
            {
                var responseId = _formSubmissionRepository.SubmitFormResponse(form);
                if (responseId == 0)
                {
                    observer.OnError(new ApplicationException("Unable to submit Spiritual Growth form for Group Leader"));
                }

                SendConfirmationEmail(spiritualGrowth.ContactId, spiritualGrowth.EmailAddress);
                SendGroupLeaderAppliedAnalytics(form.ContactId);
                observer.OnNext(responseId);
                observer.OnCompleted();
                return Disposable.Create(() => Console.WriteLine("Observable Destroyed"));
            });
        }

        public IObservable<Dictionary<string, object>> GetApplicationData(int contactId)
        {
            var formId = _configWrapper.GetConfigIntValue("GroupLeaderFormId");
            var referenceFormFieldId = _configWrapper.GetConfigIntValue("GroupLeaderFormReferenceContact");
            var studentLeaderFormFieldId = _configWrapper.GetConfigIntValue("GroupLeaderStudentFieldId");

            return Observable.Return<MpParticipant>(_participantRepository.GetParticipant(contactId)).Zip(
                Observable.Return<MpMyContact>(_contactRepository.GetContactById(contactId)),
                Observable.Return<string>(_formSubmissionRepository.GetFormResponseAnswer(formId, contactId, referenceFormFieldId, null)),
                Observable.Return<string>(_formSubmissionRepository.GetFormResponseAnswer(formId, contactId, studentLeaderFormFieldId, null)),
                (participant, contact, referenceAnswer, studentAnswer) => new Dictionary<string, object>
                {
                    {"participant", participant},
                    {"contact", contact},
                    {"referenceContactId", referenceAnswer ?? "0" },
                    {"studentLeaderRequest", studentAnswer ?? "false" }
                });
        }

        public IObservable<string> GetUrlSegment()
        {
            string appCode = _configWrapper.GetConfigValue("GroupLeaderAppCode");
            string paramName = _configWrapper.GetConfigValue("GroupLeaderApplicationUrlSegment");
            return Observable.Create<string>(observer =>
            {
                try
                {
                    string urlSegment = _configWrapper.GetMpConfigValue(appCode, paramName, true);
                    observer.OnNext(urlSegment);
                }
                catch (Exception e)
                {
                    observer.OnError(new ApplicationException("Failed to get redirect url segment: ", e));
                }

                observer.OnCompleted();
                return Disposable.Empty;
            });
        }

        public IObservable<int> SendStudentMinistryRequestEmail(Dictionary<string, object> referenceData)
        {
            var templateId = _configWrapper.GetConfigIntValue("GroupLeaderForStudentsEmailTemplate");
            return Observable.Create<int>(observer =>
            {
                try
                {
                    var studentMinistryId = _configWrapper.GetConfigIntValue("StudentMinistryContactId");
                    var studentMinistryEmail = _contactRepository.GetContactEmail(studentMinistryId);
                    var template = _communicationRepository.GetTemplateAsCommunication(
                        templateId,
                        studentMinistryId,
                        studentMinistryEmail,
                        SetupGenericEmailMergeData((MpMyContact)referenceData["contact"]));
                    var messageId = _communicationRepository.SendMessage(template);
                    observer.OnNext(messageId);
                }
                catch (Exception e)
                {
                    observer.OnError(new ApplicationException("Unable to send student ministry email", e));
                }
                return Disposable.Empty;
            });
        }

        private Dictionary<string, object> SetupGenericEmailMergeData(MpMyContact applicant)
        {
            return new Dictionary<string, object>
            {
                { "First_Name", applicant.Nickname ?? applicant.First_Name },
                { "Last_Name", applicant.Last_Name },
                { "Email_Address", applicant.Email_Address }
            };
        }

        private void SendGroupLeaderAppliedAnalytics(int contactId)
        {
            _analyticsService.Track(contactId.ToString(), "AppliedAsGroupLeader");
        }

        private void SendConfirmationEmail(int toContactId, string toEmailAddress)
        {         
            var templateId = _configWrapper.GetConfigIntValue("GroupLeaderConfirmationTemplate");
            var template = _communicationRepository.GetTemplate(templateId);
            var mergeData = new Dictionary<string, object> {{"Reply_To_Email", $"<a href=\"mailto:{template.ReplyToEmailAddress}\">{template.ReplyToEmailAddress}</a>"}};
            var confirmation = _communicationRepository.GetTemplateAsCommunication(templateId,
                                                                template.FromContactId,
                                                                template.FromEmailAddress,
                                                                template.ReplyToContactId,
                                                                template.ReplyToEmailAddress,
                                                                toContactId,
                                                                toEmailAddress,
                                                                mergeData);
            _communicationRepository.SendMessage(confirmation);
        }
   }
}

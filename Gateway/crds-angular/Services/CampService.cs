using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads.Camp;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Crossroads.Payment;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.FunctionalHelpers;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Product;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Auth.Models;

namespace crds_angular.Services
{
    public class CampService : ICampService
    {
        private readonly ICampRepository _campService;
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IParticipantRepository _participantRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IContactRepository _contactRepository;
        private readonly ICongregationRepository _congregationRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IEventParticipantRepository _eventParticipantRepository;
        private readonly IMedicalInformationRepository _medicalInformationRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IObjectAttributeService _objectAttributeService;
        private readonly ICampRules _campRules;
        private readonly IPaymentService _paymentService;

        private readonly ILog _logger = LogManager.GetLogger(typeof (CampService));

        public CampService(
            ICampRepository campService,
            IFormSubmissionRepository formSubmissionRepository,
            IConfigurationWrapper configurationWrapper,
            IParticipantRepository partcipantRepository,
            IEventRepository eventRepository,
            IApiUserRepository apiUserRepository,
            IContactRepository contactRepository,
            ICongregationRepository congregationRepository,
            IGroupRepository groupRepository,
            IEventParticipantRepository eventParticipantRepository,
            IMedicalInformationRepository medicalInformationRepository,
            IProductRepository productRepository,
            IInvoiceRepository invoiceRepository,
            ICommunicationRepository communicationRepository,
            IPaymentRepository paymentRepository,
            IObjectAttributeService objectAttributeService,
            ICampRules campRules,
            IPaymentService paymentService)
        {
            _campService = campService;
            _formSubmissionRepository = formSubmissionRepository;
            _configurationWrapper = configurationWrapper;
            _participantRepository = partcipantRepository;
            _eventRepository = eventRepository;
            _apiUserRepository = apiUserRepository;
            _contactRepository = contactRepository;
            _congregationRepository = congregationRepository;
            _groupRepository = groupRepository;
            _eventParticipantRepository = eventParticipantRepository;
            _medicalInformationRepository = medicalInformationRepository;
            _productRepository = productRepository;
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _communicationRepository = communicationRepository;
            _objectAttributeService = objectAttributeService;
            _campRules = campRules;
            _paymentService = paymentService;
        }

        public CampDTO GetCampEventDetails(int eventId)
        {
            var campEvent = _campService.GetCampEventDetails(eventId);

            var eligibleGradeGroups = campEvent.CampGradesList.Select(campGrade => new GroupDTO
            {
                GroupId = campGrade.GroupId, GroupName = campGrade.GroupName
            }).OrderBy(x => x.GroupName).ToList();

            var campEventInfo = new CampDTO
            {
                EventId = campEvent.EventId,
                EventTitle = campEvent.EventTitle,
                EventType = campEvent.EventType,
                StartDate = campEvent.StartDate,
                EndDate = campEvent.EndDate,
                OnlineProductId = campEvent.OnlineProductId,
                RegistrationEndDate = campEvent.RegistrationEndDate,
                RegistrationStartDate = campEvent.RegistrationStartDate,  
                ProgramId = campEvent.ProgramId,
                EligibleGradesList = eligibleGradeGroups,
                PrimaryContactEmail = campEvent.PrimaryContactEmail
            };

            return campEventInfo;
        }

        public ProductDTO GetCampProductDetails(int eventId, int camperContactId, AuthDTO token)
        {
            var formId = _configurationWrapper.GetConfigIntValue("SummerCampFormID");
            var formFieldId = _configurationWrapper.GetConfigIntValue("SummerCampForm.FinancialAssistance");
            var campEvent = _eventRepository.GetEvent(eventId);
            var eventProduct = _productRepository.GetProductForEvent(eventId);
            var invoiceDetails = _invoiceRepository.GetInvoiceDetailsForProductAndCamper(eventProduct.ProductId, camperContactId, eventId);
            var eventProductOptionPrices = _productRepository.GetProductOptionPricesForProduct(eventProduct.ProductId).OrderByDescending(m => m.DaysOutToHide).ToList();
            var answer = _formSubmissionRepository.GetFormResponseAnswer(formId, camperContactId, formFieldId, eventId);
            var financialAssistance = (!string.IsNullOrEmpty(answer) && Convert.ToBoolean(answer));
            var paymentDetail = invoiceDetails.Status ? _paymentService.GetPaymentDetails(0, invoiceDetails.Value.InvoiceId, token) : null;
            var campProductInfo = new ProductDTO
            {
                InvoiceId = invoiceDetails.Status ? invoiceDetails.Value.InvoiceId : 0,
                ProductId = eventProduct.ProductId,
                ProductName = eventProduct.ProductName,
                BasePrice = eventProduct.BasePrice,
                DepositPrice = eventProduct.DepositPrice.Value,
                Options = ConvertProductOptionPricetoDto(eventProductOptionPrices,eventProduct.BasePrice,campEvent.EventStartDate),
                BasePriceEndDate = campEvent.EventStartDate,
                FinancialAssistance = financialAssistance,
                PaymentDetail = paymentDetail
            };

            return campProductInfo;
        }

        public List<CampFamilyMember> GetEligibleFamilyMembers(int eventId, AuthDTO token)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var family = _contactRepository.GetHouseholdFamilyMembers(token.UserInfo.Mp.HouseholdId.Value);
            var me = family.Where(member => member.ContactId == token.UserInfo.Mp.ContactId).ToList();

            if ((me.First().HouseholdPosition == null || !me.First().HouseholdPosition.ToLower().StartsWith("head")) )
            {
                return me.Select(member => NewCampFamilyMember(member, eventId, apiToken)).ToList();
            }

            var otherFamily = _contactRepository.GetOtherHouseholdMembers(token.UserInfo.Mp.HouseholdId.Value);
            family.AddRange(otherFamily);
            family = family.Where((member) => member.HouseholdPosition == "Minor Child").ToList();
            return family.Select(member => NewCampFamilyMember(member, eventId, apiToken)).ToList();
        }

        private CampFamilyMember NewCampFamilyMember(MpHouseholdMember member, int eventId , string apiToken)
        {
            var cancelledStatus = _configurationWrapper.GetConfigIntValue("Participant_Status_Cancelled");
            var interestedStatus = _configurationWrapper.GetConfigIntValue("Participant_Status_Interested");

            DateTime? signedUpDate = null;
            bool isPending = false;
            bool isExpired = false;
            bool isSignedUp = false;
            bool isCancelled = false;
            DateTime? endDate = null;

            var participant = _eventParticipantRepository.GetEventParticipantEligibility(eventId, member.ContactId);
            if (participant != null)
            {
                signedUpDate = participant.SetupDate;
                isPending = participant.ParticipantStatus == interestedStatus && participant.EndDate != null && DateTime.Now <= participant.EndDate;
                isExpired = participant.ParticipantStatus == interestedStatus && participant.EndDate != null && DateTime.Now > participant.EndDate;
                isSignedUp = signedUpDate != null && participant.EndDate == null && participant.ParticipantStatus != cancelledStatus;
                isCancelled = participant.ParticipantStatus == cancelledStatus;
                endDate = participant.EndDate;
            }

            return new CampFamilyMember
            {
                ContactId = member.ContactId,
                IsEligible = isPending || signedUpDate != null ? true : _groupRepository.IsMemberOfEventGroup(member.ContactId, eventId, apiToken),
                SignedUpDate = signedUpDate,
                IsPending = isPending,
                IsExpired = isExpired,
                IsSignedUp = isSignedUp,
                IsCancelled = isCancelled,
                EndDate = endDate,
                LastName = member.LastName,
                PreferredName = member.Nickname ?? member.FirstName,

            };
        }

        public void SaveCamperEmergencyContactInfo(List<CampEmergencyContactDTO> emergencyContacts, int eventId, int contactId, AuthDTO token)
        {
            var family = _contactRepository.GetHouseholdFamilyMembers(token.UserInfo.Mp.HouseholdId.Value);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(token.UserInfo.Mp.HouseholdId.Value));

            if (family.Where(f => f.ContactId == contactId).ToList().Count <= 0)
            {
                throw new ContactNotFoundException(contactId);
            }

            var participant = _participantRepository.GetParticipant(contactId);
            var eventParticipantId = _eventRepository.SafeRegisterParticipant(eventId, participant.ParticipantId);
            var answers = new List<MpFormAnswer>();
            if (emergencyContacts.Count == 1)
            {
                var existingEmergencyContacts = GetCamperEmergencyContactInfo(eventId, contactId);
                if (existingEmergencyContacts.Count > 1)
                {
                    emergencyContacts.Add(new CampEmergencyContactDTO
                    {
                        Email = "",
                        FirstName = "",
                        LastName = "",
                        MobileNumber = "",
                        PrimaryEmergencyContact = false,
                        Relationship = ""
                    });
                }
            }

            foreach (var emergencyContact in emergencyContacts)
            {
                answers.AddRange(new List<MpFormAnswer>
                {
                    new MpFormAnswer
                    {
                        Response = emergencyContact.FirstName,
                        FieldId = emergencyContact.PrimaryEmergencyContact ? _configurationWrapper.GetConfigIntValue("SummerCampForm.EmergencyContactFirstName") : _configurationWrapper.GetConfigIntValue("SummerCampForm.AdditionalEmergencyContactFirstName"),
                        EventParticipantId = eventParticipantId
                    },
                    new MpFormAnswer
                    {
                        Response = emergencyContact.LastName,
                        FieldId = emergencyContact.PrimaryEmergencyContact ? _configurationWrapper.GetConfigIntValue("SummerCampForm.EmergencyContactLastName") : _configurationWrapper.GetConfigIntValue("SummerCampForm.AdditionalEmergencyContactLastName"),
                        EventParticipantId = eventParticipantId
                    },
                    new MpFormAnswer
                    {
                        Response = emergencyContact.MobileNumber,
                        FieldId = emergencyContact.PrimaryEmergencyContact ? _configurationWrapper.GetConfigIntValue("SummerCampForm.EmergencyContactMobilePhone") : _configurationWrapper.GetConfigIntValue("SummerCampForm.AdditionalEmergencyContactMobilePhone"),
                        EventParticipantId = eventParticipantId
                    },
                    new MpFormAnswer
                    {
                        Response = emergencyContact.Email,
                        FieldId = emergencyContact.PrimaryEmergencyContact ? _configurationWrapper.GetConfigIntValue("SummerCampForm.EmergencyContactEmail") : _configurationWrapper.GetConfigIntValue("SummerCampForm.AdditionalEmergencyContactEmail"),
                        EventParticipantId = eventParticipantId
                    },
                    new MpFormAnswer
                    {
                        Response = emergencyContact.Relationship,
                        FieldId = emergencyContact.PrimaryEmergencyContact ? _configurationWrapper.GetConfigIntValue("SummerCampForm.EmergencyContactRelationship") : _configurationWrapper.GetConfigIntValue("SummerCampForm.AdditionalEmergencyContactRelationship"),
                        EventParticipantId = eventParticipantId
                    }
                });
            }

            var formId = _configurationWrapper.GetConfigIntValue("SummerCampFormID");
            var formResponse = new MpFormResponse
            {
                ContactId = contactId,
                FormId = formId,
                FormAnswers = answers,
                EventId = eventId
            };

            _formSubmissionRepository.SubmitFormResponse(formResponse);
        }

        public CampReservationDTO SaveCampReservation(CampReservationDTO campReservation, int eventId, AuthDTO token)
        {
            var nickName = string.IsNullOrWhiteSpace(campReservation.PreferredName) ? campReservation.FirstName : campReservation.PreferredName;
            var contactId = Convert.ToInt32(campReservation.ContactId);

            var minorContact = new MpContact
            {
                FirstName = campReservation.FirstName,
                LastName = campReservation.LastName,
                PreferredName = $"{campReservation.LastName}, {campReservation.FirstName}",
                MiddleName = campReservation.MiddleName,
                MobilePhone = campReservation.MobilePhone,
                BirthDate = Convert.ToDateTime(campReservation.BirthDate),
                Gender = campReservation.Gender,
                Nickname = nickName,
                SchoolAttending = campReservation.SchoolAttending,
                HouseholdId = token.UserInfo.Mp.HouseholdId.Value,
                HouseholdPositionId = 2
            };

            MpParticipant participant;
            if (campReservation.ContactId == null || campReservation.ContactId == 0)
            {
                var newMinorContact = _contactRepository.CreateContact(minorContact);
                contactId = newMinorContact[0].RecordId;
                participant = _participantRepository.GetParticipant(contactId);
                campReservation.ContactId = contactId;
            }
            else
            {
                var updateToDictionary = new Dictionary<String, Object>
                {
                    {"Contact_ID", Convert.ToInt32(campReservation.ContactId)},
                    {"First_Name", minorContact.FirstName},
                    {"Last_Name", minorContact.LastName},
                    {"Middle_Name", minorContact.MiddleName},
                    {"Nickname", nickName},
                    {"Mobile_Phone", minorContact.MobilePhone},
                    {"Gender_ID", campReservation.Gender},
                    {"Date_Of_Birth", minorContact.BirthDate},
                    {"Current_School", minorContact.SchoolAttending},
                    {"Congregation_Name", (_congregationRepository.GetCongregationById(campReservation.CrossroadsSite)).Name}
                };

                _contactRepository.UpdateContact(Convert.ToInt32(campReservation.ContactId), updateToDictionary);
                participant = _participantRepository.GetParticipant(Convert.ToInt32(campReservation.ContactId));
            }

            // Save shirt size if set
            var configuration = MpObjectAttributeConfigurationFactory.Contact();
            _objectAttributeService.SaveObjectAttributes(contactId, campReservation.AttributeTypes, campReservation.SingleAttributes, configuration);

            // Save students in selected grade group
            var group = _groupRepository.GetGradeGroupForContact(contactId, _apiUserRepository.GetDefaultApiClientToken());

            if (group.Status && group.Value.GroupId != campReservation.CurrentGrade)
            {
                _groupRepository.endDateGroupParticipant(group.Value.GroupParticipantId, group.Value.GroupId, DateTime.Now);
                _groupRepository.AddParticipantToGroup(participant.ParticipantId,
                                                      campReservation.CurrentGrade,
                                                      _configurationWrapper.GetConfigIntValue("Group_Role_Default_ID"),
                                                      false,
                                                      false,
                                                      DateTime.Now);
            }
            else if (!group.Status)
            {
                _groupRepository.AddParticipantToGroup(participant.ParticipantId,
                                                        campReservation.CurrentGrade,
                                                        _configurationWrapper.GetConfigIntValue("Group_Role_Default_ID"),
                                                        false,
                                                        false,
                                                        DateTime.Now);
            }

            // Check if this person is already an event participant
            var eventParticipant = _eventParticipantRepository.GetEventParticipantEligibility(eventId, contactId);            
            var currentlyActive = (eventParticipant != null && (eventParticipant.EndDate == null || eventParticipant.EndDate >= DateTime.Now));
            var rulesPass = true;
            if (!currentlyActive)
            {
                rulesPass = _campRules.VerifyCampRules(eventId, campReservation.Gender);
            }

            // ALL OF THE BELOW CODE SHOULD ONLY HAPPEN IF THE RULES PASS            
            if (rulesPass)
            {
                //Create eventParticipant 
                int eventParticipantId;

                // This is a new event participant, determine their pending timeout and create their entry in the database
                if (eventParticipant == null)
                {
                    var endDate = DetermineEndDate(eventId);
                    eventParticipantId = _eventRepository.RegisterInterestedParticipantWithEndDate(participant.ParticipantId, eventId, endDate);
                }
                else
                {
                    eventParticipantId = eventParticipant.EventParticipantId;

                    // If the participant had previously started an application which expired, update its End Date now
                    if (eventParticipant.EndDate != null && eventParticipant.EndDate < DateTime.Now)
                    {
                        var endDate = DetermineEndDate(eventId);
                        _eventRepository.UpdateParticipantEndDate(eventParticipantId, endDate);
                    }
                }
                var crossroadsSite = _congregationRepository.GetCongregationById(campReservation.CrossroadsSite);

                //form response
                var answers = new List<MpFormAnswer>
                {
                    new MpFormAnswer
                    {
                        Response = campReservation.SchoolAttendingNext,
                        FieldId = _configurationWrapper.GetConfigIntValue("SummerCampForm.SchoolAttendingNextYear"),
                        EventParticipantId = eventParticipantId
                    },
                    new MpFormAnswer
                    {
                        Response = campReservation.RoomMate,
                        FieldId = _configurationWrapper.GetConfigIntValue("SummerCampForm.PreferredRoommate"),
                        EventParticipantId = eventParticipantId
                    },
                    new MpFormAnswer
                    {
                        Response = crossroadsSite.Name,
                        FieldId = _configurationWrapper.GetConfigIntValue("SummerCampForm.CamperCongregation"),
                        EventParticipantId = eventParticipantId
                    }
                };

                var formId = _configurationWrapper.GetConfigIntValue("SummerCampFormID");
                var formResponse = new MpFormResponse
                {
                    ContactId = contactId,
                    FormId = formId,
                    FormAnswers = answers,
                    EventId = eventId
                };

                _formSubmissionRepository.SubmitFormResponse(formResponse);
                return campReservation;
            }
            throw new ApplicationException("Rules do not pass!");
        }

        private DateTime DetermineEndDate(int eventId)
        {
            // Need to look up the event in order to set the event participant's end date based on the event timeout
            var camp = _eventRepository.GetEvent(eventId);
            int timeout = camp.MinutesUntilTimeout ?? _configurationWrapper.GetConfigIntValue("Event_Participant_Default_Minutes_Until_Timeout");

           return DateTime.Now.AddMinutes(timeout);
        }

        public void SetCamperAsRegistered(int eventId, int contactId)
        {
            var participant = _participantRepository.GetParticipant(contactId);
            _eventRepository.SetParticipantAsRegistered(eventId, participant.ParticipantId);
        }

        public List<MyCampDTO> GetMyCampInfo(AuthDTO token)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var campType = _configurationWrapper.GetConfigValue("CampEventTypeName");

            var dashboardData = new List<MyCampDTO>();
            
            var family = _contactRepository.GetHouseholdFamilyMembers(token.UserInfo.Mp.HouseholdId.Value);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(token.UserInfo.Mp.HouseholdId.Value));

            var cutoffDate = DateTime.Today.AddDays(-60);

            var camps = _eventRepository.GetEvents(campType, apiToken);
            foreach (var camp in camps.Where(c => c.EventEndDate >= cutoffDate))
            {
                var campers = _eventRepository.EventParticipants(apiToken, camp.EventId).ToList();
                if (campers.Any())
                {
                    foreach (var member in family)
                    {
                      if (campers.All(c => c.ContactId != member.ContactId)) continue;
                      var product = _productRepository.GetProductForEvent(camp.EventId);
                      var invoiceDetails = _invoiceRepository.GetInvoiceDetailsForProductAndCamper(product.ProductId, member.ContactId, camp.EventId);
                      var paymentDetail = !invoiceDetails.Status ? null : _paymentService.GetPaymentDetails(0, invoiceDetails.Value.InvoiceId, token);

                      dashboardData.Add(new MyCampDTO
                      {
                        CamperContactId = member.ContactId,
                        CamperNickName = member.Nickname ?? member.FirstName,
                        CamperLastName = member.LastName,
                        CampName = camp.EventTitle,
                        CampStartDate = camp.EventStartDate,
                        CampEndDate = camp.EventEndDate,
                        EventId = camp.EventId,
                        CampPrimaryContactEmail = _eventRepository.GetEvent(camp.EventId).PrimaryContact.EmailAddress,
                        CamperInvoice = paymentDetail
                      });
                    }
                }
            }

            return dashboardData;
        }

        public List<CampWaiverDTO> GetCampWaivers(int eventId, int contactId)
        {

            var waivers = _eventRepository.GetWaivers(eventId, contactId);
            return waivers.Select(waiver => new CampWaiverDTO
            {
                WaiverId = waiver.WaiverId,
                WaiverName = waiver.WaiverName,
                WaiverText = waiver.WaiverText,
                Required = waiver.Required,
                Accepted = waiver.Accepted,
                SigneeContactId = waiver.SigneeContactId
            }).ToList();
        }

        public void SaveWaivers(AuthDTO token, int eventId, int contactId, List<CampWaiverResponseDTO> waivers)
        {
            var eventParticipantId = _eventParticipantRepository.GetEventParticipantByContactId(eventId, contactId);
            var waiverResponses = waivers.Select(waiver => new MpWaiverResponse()
            {
                EventParticipantId = eventParticipantId,
                WaiverId = waiver.WaiverId,
                Accepted = waiver.WaiverAccepted,
                SigneeContactId = token.UserInfo.Mp.ContactId
            }).ToList();
            _eventRepository.SetWaivers(waiverResponses);
        }

        public void SaveInvoice(CampProductDTO campProductDto, AuthDTO token)
        {
            var family = _contactRepository.GetHouseholdFamilyMembers(token.UserInfo.Mp.HouseholdId.Value);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(token.UserInfo.Mp.HouseholdId.Value));

            if (family.Where(f => f.ContactId == campProductDto.ContactId).ToList().Count <= 0)
            {
                throw new ContactNotFoundException(campProductDto.ContactId);
            }

            // set finainacial assistance flag in form response
            var participant = _participantRepository.GetParticipant(campProductDto.ContactId);
            var eventParticipantId = _eventRepository.GetEventParticipantRecordId(campProductDto.EventId, participant.ParticipantId);

            var answers = new List<MpFormAnswer>
            {
                new MpFormAnswer
                {
                    Response = campProductDto.FinancialAssistance.ToString(),
                    FieldId = _configurationWrapper.GetConfigIntValue("SummerCampForm.FinancialAssistance"),
                    EventParticipantId = eventParticipantId
                }
            };

            var formId = _configurationWrapper.GetConfigIntValue("SummerCampFormID");
            var formResponse = new MpFormResponse
            {
                ContactId = campProductDto.ContactId,
                FormId = formId,
                FormAnswers = answers,
                EventId = campProductDto.EventId
            };

            _formSubmissionRepository.SubmitFormResponse(formResponse);
            var campEvent = _eventRepository.GetEvent(campProductDto.EventId);
            var product = _productRepository.GetProductForEvent(campProductDto.EventId);
            var optionPrices = _productRepository.GetProductOptionPricesForProduct(product.ProductId);
            var productOptionPriceId = optionPrices.Count > 0 ? 
                ConvertProductOptionPricetoDto(optionPrices, product.BasePrice, campEvent.EventStartDate)
                    .Where(i => i.EndDate > DateTime.Now)
                    .OrderBy(i => i.EndDate).FirstOrDefault()?
                    .ProductOptionPriceId 
                : (int?)null;
            var invoiceDetail = _invoiceRepository.GetInvoiceDetailsForProductAndCamper(product.ProductId,campProductDto.ContactId, campProductDto.EventId);
            if (invoiceDetail.Status)
            {
              _invoiceRepository.UpdateInvoiceAndDetail(invoiceDetail.Value.InvoiceId, product, productOptionPriceId, token.UserInfo.Mp.ContactId, campProductDto.ContactId, eventParticipantId); 
            }
            else
            {
              _invoiceRepository.CreateInvoiceAndDetail(product.ProductId, productOptionPriceId, token.UserInfo.Mp.ContactId, campProductDto.ContactId, eventParticipantId);
            }
            
        }

        public bool SendCampConfirmationEmail(int eventId, int invoiceId, int paymentId)
        {
            var baseUrl = _configurationWrapper.GetConfigValue("BaseUrl");
            var templateId = _configurationWrapper.GetConfigIntValue("CampConfirmationEmailTemplate");
            var mpEvent = _eventRepository.GetEvent(eventId);
            var payments = _paymentRepository.GetPaymentsForInvoice(invoiceId);
            var thisPayment = payments.Where(p => p.PaymentId == paymentId).ToList().FirstOrDefault();

            if (thisPayment != null)
            {
                var from = _contactRepository.GetContactById(mpEvent.PrimaryContactId);
                var recipient = _contactRepository.GetContactById(thisPayment.ContactId);

                var mergeData = new Dictionary<string, object>
                {
                    {"BASE_URL", baseUrl },
                    {"EVENT_URL", mpEvent.RegistrationURL },
                    {"EVENT_START_DATE", mpEvent.EventStartDate.ToShortDateString()},
                    {"EVENT_END_DATE", mpEvent.EventEndDate.ToShortDateString()},
                    {"EVENT_TITLE", mpEvent.EventTitle},
                    {"PAYMENT_AMOUNT", $"${thisPayment.PaymentTotal.ToString("0.00")}"},
                    {"DISPLAY_NAME", $"{from.First_Name} {from.Last_Name}" },
                    {"EMAIL_ADDRESS", mpEvent.PrimaryContact.EmailAddress }
                };

                var template = _communicationRepository.GetTemplateAsCommunication(templateId,
                                                                                   mpEvent.PrimaryContactId,
                                                                                   mpEvent.PrimaryContact.EmailAddress,
                                                                                   mpEvent.PrimaryContactId,
                                                                                   mpEvent.PrimaryContact.EmailAddress,
                                                                                   recipient.Contact_ID,
                                                                                   recipient.Email_Address,
                                                                                   mergeData);
                var result = _communicationRepository.SendMessage(template);
                return result > 0;
            }
            else
            {
                throw new PaymentTypeNotFoundException(paymentId);
            }
        }

        public void SaveCamperMedicalInfo(MedicalInfoDTO medicalInfo, int contactId, AuthDTO token)
        {
            var family = _contactRepository.GetHouseholdFamilyMembers(token.UserInfo.Mp.HouseholdId.Value);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(token.UserInfo.Mp.HouseholdId.Value));

            if (family.Where(f => f.ContactId == contactId).ToList().Count <= 0)
            {
                throw new ContactNotFoundException(contactId);
            }
            if (medicalInfo != null)
            {
                var mpMedicalInfo = new MpMedicalInformation
                {
                    MedicalInformationId = medicalInfo.MedicalInformationId,
                    ContactId = contactId,
                    InsuranceCompany = medicalInfo.InsuranceCompany ?? "N/A",
                    PhysicianName = medicalInfo.PhysicianName ?? "N/A",
                    PhysicianPhone = medicalInfo.PhysicianPhone ?? "N/A",
                    PolicyHolder = medicalInfo.PolicyHolder ?? "N/A",
                };

                var meds = medicalInfo.MedicationsAdministered?.Where(med => med != null).ToList();
                mpMedicalInfo.MedicationsAdministered = meds.Any() ? meds?.Aggregate((c, n) => c + "," + n) : string.Empty;

                var medicalInformation =  _medicalInformationRepository.SaveMedicalInfo(mpMedicalInfo, contactId);
                var updateToDictionary = new Dictionary<String, Object>
                {
                    {"Contact_ID", contactId},
                    {"Medicalinformation_ID",medicalInformation.MedicalInformationId}
                };
                _contactRepository.UpdateContact(contactId, updateToDictionary);

                var updateToAllergyList = new List<MpMedicalAllergy>();
                var createToAllergyList = new List<MpMedicalAllergy>();
                foreach (var allergy in medicalInfo.Allergies)
                {
                    if (allergy.AllergyId != 0)
                    {
                        updateToAllergyList.Add(new MpMedicalAllergy
                        {
                            Allergy = new MpAllergy {
                                AllergyID = allergy.AllergyId,
                                AllergyType = allergy.AllergyTypeId,
                                AllergyDescription = allergy.AllergyDescription
                            },
                            MedicalInformationId = medicalInformation.MedicalInformationId,
                            MedicalInfoAllergyId = allergy.MedicalInformationAllergyId
                        });
                    }
                    else if (!string.IsNullOrEmpty(allergy.AllergyDescription))
                    {
                        createToAllergyList.Add(new MpMedicalAllergy
                        {
                            Allergy = new MpAllergy
                            {
                                AllergyType = GetAllergyType(allergy.AllergyType),
                                AllergyDescription = allergy.AllergyDescription
                            },
                            MedicalInformationId = medicalInformation.MedicalInformationId,
                            MedicalInfoAllergyId = allergy.MedicalInformationAllergyId
                        });
                    }
                }
                _medicalInformationRepository.UpdateOrCreateMedAllergy(updateToAllergyList, createToAllergyList);
                _medicalInformationRepository.UpdateOrCreateMedications(medicalInfo.Medications.Select(m => new MpMedication {MedicalInformationMedicationId = m.MedicalInformationMedicationId, MedicalInformationId = medicalInformation.MedicalInformationId, MedicationName = m.MedicationName, MedicationTypeId = m.MedicationTypeId, DosageAmount = m.Dosage, DosageTimes = m.TimesOfDay, Deleted = m.Deleted}).ToList());
            }
        }



        public MedicalInfoDTO GetCampMedicalInfo(int eventId, int contactId, AuthDTO token)
        {
            var family = _contactRepository.GetHouseholdFamilyMembers(token.UserInfo.Mp.HouseholdId.Value);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(token.UserInfo.Mp.HouseholdId.Value));

            if (family.Where(f => f.ContactId == contactId).ToList().Count <= 0)
            {
                throw new ContactNotFoundException(contactId);
            }
            var camperMed = _medicalInformationRepository.GetMedicalInformation(contactId);
            if (camperMed == null)
            {
                return new MedicalInfoDTO();
            }

            var allergies = _medicalInformationRepository.GetMedicalAllergyInfo(contactId);
            var medications = _medicalInformationRepository.GetMedications(contactId);

            var camperMedInfo = new MedicalInfoDTO
            {
                ContactId = contactId,
                MedicalInformationId = camperMed.MedicalInformationId,
                InsuranceCompany = camperMed.InsuranceCompany=="N/A"? null :camperMed.InsuranceCompany,
                PolicyHolder = camperMed.PolicyHolder == "N/A"? null : camperMed.PolicyHolder,
                PhysicianName = camperMed.PhysicianName == "N/A" ? null : camperMed.PhysicianName,
                PhysicianPhone = camperMed.PhysicianPhone == "N/A" ? null : camperMed.PhysicianPhone,
                MedicationsAdministered = camperMed.MedicationsAdministered?.Split(',').ToList() ?? new List<string>()
            };
            camperMedInfo.Allergies = new List<Allergy>();
            foreach (var medInfo in allergies )
            {
                if (medInfo.AllergyType != string.Empty)
                {
                    var allergy = new Allergy
                    {
                        MedicalInformationAllergyId = medInfo.MedicalInfoAllergyId,
                        AllergyDescription = medInfo.AllergyDescription,
                        AllergyType = medInfo.AllergyType,
                        AllergyTypeId = medInfo.AllergyTypeId,
                        AllergyId = medInfo.AllergyId
                    };
                    camperMedInfo.Allergies.Add(allergy);
                }
            }
            if (camperMedInfo.Allergies.Count > 0) { camperMedInfo.ShowAllergies = true; }

            camperMedInfo.Medications = new List<Medication>();
            foreach (var medication in medications)
            {
                camperMedInfo.Medications.Add(new Medication
                {
                    MedicalInformationMedicationId = medication.MedicalInformationMedicationId,
                    MedicationName = medication.MedicationName,
                    MedicationTypeId = medication.MedicationTypeId,
                    Dosage = medication.DosageAmount,
                    TimesOfDay = medication.DosageTimes
                });
            }
            if (camperMedInfo.Medications.Count > 0) { camperMedInfo.ShowMedications = true; }

            return camperMedInfo;
        }

        public List<CampEmergencyContactDTO> GetCamperEmergencyContactInfo(int eventId, int contactId)
        {
            var formId = _configurationWrapper.GetConfigIntValue("SummerCampFormID");
            var response = _formSubmissionRepository.GetFormResponse(formId, contactId, eventId);
            var emergencyContacts = new List<CampEmergencyContactDTO>();
            emergencyContacts.Add(new CampEmergencyContactDTO
            {
                Email = response.FormAnswers.FirstOrDefault(a => a.FieldId == _configurationWrapper.GetConfigIntValue("SummerCampForm.EmergencyContactEmail"))?.Response,
                FirstName = response.FormAnswers.FirstOrDefault(a => a.FieldId == _configurationWrapper.GetConfigIntValue("SummerCampForm.EmergencyContactFirstName"))?.Response,
                LastName = response.FormAnswers.FirstOrDefault(a => a.FieldId == _configurationWrapper.GetConfigIntValue("SummerCampForm.EmergencyContactLastName"))?.Response,
                MobileNumber = response.FormAnswers.FirstOrDefault(a => a.FieldId == _configurationWrapper.GetConfigIntValue("SummerCampForm.EmergencyContactMobilePhone"))?.Response,
                PrimaryEmergencyContact = true,
                Relationship = response.FormAnswers.FirstOrDefault(a => a.FieldId == _configurationWrapper.GetConfigIntValue("SummerCampForm.EmergencyContactRelationship"))?.Response
            });

            emergencyContacts.Add(new CampEmergencyContactDTO
            {
                Email = response.FormAnswers.FirstOrDefault(a => a.FieldId == _configurationWrapper.GetConfigIntValue("SummerCampForm.AdditionalEmergencyContactEmail"))?.Response,
                FirstName = response.FormAnswers.FirstOrDefault(a => a.FieldId == _configurationWrapper.GetConfigIntValue("SummerCampForm.AdditionalEmergencyContactFirstName"))?.Response,
                LastName = response.FormAnswers.FirstOrDefault(a => a.FieldId == _configurationWrapper.GetConfigIntValue("SummerCampForm.AdditionalEmergencyContactLastName"))?.Response,
                MobileNumber = response.FormAnswers.FirstOrDefault(a => a.FieldId == _configurationWrapper.GetConfigIntValue("SummerCampForm.AdditionalEmergencyContactMobilePhone"))?.Response,
                PrimaryEmergencyContact = false,
                Relationship = response.FormAnswers.FirstOrDefault(a => a.FieldId == _configurationWrapper.GetConfigIntValue("SummerCampForm.AdditionalEmergencyContactRelationship"))?.Response
            });

            return emergencyContacts;
        }

        public CampReservationDTO GetCamperInfo(AuthDTO token, int eventId, int contactId)
        {
            var family = _contactRepository.GetHouseholdFamilyMembers(token.UserInfo.Mp.HouseholdId.Value);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(token.UserInfo.Mp.HouseholdId.Value));

            if (family.Where(f => f.ContactId == contactId).ToList().Count <= 0)
            {
                return null;
            }
            var camperContact = _contactRepository.GetContactById(contactId);

            var apiToken = _apiUserRepository.GetDefaultApiClientToken();

            // get camper grade if they have one
            var groupResult = _groupRepository.GetGradeGroupForContact(contactId, apiToken);

            var campFormId = _configurationWrapper.GetConfigIntValue("SummerCampFormID");
            var nextYearSchoolFormFieldId = _configurationWrapper.GetConfigIntValue("SummerCampForm.SchoolAttendingNextYear");
            var nextYearSchool = _formSubmissionRepository.GetFormResponseAnswer(campFormId, camperContact.Contact_ID, nextYearSchoolFormFieldId, eventId);

            var preferredRoommateFieldId = _configurationWrapper.GetConfigIntValue("SummerCampForm.PreferredRoommate");
            var preferredRoommate = _formSubmissionRepository.GetFormResponseAnswer(campFormId, camperContact.Contact_ID, preferredRoommateFieldId, eventId);
            var crossroadsSiteFieldId = _configurationWrapper.GetConfigIntValue("SummerCampForm.CamperCongregation");
            var crossroadsSite = _formSubmissionRepository.GetFormResponseAnswer(campFormId, camperContact.Contact_ID, crossroadsSiteFieldId, eventId);

            var congregation = (string.IsNullOrEmpty(crossroadsSite))
                ? new Err<MpCongregation>("Congregation not set")
                : _congregationRepository.GetCongregationByName(crossroadsSite, apiToken);
            var configuration = MpObjectAttributeConfigurationFactory.Contact();
            var attributesTypes = _objectAttributeService.GetObjectAttributes(contactId, configuration);

            return new CampReservationDTO
            {
                ContactId = camperContact.Contact_ID,
                FirstName = camperContact.First_Name,
                LastName = camperContact.Last_Name,
                MiddleName = camperContact.Middle_Name,
                PreferredName = camperContact.Nickname,
                MobilePhone = camperContact.Mobile_Phone,
                CrossroadsSite = congregation.Status ? congregation.Value.CongregationId : 0,
                BirthDate = Convert.ToString(camperContact.Date_Of_Birth),
                SchoolAttending = camperContact.Current_School,
                SchoolAttendingNext = nextYearSchool,
                Gender = Convert.ToInt32(camperContact.Gender_ID),
                CurrentGrade = groupResult.Status ? groupResult.Value.GroupId : 0,
                RoomMate = preferredRoommate,
                AttributeTypes = attributesTypes.MultiSelect,
                SingleAttributes = attributesTypes.SingleSelect
            };
        }        

        private int GetAllergyType(String type)
        {
            int ret = 0;
            switch (type)
            {
                case "Medicine":
                    ret = _configurationWrapper.GetConfigIntValue("MedicineAllergyType");
                    break;
                case "Environmental":
                    ret = _configurationWrapper.GetConfigIntValue("EnvironmentalAllergyType");
                    break;
                case "Other":
                    ret = _configurationWrapper.GetConfigIntValue("OtherAllergyType");
                    break;
                case "Food":
                    ret = _configurationWrapper.GetConfigIntValue("FoodAllergyType");
                    break;
            }
            return ret;
        }

        private static List<ProductOptionDTO> ConvertProductOptionPricetoDto(List<MpProductOptionPrice> options, decimal basePrice, DateTime registrationEnd)
        {

            return options.Select(option => new ProductOptionDTO
                                  {
                                      ProductOptionPriceId = option.ProductOptionPriceId,
                                      OptionTitle = option.OptionTitle,
                                      OptionPrice = option.OptionPrice,
                                      DaysOutToHide = option.DaysOutToHide,
                                      TotalWithOptionPrice = basePrice + option.OptionPrice,
                                      EndDate = option.DaysOutToHide!= null ? registrationEnd.AddDays(Convert.ToDouble(option.DaysOutToHide) * -1) : (DateTime?)null
            }).ToList();
        }
    }
}

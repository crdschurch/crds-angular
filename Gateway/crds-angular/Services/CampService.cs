using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads.Camp;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using log4net;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Product;
using MinistryPlatform.Translation.Repositories.Interfaces;

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
            IInvoiceRepository invoiceRepository)
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
        }

        public CampDTO GetCampEventDetails(int eventId)
        {
            var campEvent = _campService.GetCampEventDetails(eventId);
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
                ProgramId = campEvent.ProgramId
            };

            return campEventInfo;
        }

        public ProductDTO GetCampProductDetails(int eventId, int camperContactId, string token)
        {
            var formId = _configurationWrapper.GetConfigIntValue("SummerCampFormID");
            var formFieldId = _configurationWrapper.GetConfigIntValue("SummerCampForm.FinancialAssistance");
            var me = _contactRepository.GetMyProfile(token);
            var campEvent = _eventRepository.GetEvent(eventId);
            var eventProduct = _productRepository.GetProductForEvent(eventId);
            var eventProductOptionPrices = _productRepository.GetProductOptionPricesForProduct(eventProduct.ProductId).OrderByDescending(m => m.DaysOutToHide).ToList();
            var invoiceDetails = _invoiceRepository.GetInvoiceDetailsForProductAndCamperAndContact(eventProduct.ProductId, camperContactId, me.Contact_ID);
            var answer = _formSubmissionRepository.GetFormResponseAnswer(formId, camperContactId, formFieldId);
            var financialAssistance = (string.IsNullOrEmpty(answer) ? false : Convert.ToBoolean(answer));

            var campProductInfo = new ProductDTO
            {
                InvoiceId = invoiceDetails.Status ? invoiceDetails.Value.InvoiceId : 0,
                ProductId = eventProduct.ProductId,
                ProductName = eventProduct.ProductName,
                BasePrice = eventProduct.BasePrice,
                DepositPrice = eventProduct.DepositPrice,
                Options = ConvertProductOptionPricetoDto(eventProductOptionPrices,eventProduct.BasePrice,campEvent.EventStartDate),
                BasePriceEndDate = campEvent.EventStartDate,
                FinancialAssistance = financialAssistance
            };

            return campProductInfo;
        }

        

        public List<CampFamilyMember> GetEligibleFamilyMembers(int eventId, string token)
        {
            var apiToken = _apiUserRepository.GetToken();
            var myContact = _contactRepository.GetMyProfile(token);            
            var family = _contactRepository.GetHouseholdFamilyMembers(myContact.Household_ID);            
            var me = family.Where(member => member.ContactId == myContact.Contact_ID).ToList();

            if ((me.First().HouseholdPosition == null || !me.First().HouseholdPosition.ToLower().StartsWith("head")) )
            {
                return me.Select(member => new CampFamilyMember
                {
                    ContactId = member.ContactId,
                    IsEligible = _groupRepository.IsMemberOfEventGroup(member.ContactId, eventId, apiToken),
                    SignedUpDate = _eventParticipantRepository.EventParticipantSignupDate(member.ContactId, eventId, apiToken),
                    LastName = member.LastName,
                    PreferredName = member.Nickname ?? member.FirstName
                }).ToList();
            } 

            var otherFamily = _contactRepository.GetOtherHouseholdMembers(myContact.Contact_ID);
            family.AddRange(otherFamily);
            family = family.Where((member) => member.HouseholdPosition == "Minor Child").ToList();
            return family.Select(member => new CampFamilyMember()
            {
                ContactId = member.ContactId,
                IsEligible = _groupRepository.IsMemberOfEventGroup(member.ContactId, eventId, apiToken),
                SignedUpDate = _eventParticipantRepository.EventParticipantSignupDate(member.ContactId, eventId, apiToken),
                LastName = member.LastName,
                PreferredName = member.Nickname ?? member.FirstName
            }).ToList();
        }

        public void SaveCamperEmergencyContactInfo(List<CampEmergencyContactDTO> emergencyContacts, int eventId, int contactId, string token)
        {
            var loggedInContact = _contactRepository.GetMyProfile(token);
            var family = _contactRepository.GetHouseholdFamilyMembers(loggedInContact.Household_ID);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(loggedInContact.Contact_ID));

            if (family.Where(f => f.ContactId == contactId).ToList().Count <= 0)
            {
                throw new ContactNotFoundException(contactId);
            }

            var participant = _participantRepository.GetParticipant(contactId);
            var eventParticipantId = _eventRepository.SafeRegisterParticipant(eventId, participant.ParticipantId);
            var answers = new List<MpFormAnswer>();

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
                FormAnswers = answers
            };

            _formSubmissionRepository.SubmitFormResponse(formResponse);
        }

        public void SaveCampReservation(CampReservationDTO campReservation, int eventId, string token)
        {
            var nickName = campReservation.PreferredName ?? campReservation.FirstName;
            MpParticipant participant;
            var contactId = Convert.ToInt32(campReservation.ContactId);

            var minorContact = new MpContact
            {
                FirstName = campReservation.FirstName,
                LastName = campReservation.LastName,
                MiddleName = campReservation.MiddleName,
                BirthDate = Convert.ToDateTime(campReservation.BirthDate),
                Gender = campReservation.Gender,
                Nickname = nickName,
                SchoolAttending = campReservation.SchoolAttending,
                HouseholdId = (_contactRepository.GetMyProfile(token)).Household_ID,
                HouseholdPositionId = 2
            };

            if (campReservation.ContactId == null || campReservation.ContactId == 0)
            {
                var newMinorContact = _contactRepository.CreateContact(minorContact);
                contactId = newMinorContact[0].RecordId;
                participant = _participantRepository.GetParticipant(contactId);
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
                    {"Gender_ID", campReservation.Gender},
                    {"Date_Of_Birth", minorContact.BirthDate},
                    {"Current_School", minorContact.SchoolAttending},
                    {"Congregation_Name", (_congregationRepository.GetCongregationById(campReservation.CrossroadsSite)).Name}
                };

                _contactRepository.UpdateContact(Convert.ToInt32(campReservation.ContactId), updateToDictionary);
                participant = _participantRepository.GetParticipant(Convert.ToInt32(campReservation.ContactId));
            }

            int eventParticipantId = _eventRepository.GetEventParticipantRecordId(eventId, participant.ParticipantId);
            if (eventParticipantId == 0)
            {
                eventParticipantId = _eventRepository.RegisterParticipantForEvent(participant.ParticipantId, eventId);
            }
            else
            {
                _logger.Error("The person is already an event participant");
            }


            //form response
            var answers = new List<MpFormAnswer>
            {
                new MpFormAnswer
                {
                    Response = campReservation.CurrentGrade,
                    FieldId = _configurationWrapper.GetConfigIntValue("SummerCampForm.CurrentGrade"),
                    EventParticipantId = eventParticipantId
                },
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
                }
            };

            var formId = _configurationWrapper.GetConfigIntValue("SummerCampFormID");
            var formResponse = new MpFormResponse
            {
                ContactId = contactId,
                FormId = formId,
                FormAnswers = answers
            };

            _formSubmissionRepository.SubmitFormResponse(formResponse);
        }

        public List<MyCampDTO> GetMyCampInfo(string token)
        {
            var apiToken = _apiUserRepository.GetToken();
            var campType = _configurationWrapper.GetConfigValue("CampEventTypeName");

            var dashboardData = new List<MyCampDTO>();

            var loggedInContact = _contactRepository.GetMyProfile(token);
            var family = _contactRepository.GetHouseholdFamilyMembers(loggedInContact.Household_ID);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(loggedInContact.Contact_ID));

            var camps = _eventRepository.GetEvents(campType, apiToken);
            foreach (var camp in camps.Where(c => c.EventEndDate >= DateTime.Today))
            {
                var campers = _eventRepository.EventParticipants(apiToken, camp.EventId).ToList();
                if (campers.Any())
                {
                    foreach (var member in family)
                    {
                        if (campers.Any(c => c.ContactId == member.ContactId))
                        {
                            dashboardData.Add(new MyCampDTO
                            {
                                CamperContactId = member.ContactId,
                                CamperNickName = member.Nickname ?? member.FirstName,
                                CamperLastName = member.LastName,
                                CampName = camp.EventTitle,
                                CampStartDate = camp.EventStartDate,
                                CampEndDate = camp.EventEndDate,
                                EventId = camp.EventId
                            });
                        }
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

        public void SaveWaivers(string token, int eventId, int contactId, List<CampWaiverResponseDTO> waivers)
        {
            var loggedInContact = _contactRepository.GetMyProfile(token);
            var eventParticipantId = _eventParticipantRepository.GetEventParticipantByContactId(eventId, contactId);
            var waiverResponses = waivers.Select(waiver => new MpWaiverResponse()
            {
                EventParticipantId = eventParticipantId,
                WaiverId = waiver.WaiverId,
                Accepted = waiver.WaiverAccepted,
                SigneeContactId = loggedInContact.Contact_ID
            }).ToList();
            _eventRepository.SetWaivers(waiverResponses);
        }

        public void SaveInvoice(CampProductDTO campProductDto, string token)
        {
            var loggedInContact = _contactRepository.GetMyProfile(token);
            var family = _contactRepository.GetHouseholdFamilyMembers(loggedInContact.Household_ID);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(loggedInContact.Contact_ID));

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
                FormAnswers = answers
            };

            _formSubmissionRepository.SubmitFormResponse(formResponse);

            // if an invoice exists for this eventparticipant then don't create a new one
            if (_invoiceRepository.InvoiceExistsForEventParticipant(eventParticipantId)) return;

            // create the invoice with product from event and best pricing for the current date
            //get the product id for this event
            var campEvent = _eventRepository.GetEvent(campProductDto.EventId);
            var product = _productRepository.GetProductForEvent(campProductDto.EventId);
            var optionPrices = _productRepository.GetProductOptionPricesForProduct(product.ProductId);
            //find current option price (if any)
            var productOptionPriceId = optionPrices.Count > 0 ? ConvertProductOptionPricetoDto(optionPrices, product.BasePrice, campEvent.EventStartDate).Where(i => i.EndDate > DateTime.Now).OrderByDescending(i => i.EndDate).FirstOrDefault()?.ProductOptionPriceId : (int?)null;

            _invoiceRepository.CreateInvoiceAndDetail(product.ProductId, productOptionPriceId, loggedInContact.Contact_ID, campProductDto.ContactId, eventParticipantId);
        }

        public void SaveCamperMedicalInfo(MedicalInfoDTO medicalInfo, int contactId, string token)
        {
            var loggedInContact = _contactRepository.GetMyProfile(token);
            var family = _contactRepository.GetHouseholdFamilyMembers(loggedInContact.Household_ID);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(loggedInContact.Contact_ID));

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
                    PolicyHolder = medicalInfo.PolicyHolder ?? "N/A"
                };
                var medicalInformation =  _medicalInformationRepository.SaveMedicalInfo(mpMedicalInfo, contactId);
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
                                AllergyType = new MpAllergyType {
                                    AllergyType = allergy.AllergyType,
                                    AllergyTypeID = allergy.AllergyTypeId
                                },
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
                                AllergyType = new MpAllergyType {
                                    AllergyType = allergy.AllergyType,
                                    AllergyTypeID = allergy.AllergyTypeId
                                },
                                AllergyDescription = allergy.AllergyDescription
                            },
                            MedicalInformationId = medicalInformation.MedicalInformationId,
                            MedicalInfoAllergyId = allergy.MedicalInformationAllergyId
                        });
                    }
                }
                _medicalInformationRepository.UpdateOrCreateMedAllergy(updateToAllergyList, createToAllergyList);
            }           
        }

        public MedicalInfoDTO GetCampMedicalInfo(int eventId, int contactId, string token)
        {
            var loggedInContact = _contactRepository.GetMyProfile(token);
            var family = _contactRepository.GetHouseholdFamilyMembers(loggedInContact.Household_ID);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(loggedInContact.Contact_ID));

            if (family.Where(f => f.ContactId == contactId).ToList().Count <= 0)
            {
                throw new ContactNotFoundException(contactId);
            }
            var camperMed = _medicalInformationRepository.GetMedicalInformation(contactId);
            if (camperMed == null) { return null; }

            var allergies = _medicalInformationRepository.GetMedicalAllergyInfo(contactId);
            
            var camperMedInfo = new MedicalInfoDTO
            {
                ContactId = contactId,
                MedicalInformationId = camperMed.MedicalInformationId,              
                InsuranceCompany = camperMed.InsuranceCompany,
                PolicyHolder = camperMed.PolicyHolder,
                PhysicianName = camperMed.PhysicianName,
                PhysicianPhone = camperMed.PhysicianPhone
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
            return camperMedInfo;
        }

        public List<CampEmergencyContactDTO> GetCamperEmergencyContactInfo(int eventId, int contactId, string token)
        {
            var formId = _configurationWrapper.GetConfigIntValue("SummerCampFormID");
            var response = _formSubmissionRepository.GetFormResponse(formId, contactId);
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

        public CampReservationDTO GetCamperInfo(string token, int eventId, int contactId)
        {
            var loggedInContact = _contactRepository.GetMyProfile(token);
            var family = _contactRepository.GetHouseholdFamilyMembers(loggedInContact.Household_ID);
            family.AddRange(_contactRepository.GetOtherHouseholdMembers(loggedInContact.Contact_ID));

            if (family.Where(f => f.ContactId == contactId).ToList().Count <= 0)
            {
                return null;
            }
            var camperContact = _contactRepository.GetContactById(contactId);

            var apiToken = _apiUserRepository.GetToken();

            // get camper grade if they have one
            var groupResult = _groupRepository.GetGradeGroupForContact(contactId, apiToken);

            return new CampReservationDTO
            {
                ContactId = camperContact.Contact_ID,
                FirstName = camperContact.First_Name,
                LastName = camperContact.Last_Name,
                MiddleName = camperContact.Middle_Name,
                PreferredName = camperContact.Nickname,
                CrossroadsSite = Convert.ToInt32(camperContact.Congregation_ID),
                BirthDate = Convert.ToString(camperContact.Date_Of_Birth),
                SchoolAttending = camperContact.Current_School,
                Gender = Convert.ToInt32(camperContact.Gender_ID),
                CurrentGrade = groupResult.Status ? groupResult.Value.GroupName : null
            };
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

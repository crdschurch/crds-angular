using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class ContactRepository : BaseRepository, IContactRepository
    {

        public const string GetContactByIdStoredProc = "api_crds_Get_Contact_By_ID";

        private readonly int _addressesPageId;
        private readonly int _congregationDefaultId;
        private readonly int _contactsPageId;
        private readonly int _householdDefaultSourceId;
        private readonly int _householdPositionDefaultId;
        private readonly int _householdsPageId;
        private readonly ILog _logger = LogManager.GetLogger(typeof (ContactRepository));

        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly int _participantsPageId;
        private readonly int _securityRolesSubPageId;
        

        public ContactRepository(IMinistryPlatformService ministryPlatformService, IAuthenticationRepository authenticationService, IConfigurationWrapper configuration, IMinistryPlatformRestRepository ministryPlatformRest, IApiUserRepository apiUserRepository)
            : base(authenticationService, configuration)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRest = ministryPlatformRest;
            _apiUserRepository = apiUserRepository;

            _householdsPageId = configuration.GetConfigIntValue("Households");
            _securityRolesSubPageId = configuration.GetConfigIntValue("SecurityRolesSubPageId");
            _congregationDefaultId = configuration.GetConfigIntValue("Congregation_Default_ID");
            _householdDefaultSourceId = configuration.GetConfigIntValue("Household_Default_Source_ID");
            _householdPositionDefaultId = configuration.GetConfigIntValue("Household_Position_Default_ID");
            _addressesPageId = configuration.GetConfigIntValue("Addresses");
            _contactsPageId = configuration.GetConfigIntValue("Contacts");
            _participantsPageId = configuration.GetConfigIntValue("Participants");

        }

        public int CreateContactForGuestGiver(string emailAddress, string displayName, string firstName, string lastName)
        {
            var contactDonor = new MpContactDonor
            {
                Details = new MpContactDetails
                {
                    DisplayName = displayName,
                    EmailAddress = emailAddress,
                    FirstName =  firstName,
                    LastName = lastName
                }
            };
            return (CreateContact(contactDonor));
        }

        public int CreateContactForNewDonor(MpContactDonor mpContactDonor)
        {
            return (CreateContact(mpContactDonor));
        }

        public int CreateContactForSponsoredChild(string firstName, string lastName, string town, string idCard)
        {
            var householdId = CreateAddressHouseholdForSponsoredChild(town, lastName);

            var contact = new MpMyContact
            {
                First_Name = firstName,
                Last_Name = lastName,
                ID_Number = idCard,
                Household_ID = householdId
            };

            return CreateContact(contact);
        }

        //Get ID of currently logged in user
        public int GetContactId(string token)
        {
            return _ministryPlatformService.GetContactInfo(token).ContactId;
        }

        public MpMyContact GetContactById(int contactId)
        {
            var parms = new Dictionary<string, object> { { "@ContactID", contactId } };
            var contacts = _ministryPlatformRest.UsingAuthenticationToken(ApiLogin()).GetFromStoredProc<MpMyContact>(GetContactByIdStoredProc, parms)?.FirstOrDefault();
            if (contacts != null && contacts.Count > 1)
            {
                throw new ApplicationException("GetContactById returned multiple records");
            }
            var contact = contacts?.FirstOrDefault();

            return contact;
        }

        public IObservable<MpSimpleContact> GetSimpleContact(int contactId)
        {
            try
            {
                return Observable.Start(() => _ministryPlatformRest.UsingAuthenticationToken(ApiLogin()).Get<MpSimpleContact>(contactId));
            }
            catch (Exception e)
            {
                return Observable.Throw<MpSimpleContact>(new Exception($"Can't find a contact with ID = {contactId}"));
            }
        }

        public MpMyContact GetContactByIdCard(string idCard)
        {
            var searchString = string.Format(new String(',', 34) + "\"{0}\"", idCard);
            var pageViewRecords = _ministryPlatformService.GetPageViewRecords("AllIndividualsWithContactId", ApiLogin(), searchString);
            if (pageViewRecords.Count > 1)
            {
                throw new ApplicationException("GetContactById returned multiple records");
            }

            if (pageViewRecords.Count == 0)
            {
                return null;
            }
            return ParseProfileRecord(pageViewRecords[0]);
        }

        public MpMyContact GetContactByParticipantId(int participantId)
        {
            var token = ApiLogin();
            var searchString = string.Format("{0},", participantId);
            var contacts = _ministryPlatformService.GetPageViewRecords("ContactByParticipantId", token, searchString);
            var c = contacts.SingleOrDefault();
            if (c == null)
            {
                return null;
            }
            var contact = new MpMyContact
            {
                Contact_ID = c.ToInt("Contact_ID"),
                Email_Address = c.ToString("Email_Address"),
                Last_Name = c.ToString("Last_Name"),
                First_Name = c.ToString("First_Name")
            };
            return contact;
        }

        public string GetContactEmail(int contactId)
        {
            try
            {
                var recordsDict = _ministryPlatformService.GetRecordDict(_contactsPageId, contactId, ApiLogin());

                var contactEmail = recordsDict["Email_Address"].ToString();

                return contactEmail;
            }
            catch (NullReferenceException)
            {
                _logger.Debug(string.Format("Trying to email address of {0} failed", contactId));
                return string.Empty;
            }
        }

        public MpContact GetEmailFromDonorId(int donorId)
        {
            MpContact contact = new MpContact();
            try
            {
                string searchFilter = $"Donor_Record={donorId}";
                var foundEmails = _ministryPlatformRest.UsingAuthenticationToken(base.ApiLogin()).Search<MpContact>(searchFilter, "Contact_ID, Donor_Record, Email_Address");
                if (foundEmails.Count == 1)
                    contact = foundEmails.First();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("GetEmailFromDonorId failed.  Donor Id: {0}", donorId), ex);
            }

            return contact;
        }

        public int GetContactIdByEmail(string email)
        {
            var records = _ministryPlatformService.GetRecordsDict(_configurationWrapper.GetConfigIntValue("Contacts"), ApiLogin(), (email));
            if (records.Count > 1)
            {
                throw new Exception("User email did not return exactly one user record");
            }
            if (records.Count < 1)
            {
                return 0;
            }

            var record = records[0];
            return record.ToInt("dp_RecordID");
        }


        #region changes inactive users to active and records in the audit log
        public void UpdateContactToActive(int contactId)
        {
            try
            {
                var token = ApiLogin();
                var searchString = $"Contact_ID = {contactId}";
                const string column = "Contact_Status_ID";
                var contactstatus = _ministryPlatformRest.UsingAuthenticationToken(token).Search<int>("Contacts", searchString, column, null);

                if (contactstatus == 2)
                {
                    contactstatus = 1;
                    Dictionary<string, object> activestatus = new Dictionary<string, object>()
                    {
                        {"Contact_ID",contactId },
                        {"Contact_Status_ID", contactstatus}
                    };
                    _ministryPlatformRest.UsingAuthenticationToken(token).UpdateRecord("Contacts", -1, activestatus);
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("Error Changing User to Active Status: " + e.Message);
            }
        }

    #endregion

        public int GetActiveContactIdByEmail(string email)
        {
            // filter out contact_status = deceased (id = 3)
            var token = ApiLogin();
            string filter = $" Display_Name != 'Guest Giver' AND Contact_Status_ID != 3 AND Email_Address = '{email}'";
            var columns = new List<string>
            {
                "Email_Address",
                "Contact_ID"
            };

            var records = _ministryPlatformRest.UsingAuthenticationToken(token).Search<MpMyContact>(filter, columns);

            if (records.Count > 1)
            {
                _logger.Error($"There are multiple contacts for email: <{email}>, trying to find a unique contact by user record");
                var userRecord =
                    _ministryPlatformRest.UsingAuthenticationToken(token)
                        .Search<MpUser, MpContact>($"User_Name = '{email}'", "Contact_Id");
                if (userRecord.Count < 1)
                {
                    throw new Exception("User email did not return exactly one contact record and no user record found");
                }
                return userRecord[0].ContactId;
            }
            if (records.Count < 1)
            {
                return 0;
            }
            
            return records[0].Contact_ID;
        }

        public int GetContactIdByParticipantId(int participantId)
        {
            var token = ApiLogin();
            var participant = _ministryPlatformService.GetRecordDict(_participantsPageId, participantId, token);
            return participant.ToInt("Contact_ID");
        }

        public IList<int> GetContactIdByRoleId(int roleId, string token)
        {
            var records = _ministryPlatformService.GetSubPageRecords(_securityRolesSubPageId, roleId, token);

            return records.Select(record => (int) record["Contact_ID"]).ToList();
        }

        public List<MpHouseholdMember> GetHouseholdFamilyMembers(int householdId)
        {
            var token = ApiLogin();
            var familyRecords = _ministryPlatformService.GetSubpageViewRecords("HouseholdMembers", householdId, token);
            var family = familyRecords.Select(famRec => new MpHouseholdMember
            {
                ContactId = famRec.ToInt("Contact_ID"),
                FirstName = famRec.ToString("First_Name"),
                Nickname = famRec.ToString("Nickname"),
                LastName = famRec.ToString("Last_Name"),
                DateOfBirth = famRec.ToDate("Date_of_Birth"),
                Age = famRec.ToInt("__Age"),
                HouseholdPosition = famRec.ToString("Household_Position"),
                StatementTypeId = famRec.ContainsKey("Statement_Type_ID") ? famRec.ToInt("Statement_Type_ID") : (int?) null,
                DonorId = famRec.ContainsKey("Donor_ID") ? famRec.ToInt("Donor_ID") : (int?) null
            }).ToList();
            return family;
        }
        
        /// <summary>
        /// Get the Other/Former members of a particular household
        /// </summary>
        /// <param name="householdId">the householdId of the household where you want the other members</param>
        /// <returns> a list of MpHouseholdMember </returns>
        public List<MpHouseholdMember> GetOtherHouseholdMembers(int householdId)
        {
            var token = ApiLogin();
            var filter = $"Contact_Households.Household_ID = {householdId} ";
            var columns = new List<string>
            {
                "Contact_Households.Contact_ID",
                "Contact_Households.Household_ID",
                "Contact_Households.Household_Position_ID",
                "Household_Position_ID_Table.Household_Position",
                "Contact_ID_Table.First_Name",
                "Contact_ID_Table.Nickname",
                "Contact_ID_Table.Last_Name",
                "Contact_ID_Table.Date_of_Birth",
                "Contact_ID_Table.__Age",
                "Contact_Households.End_Date"
            };
            var result = _ministryPlatformRest.UsingAuthenticationToken(token).Search<MpContactHousehold>(filter, columns);
            var householdMembers = result.Where((hm) => hm.EndDate == null || hm.EndDate > DateTime.Now).Select((hm) => new MpHouseholdMember
            {
                Age = hm.Age ?? 0,
                ContactId = hm.ContactId,
                DateOfBirth = hm.DateOfBirth ?? new DateTime(),
                FirstName = hm.FirstName,
                LastName = hm.LastName,
                HouseholdPosition = hm.HouseholdPosition,
                Nickname = hm.Nickname,
            }).ToList();
            return householdMembers;
        }      

        public MpMyContact GetMyProfile(string token)
        {
            var recordsDict = _ministryPlatformService.GetRecordsDict("MyProfile", token);

            if (recordsDict.Count > 1)
            {
                throw new ApplicationException("GetMyProfile returned multiple records");
            }

            var contact = ParseProfileRecord(recordsDict[0]);

            return contact;
        }

        public List<Dictionary<string, object>> PrimaryContacts(bool staffOnly = false)
        {
            string columns = string.Join(", ",
                "Contact_ID_Table.Contact_ID",
                "Contact_ID_Table.Display_Name",
                "Contact_ID_Table.Email_Address",
                "Contact_ID_Table.First_Name",
                "Contact_ID_Table.Last_Name"
            );

            // always include contacts with the "Staff" attribute
            List<int> attributeIds = new List<int>()
            {
                _configurationWrapper.GetConfigIntValue("StaffContactAttribute")
            };

            // optionally include contacts with the "Event Tool Contact" attribute
            if (!staffOnly)
                attributeIds.Add(_configurationWrapper.GetConfigIntValue("EventToolContactAttribute"));

            string filter = $"Attribute_ID IN ({string.Join(",", attributeIds)})";
            filter += " AND Start_Date <= GETDATE() AND (End_Date IS NULL OR End_Date > GETDATE())";

            string apiToken = ApiLogin();
            var records = _ministryPlatformRest.UsingAuthenticationToken(apiToken)
                            .Search<MpContactAttribute, Dictionary<string, object>>(filter, columns, "Display_Name", true);
            return records;
        }

        public void UpdateContact(int contactId, Dictionary<string, object> profileDictionary)
        {
            var retValue = WithApiLogin<int>(token =>
            {
                try
                {
                    // Validate phone numbers' format before update
                    if (profileDictionary.ContainsKey("Mobile_Phone"))
                    {
                        var mobileNumber = profileDictionary["Mobile_Phone"] as string;
                        if (!Helpers.PhoneNumberValidator.Validate(mobileNumber))
                        {
                            throw new ApplicationException("Mobile phone format is wrong. Format should be ###-###-####");
                        }
                    }

                    _ministryPlatformService.UpdateRecord(_configurationWrapper.GetConfigIntValue("Contacts"), profileDictionary, token);
                    return 1;
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Error Saving mpContact: " + e.Message);
                }
            });
        }

        public void UpdateContact(int contactId,
                                  Dictionary<string, object> profileDictionary,
                                  Dictionary<string, object> householdDictionary,
                                  Dictionary<string, object> addressDictionary)
        {
            WithApiLogin<int>(token =>
            {
                try
                {
                    if (profileDictionary.ContainsKey("Mobile_Phone"))
                    {
                        var mobileNumber = profileDictionary["Mobile_Phone"] as string;
                        if (!Helpers.PhoneNumberValidator.Validate(mobileNumber))
                        {
                            throw new ApplicationException("Mobile phone format is wrong. Format should be ###-###-####");
                        }
                    }
                    if (profileDictionary.ContainsKey("Home_Phone"))
                    {
                        var homeNumber = householdDictionary["Home_Phone"] as string;

                        if (!Helpers.PhoneNumberValidator.Validate(homeNumber))
                        {
                            throw new ApplicationException("Home phone format is wrong. Format should be ###-###-####");
                        }
                    }

                    _ministryPlatformService.UpdateRecord(_configurationWrapper.GetConfigIntValue("Contacts"), profileDictionary, token);

                    UpdateHouseholdAddress(contactId, householdDictionary, addressDictionary);
                    return 1;
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Error Saving mpContact: " + e.Message);
                }
            });
        }

        public void SetHouseholdAddress(int contactId, int householdId, int addressId)
        {
            var token = ApiLogin();
            var household = new MpHousehold() { Address_ID = addressId, Household_ID = householdId };
            _ministryPlatformRest.UsingAuthenticationToken(token).Update<MpHousehold>(household);
        }

        // This function will insert/update the Addresses table, and optionally update
        // the Households table.  householdDictionary is optional when updating an
        // existing address, but required when creating a new address.
        public void UpdateHouseholdAddress(int contactId,
                                  Dictionary<string, object> householdDictionary,
                                  Dictionary<string, object> addressDictionary)
        {
            string apiToken = ApiLogin();

            if (addressDictionary != null)
            {
                // don't create orphaned address records
                bool addressAlreadyExists = addressDictionary["Address_ID"] != null ? true : false;
                if (!addressAlreadyExists && householdDictionary == null)
                    throw new ArgumentException("Household is required when adding a new address");

                if (addressAlreadyExists)
                {
                    //address exists, update it
                    _ministryPlatformService.UpdateRecord(_configurationWrapper.GetConfigIntValue("Addresses"), addressDictionary, apiToken);
                }
                else
                {
                    //address does not exist, create it, then attach to household
                    var addressId = _ministryPlatformService.CreateRecord(_configurationWrapper.GetConfigIntValue("Addresses"), addressDictionary, apiToken);
                    householdDictionary["Address_ID"] = addressId;
                }
            }

            if (householdDictionary != null)
                _ministryPlatformService.UpdateRecord(_configurationWrapper.GetConfigIntValue("Households"), householdDictionary, apiToken);
        }

        public List<MpRecordID> CreateContact(MpContact minorContact)
        {
            var storedProc = _configurationWrapper.GetConfigValue("CreateContactStoredProc");
            var apiToken = ApiLogin();
            var fields = new Dictionary<String, Object>
              {
                {"@FirstName", minorContact.FirstName},
                {"@LastName", minorContact.LastName},
                {"@MiddleName", minorContact.MiddleName ?? ""},
                {"@PreferredName", minorContact.PreferredName },
                {"@NickName", minorContact.Nickname },
                {"@Birthdate", minorContact.BirthDate },
                {"@Gender", minorContact.Gender },
                {"@SchoolAttending", minorContact.SchoolAttending },
                {"@HouseholdId", minorContact.HouseholdId },
                {"@HouseholdPosition", minorContact.HouseholdPositionId },
                {"@MobilePhone", minorContact.MobilePhone ?? ""}
             };

            var result = _ministryPlatformRest.UsingAuthenticationToken(apiToken).GetFromStoredProc<MpRecordID>(storedProc, fields);
            var contactIdList = result.FirstOrDefault() ?? new List<MpRecordID>();
            return contactIdList;
        }

        public MpMyContact GetContactByUserRecordId(int userRecordId, string apiToken=null)
        {
            if (String.IsNullOrEmpty(apiToken))
                apiToken = ApiLogin();
            Dictionary<string, object> filter = new Dictionary<string, object>()
            {
                {"User_Account", userRecordId}
            };
            var records = _ministryPlatformRest.UsingAuthenticationToken(apiToken).Get<MpMyContact>("Contacts",filter);
            if (records.Count > 1)
            {
                throw new ApplicationException("GetContactByUserRecordId returned multiple records");
            }
            if (records.Count == 0)
            {
                return null;
            }
            return records.FirstOrDefault();

        }

        public IObservable<MpHousehold> UpdateHousehold(MpHousehold household)
        {
            var token = ApiLogin();            
            var asyncresult = Task.Run(() => _ministryPlatformRest.UsingAuthenticationToken(token).Update<MpHousehold>(household));
            return asyncresult.ToObservable();
        }

        private static MpMyContact ParseProfileRecord(Dictionary<string, object> recordsDict)
        {
            var contact = new MpMyContact
            {
                Address_ID = recordsDict.ToNullableInt("Address_ID"),
                Address_Line_1 = recordsDict.ToString("Address_Line_1"),
                Address_Line_2 = recordsDict.ToString("Address_Line_2"),
                Congregation_ID = recordsDict.ToNullableInt("Congregation_ID"),
                Household_ID = recordsDict.ToInt("Household_ID"),
                Household_Name = recordsDict.ToString("Household_Name"),
                City = recordsDict.ToString("City"),
                State = recordsDict.ToString("State"),
                County = recordsDict.ToString("County"),
                Postal_Code = recordsDict.ToString("Postal_Code"),
                Contact_ID = recordsDict.ToInt("Contact_ID"),
                Date_Of_Birth = recordsDict.ToDateAsString("Date_of_Birth"),
                Email_Address = recordsDict.ToString("Email_Address"),
                Employer_Name = recordsDict.ToString("Employer_Name"),
                First_Name = recordsDict.ToString("First_Name"),
                Foreign_Country = recordsDict.ToString("Foreign_Country"),
                Gender_ID = recordsDict.ToNullableInt("Gender_ID"),
                Home_Phone = recordsDict.ToString("Home_Phone"),
                Current_School = recordsDict.ToString("Current_School"),
                Last_Name = recordsDict.ToString("Last_Name"),
                Maiden_Name = recordsDict.ToString("Maiden_Name"),
                Marital_Status_ID = recordsDict.ToNullableInt("Marital_Status_ID"),
                Middle_Name = recordsDict.ToString("Middle_Name"),
                Mobile_Carrier = recordsDict.ToNullableInt("Mobile_Carrier_ID"),
                Mobile_Phone = recordsDict.ToString("Mobile_Phone"),
                Nickname = recordsDict.ToString("Nickname"),
                Age = recordsDict.ToInt("Age"),
                Passport_Number = recordsDict.ToString("Passport_Number"),
                Passport_Country = recordsDict.ToString("Passport_Country"),
                Passport_Expiration = ParseExpirationDate(recordsDict.ToNullableDate("Passport_Expiration")),
                Passport_Firstname = recordsDict.ToString("Passport_Firstname"),
                Passport_Lastname = recordsDict.ToString("Passport_Lastname"),
                Passport_Middlename = recordsDict.ToString("Passport_Middlename")
                
            };
            if (recordsDict.ContainsKey("Participant_Start_Date"))
            {
                contact.Participant_Start_Date = recordsDict.ToDate("Participant_Start_Date");
            }
            if (recordsDict.ContainsKey("Attendance_Start_Date"))
            {
                contact.Attendance_Start_Date = recordsDict.ToNullableDate("Attendance_Start_Date");
            }

            if (recordsDict.ContainsKey("ID_Card"))
            {
                contact.ID_Number = recordsDict.ToString("ID_Card");
            }
            return contact;
        }

        private static DateTime? ParseExpirationDate(DateTime? date)
        {
            if (date != null)
            {
                return date;
            }
            return null;
        }

        public MpContact CreateSimpleContact(string firstName, string lastName, string email, string dob, string mobile)
        {
            var contactId = CreateContact(new MpMyContact
            {
                Date_Of_Birth = dob,
                First_Name = firstName,
                Last_Name = lastName,
                Email_Address = email,
                Mobile_Phone = mobile
            });
            return new MpContact() { ContactId = contactId, EmailAddress = email};
        }

        private int CreateAddressHouseholdForSponsoredChild(string town, string lastName)
        {
            if (!String.IsNullOrEmpty(town))
            {
                var address = new MpPostalAddress
                {
                    City = town,
                    Line1 = "Not Known"
                };
                return CreateHouseholdAndAddress(lastName, address, ApiLogin());
            }
            else
            {
                return -1;
            }
        }

        private int CreateContact(MpMyContact contact)
        {
            var contactDictionary = new Dictionary<string, object>
            {
                {"Company", false},
                {"Last_Name", contact.Last_Name},
                {"First_Name", contact.First_Name},
                {"Email_Address", contact.Email_Address },
                {"Display_Name", contact.Last_Name + ", " + contact.First_Name},
                {"Nickname", contact.First_Name},
                {"ID_Card", contact.ID_Number}
            };

            if (contact.Household_ID > 0)
            {
                contactDictionary.Add("HouseHold_ID", contact.Household_ID);
                contactDictionary.Add("Household_Position_ID", _householdPositionDefaultId);
            }

            if (contact.Mobile_Phone != string.Empty)
            {
                contactDictionary.Add("Mobile_Phone", contact.Mobile_Phone);
            }

            if (contact.Date_Of_Birth != string.Empty)
            {
                contactDictionary.Add("Date_Of_Birth", contact.Date_Of_Birth);
            }

            try
            {
                var token = ApiLogin();
                return (_ministryPlatformService.CreateRecord(_contactsPageId, contactDictionary, token, false));
            }
            catch (Exception e)
            {
                var msg = $"Error creating Contact, firstName: {contact.First_Name} lastName: {contact.Last_Name} idCard: {contact.ID_Number}";
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        private int CreateContact(MpContactDonor mpContactDonor)
        {
            var token = ApiLogin();

            var emailAddress = mpContactDonor.Details.EmailAddress;
            var displayName = mpContactDonor.Details.DisplayName;
            var firstName = mpContactDonor.Details.FirstName;
            var lastName = mpContactDonor.Details.LastName;
            int? householdId = null;
            if (mpContactDonor.Details.HasAddress)
            {
                try
                {
                    householdId = CreateHouseholdAndAddress(displayName, mpContactDonor.Details.Address, token);
                }
                catch (Exception e)
                {
                    var msg = string.Format("Error creating household and address for emailAddress: {0} displayName: {1}",
                                            emailAddress,
                                            displayName);
                    _logger.Error(msg, e);
                    throw (new ApplicationException(msg, e));
                }
            }

            var contactDictionary = new Dictionary<string, object>
            {
                {"Email_Address", emailAddress},
                {"First_Name", firstName },
                {"Last_Name", lastName },
                {"Company", false},
                {"Display_Name", displayName},
                {"Nickname", displayName},
                {"Household_ID", householdId},
                {"Household_Position_ID", _householdPositionDefaultId}
            };

            try
            {
                return (_ministryPlatformService.CreateRecord(_contactsPageId, contactDictionary, token));
            }
            catch (Exception e)
            {
                var msg = string.Format("Error creating mpContact, emailAddress: {0} displayName: {1}",
                                        emailAddress,
                                        displayName);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        private int CreateHouseholdAndAddress(string householdName, MpPostalAddress address, string apiToken)
        {
            var addressDictionary = new Dictionary<string, object>
            {
                {"Address_Line_1", address.Line1},
                {"Address_Line_2", address.Line2},
                {"City", address.City},
                {"State/Region", address.State},
                {"Postal_Code", address.PostalCode}
            };
            var addressId = _ministryPlatformService.CreateRecord(_addressesPageId, addressDictionary, apiToken);

            var householdDictionary = new Dictionary<string, object>
            {
                {"Household_Name", householdName},
                {"Congregation_ID", _congregationDefaultId},
                {"Household_Source_ID", _householdDefaultSourceId},
                {"Address_ID", addressId}
            };

            return (_ministryPlatformService.CreateRecord(_householdsPageId, householdDictionary, apiToken));
        }
    }
}

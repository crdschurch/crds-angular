using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Crossroads.Utilities;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Enum;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.PlatformService;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Newtonsoft.Json.Linq;
using MpCommunication = MinistryPlatform.Translation.Models.MpCommunication;
using Constants = Crossroads.Utilities.Constants;

namespace MinistryPlatform.Translation.Repositories
{
    public class DonorRepository : BaseRepository, IDonorRepository
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(DonorRepository));

        private readonly int _donorPageId;
        private readonly int _donationPageId;
        private readonly int _donationDistributionPageId;
        private readonly int _donorAccountsPageId;
        private readonly int _findDonorByAccountPageViewId;
        private readonly int _donationStatusesPageId;
        private readonly int _donorLookupByEncryptedAccount;
        private readonly int _myHouseholdDonationDistributions;
        private readonly int _recurringGiftBySubscription;
        private readonly int _recurringGiftPageId;
        private readonly int _myDonorPageId;
        private readonly int _myHouseholdDonationRecurringGifts;
        private readonly int _myHouseholdRecurringGiftsApiPageView;
        private readonly int _myHouseholdPledges;
        
        public const string DonorRecordId = "Donor_Record";
        public const string DonorProcessorId = "Processor_ID";
        public const string EmailReason = "None";
        public const string DefaultInstitutionName = "Bank";
        public const string DonorRoutingNumberDefault = "0";
        public const string DonorAccountNumberDefault = "0";
        // This is taken from GnosisChecks: 
        // https://github.com/crdschurch/GnosisChecks/blob/24edc373ae62660028c1637396a9b834dfb2fd4d/Modules.vb#L12
        public const string HashKey = "Mcc3#e758ebe8Seb1fdeF628dbK796e5";

        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly IProgramRepository _programService;
        private readonly ICommunicationRepository _communicationService;
        private readonly IContactRepository _contactService;
        private readonly ICryptoProvider _crypto;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly DateTimeFormatInfo _dateTimeFormat;
        

        public DonorRepository(IMinistryPlatformService ministryPlatformService, IMinistryPlatformRestRepository ministryPlatformRestRepository, IProgramRepository programService, ICommunicationRepository communicationService, IAuthenticationRepository authenticationService, IContactRepository contactService,  IConfigurationWrapper configuration, ICryptoProvider crypto, IApiUserRepository apiUserRepository)
            : base(authenticationService, configuration)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
            _programService = programService;
            _communicationService = communicationService;
            _contactService = contactService;
            _apiUserRepository = apiUserRepository;
            _crypto = crypto;

            _donorPageId = configuration.GetConfigIntValue("Donors");
            _donationPageId = configuration.GetConfigIntValue("Donations");
            _donationDistributionPageId = configuration.GetConfigIntValue("Distributions");
            _donorAccountsPageId = configuration.GetConfigIntValue("DonorAccounts");
            _findDonorByAccountPageViewId = configuration.GetConfigIntValue("FindDonorByAccountPageView");
            _donationStatusesPageId = configuration.GetConfigIntValue("DonationStatus");
            _donorLookupByEncryptedAccount = configuration.GetConfigIntValue("DonorLookupByEncryptedAccount");
            _myHouseholdDonationDistributions = configuration.GetConfigIntValue("MyHouseholdDonationDistributions");
            _recurringGiftBySubscription = configuration.GetConfigIntValue("RecurringGiftBySubscription");
            _recurringGiftPageId = configuration.GetConfigIntValue("RecurringGifts");
            _myDonorPageId = configuration.GetConfigIntValue("MyDonor");
            _myHouseholdDonationRecurringGifts = configuration.GetConfigIntValue("MyHouseholdDonationRecurringGifts");
            _myHouseholdRecurringGiftsApiPageView = configuration.GetConfigIntValue("MyHouseholdRecurringGiftsApiPageView");
            _myHouseholdPledges = configuration.GetConfigIntValue("MyHouseholdPledges");

            _dateTimeFormat = new DateTimeFormatInfo
            {
                AMDesignator = "am",
                PMDesignator = "pm"
            };

        }


        public int CreateDonorRecord(int contactId, string processorId, DateTime setupTime, int? statementFrequencyId = 1,
                                     // default to quarterly
                                     int? statementTypeId = 1,
                                     //default to individual
                                     int? statementMethodId = 2,
                                     // default to email/online
                                     MpDonorAccount mpDonorAccount = null
            )
        {
            //this assumes that you do not already have a donor record - new giver
            var values = new Dictionary<string, object>
            {
                {"Contact_ID", contactId},
                {"Statement_Frequency_ID", statementFrequencyId},
                {"Statement_Type_ID", statementTypeId},
                {"Statement_Method_ID", statementMethodId},
                {"Setup_Date", setupTime}, //default to current date/time
                {"Processor_ID", processorId}
            };

            var apiToken = ApiLogin();

            int donorId;

            try
            {
                donorId = _ministryPlatformService.CreateRecord(_donorPageId, values, apiToken, true);
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("CreateDonorRecord failed.  Contact Id: {0}", contactId), e);
            }
   
            if (mpDonorAccount!= null)
            {
                CreateDonorAccount(DefaultInstitutionName,
                                   DonorAccountNumberDefault,
                                   DonorRoutingNumberDefault,
                                   mpDonorAccount.EncryptedAccount,
                                   donorId,
                                   mpDonorAccount.ProcessorAccountId,
                                   processorId);
            }
            
            return donorId;
        }

        // TODO Need this method to accept an authorized user token in order to facilitate admin setup/edit
        public int CreateDonorAccount(string giftType, string routingNumber, string acctNumber, string encryptedAcct, int donorId, string processorAcctId, string processorId)
        {
            var apiToken = ApiLogin();

            var institutionName = giftType ?? DefaultInstitutionName;
            var accountType = (institutionName == "Bank") ? AccountType.Checking : AccountType.Credit;

            try
            {
              var  values = new Dictionary<string, object>
                {
                    { "Institution_Name", institutionName },
                    { "Account_Number", acctNumber },
                    { "Routing_Number", DonorRoutingNumberDefault },
                    { "Encrypted_Account", encryptedAcct },
                    { "Donor_ID", donorId },
                    { "Non-Assignable", false },
                    { "Account_Type_ID", (int)accountType},
                    { "Closed", false },
                    {"Processor_Account_ID", processorAcctId},
                    {"Processor_ID", processorId}
                };

                 var donorAccountId = _ministryPlatformService.CreateRecord(_donorAccountsPageId, values, apiToken);  
                 return donorAccountId; 
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("CreateDonorAccount failed.  Donor Id: {0}", donorId), e);
            }
           
        }

        public void DeleteDonorAccount(int donorAccountId)
        {
            var authorizedUserToken = _apiUserRepository.GetDefaultApiClientToken();
            try
            {
                _ministryPlatformService.DeleteRecord(_donorAccountsPageId, donorAccountId, new []
                {
                    new DeleteOption
                    {
                        Action = DeleteAction.Delete
                    }
                }, authorizedUserToken);
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("RemoveDonorAccount failed.  Donor Id: {0}", donorAccountId), e);
            }
        }

        public void UpdateRecurringGiftDonorAccount(int recurringGiftId, int donorAccountId)
        {
            var recurringGiftValues = new Dictionary<string, object>
            {
                {"Donor_Account_ID", donorAccountId}
            };

            UpdateRecurringGift(_myHouseholdDonationRecurringGifts, recurringGiftId, recurringGiftValues);
        }

        [Obsolete("Use cancel recurring without token")]
        public void CancelRecurringGift(string authorizedUserToken, int recurringGiftId)
        {
            var recurringGiftValues = new Dictionary<string, object>
            {
                {"End_Date", DateTime.Now.Date}
            };
           
            UpdateRecurringGift(_myHouseholdDonationRecurringGifts, recurringGiftId, recurringGiftValues);
        }

        public void CancelRecurringGift(int recurringGiftId)
        {
            var recurringGiftValues = new Dictionary<string, object>
            {
                {"End_Date", DateTime.Now.Date}
            };
            UpdateRecurringGift(_recurringGiftPageId, recurringGiftId, recurringGiftValues);
        }
        
        public void UpdateRecurringGiftFailureCount(int recurringGiftId, int failCount)
        {
            var recurringGiftValues = new Dictionary<string, object>
            {
                {"Consecutive_Failure_Count", failCount}
            };

            try
            {
                UpdateRecurringGift(_recurringGiftPageId, recurringGiftId, recurringGiftValues);
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                       string.Format(
                           "Update Recurring Gift Failure Count failed.  Recurring Gift Id: {0}"),e);
            }
        }


        public void UpdateRecurringGift(int pageView, int recurringGiftId, Dictionary<string, object> recurringGiftValues)
        {
            var token = _apiUserRepository.GetDefaultApiClientToken();
            recurringGiftValues["Recurring_Gift_ID"] = recurringGiftId;

            try
            {
               _ministryPlatformService.UpdateRecord(pageView, recurringGiftValues, token);
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                    string.Format(
                        "Update Recurring Gift Donor Account failed.  Recurring Gift Id: {0}, Updates: {1}"
                        , recurringGiftId
                        , string.Join(";", recurringGiftValues)),
                    e);
            }
            
        }

        private void CreateDistributionRecord(int donationId, decimal amount, string programId, int? pledgeId = null, string apiToken = null)
        {
            if (apiToken == null)
            {
                apiToken = ApiLogin();
            }

            var distributionValues = new Dictionary<string, object>
            {
                {"Donation_ID", donationId},
                {"Amount", amount},
                {"Program_ID", programId}                
            };
            if (pledgeId != null)
            {
                distributionValues.Add("Pledge_ID", pledgeId);
            }

            try
            {
                _ministryPlatformService.CreateRecord(_donationDistributionPageId, distributionValues, apiToken, true);
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                    string.Format("CreateDonationDistributionRecord failed.  Donation Id: {0}", donationId), e);
            }
        }

        public int CreateDonationAndDistributionRecord(MpDonationAndDistributionRecord donationAndDistribution, bool sendConfirmationEmail = true)
        {
            var pymtId = PaymentType.GetPaymentType(donationAndDistribution.PymtType).id;
            var fee = donationAndDistribution.FeeAmt.HasValue ? donationAndDistribution.FeeAmt / Constants.StripeDecimalConversionValue : null;

            var apiToken = ApiLogin();

            var donationValues = new Dictionary<string, object>
            {
                {"Donor_ID", donationAndDistribution.DonorId},
                {"Donation_Amount", donationAndDistribution.DonationAmt},
                {"Processor_Fee_Amount", fee},
                {"Payment_Type_ID", pymtId},
                {"Donation_Date", donationAndDistribution.SetupDate},
                {"Transaction_code", donationAndDistribution.ChargeId},
                {"Registered_Donor", donationAndDistribution.RegisteredDonor},
                {"Anonymous", donationAndDistribution.Anonymous},
                {"Processor_ID", donationAndDistribution.ProcessorId },
                {"Donation_Status_Date", donationAndDistribution.SetupDate},
                {"Donation_Status_ID", donationAndDistribution.DonationStatus ?? 1}, //hardcoded to pending if no status specified
                {"Recurring_Gift_ID", donationAndDistribution.RecurringGiftId},
                {"Is_Recurring_Gift", donationAndDistribution.RecurringGift},
                {"Donor_Account_ID", donationAndDistribution.DonorAcctId},
                {"Source_Url", donationAndDistribution.SourceUrl},
                {"Predefined_Amount", donationAndDistribution.PredefinedAmount},
            };
            if (!string.IsNullOrWhiteSpace(donationAndDistribution.CheckScannerBatchName))
            {
                donationValues["Check_Scanner_Batch"] = donationAndDistribution.CheckScannerBatchName;
            }

            if (!string.IsNullOrWhiteSpace(donationAndDistribution.CheckNumber))
            {
                donationValues["Item_Number"] = donationAndDistribution.CheckNumber;
            }

            if (!string.IsNullOrWhiteSpace(donationAndDistribution.Notes))
            {
                donationValues["Notes"] = donationAndDistribution.Notes;
            }

            int donationId;

            try
            {
                donationId = _ministryPlatformService.CreateRecord(_donationPageId, donationValues, apiToken, true);
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("CreateDonationRecord failed.  Donor Id: {0}", donationAndDistribution.DonorId), e);
            }

            if (donationAndDistribution.HasDistributions)
            {
                foreach (var distribution in donationAndDistribution.Distributions)
                {
                    CreateDistributionRecord(donationId,
                                             distribution.donationDistributionAmt/Constants.StripeDecimalConversionValue,
                                             distribution.donationDistributionProgram,
                                             distribution.PledgeId,
                                             apiToken);
                }
            }
            else if (string.IsNullOrWhiteSpace(donationAndDistribution.ProgramId))
            {
                return (donationId);
            } 
            else 
            {
                CreateDistributionRecord(donationId, donationAndDistribution.DonationAmt, donationAndDistribution.ProgramId, donationAndDistribution.PledgeId, apiToken);
            }

            if (!sendConfirmationEmail)
            {
                return (donationId);
            }

            if (sendConfirmationEmail)
            {
                try
                {
                    SetupConfirmationEmail(Convert.ToInt32(donationAndDistribution.ProgramId), donationAndDistribution.DonorId, donationAndDistribution.DonationAmt, donationAndDistribution.SetupDate, donationAndDistribution.PymtType, donationAndDistribution.PledgeId ?? 0);
                }
            catch (Exception e)
                {
                _logger.Error(string.Format("Failed when processing the template for Donation Id: {0}", donationId), e);
                }
            }

            return donationId;
        }

        public MpContactDonor GetContactDonor(int contactId)
        {
            MpContactDonor donor;
            try
            {
                var token = base.ApiLogin();
                var parameters = new Dictionary<string, object>
                {
                    { "@Contact_ID", contactId }
                };
              
                var storedProcReturn = _ministryPlatformRestRepository.UsingAuthenticationToken(token).GetFromStoredProc<MpContactDonor>("api_crds_Get_Contact_Donor", parameters);


                if (storedProcReturn != null && storedProcReturn.Count > 0 && storedProcReturn[0].Count > 0)
                    donor = storedProcReturn[0].First();
                else
                {
                    donor = new MpContactDonor
                    {
                        ContactId = contactId,
                        RegisteredUser = true
                    };
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("GetDonorRecord failed.  Contact Id: {0}", contactId), ex);
            }

            return donor;

        }
        public MpContactDonor GetPossibleGuestContactDonor(string email)
        {
            MpContactDonor donor;
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return null;
                }
                var searchStr =  "," + email;
                var records =
                    WithApiLogin(
                        apiToken => (_ministryPlatformService.GetPageViewRecords("PossibleGuestDonorContact", apiToken, searchStr)));
                if (records != null && records.Count > 0)
                {
                    var record = records.First();
                    donor = new MpContactDonor()
                    {
                        
                        DonorId = record.ToInt(DonorRecordId),
                        ProcessorId = record.ToString(DonorProcessorId),
                        ContactId = record.ToInt("Contact_ID"),
                        Email = record.ToString("Email_Address"),
                        RegisteredUser = false,
                        Details = new MpContactDetails
                        {
                            FirstName = record.ToString("First_Name"),
                            LastName = record.ToString("Last_Name")
                        }
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("GetPossibleGuestDonorContact failed. Email: {0}", email), ex);
            }

            return donor;

        }

        public MpContactDonor GetContactDonorForDonorAccount(string accountNumber, string routingNumber)
        {
            var search = string.Format(",\"{0}\"", CreateHashedAccountAndRoutingNumber(accountNumber, routingNumber));

            var accounts = WithApiLogin(apiToken => _ministryPlatformService.GetPageViewRecords(_findDonorByAccountPageViewId, apiToken, search));
            if (accounts == null || accounts.Count == 0)
            {
                return (null);
            }

            var contactId = accounts[0]["Contact_ID"] as int? ?? -1;

            if (contactId == -1)
            {
                return (null); 
            }

            var contactDonor = GetContactDonor(contactId);

            contactDonor.Account = new MpDonorAccount
            {
                DonorAccountId = accounts[0].ToInt("Donor_Account_ID"),
                ProcessorAccountId = accounts[0]["Processor_Account_ID"].ToString(),
                ProcessorId = accounts[0]["Processor_ID"].ToString()
            };

            return contactDonor;
        }

        public MpContactDonor GetContactDonorForCheckAccount(string encrptedKey)
        {
            var donorAccount = WithApiLogin(apiToken => _ministryPlatformService.GetPageViewRecords(_donorLookupByEncryptedAccount, apiToken, "," + encrptedKey));
            if (donorAccount == null || donorAccount.Count == 0)
            {
                return null;
            }
            var contactId = Convert.ToInt32(donorAccount[0]["Contact_ID"]);
            var myContact = _contactService.GetContactById(contactId);

            var details = new MpContactDonor
            {
               DonorId = (int) donorAccount[0]["Donor_ID"],
               Details = new MpContactDetails
               {
                   DisplayName = donorAccount[0]["Display_Name"].ToString(),
                   Address = new MpPostalAddress
                   {
                       Line1 = myContact.Address_Line_1,
                       Line2 = myContact.Address_Line_2,
                       City = myContact.City,
                       State = myContact.State,
                       PostalCode = myContact.Postal_Code
                   } 
               }
               
            };

            return details;
        }

        /// <summary>
        /// Create a SHA256 of the given account and routing number.  The algorithm for this matches the same from GnosisChecks:
        /// https://github.com/crdschurch/GnosisChecks/blob/24edc373ae62660028c1637396a9b834dfb2fd4d/Modules.vb#L52
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="routingNumber"></param>
        /// <returns></returns>
        public string CreateHashedAccountAndRoutingNumber(string accountNumber, string routingNumber)
        {
            var crypt = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(string.Concat(routingNumber, accountNumber, HashKey));
            byte[] crypto = crypt.ComputeHash(bytes, 0, bytes.Length);
            var hashString = Convert.ToBase64String(crypto).Replace('/', '~');
            return (hashString);
        }

        public string DecryptCheckValue(string value)
        {
            var valueDecrypt = _crypto.DecryptValue(value);
            return valueDecrypt;
        }

        public int UpdatePaymentProcessorCustomerId(int donorId, string paymentProcessorCustomerId)
        {
            var parms = new Dictionary<string, object> {
                { "Donor_ID", donorId },
                { DonorProcessorId, paymentProcessorCustomerId },
            };

            try
            {
                _ministryPlatformService.UpdateRecord(_donorPageId, parms, ApiLogin());
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                    string.Format("UpdatePaymentProcessorCustomerId failed. donorId: {0}, paymentProcessorCustomerId: {1}", donorId, paymentProcessorCustomerId), e);
            }

            return (donorId);
        }

        public string UpdateDonorAccount(string encryptedKey, string sourceId, string customerId)
        {
            try
            {
                var donorAccount = WithApiLogin(apiToken => _ministryPlatformService.GetPageViewRecords(_donorLookupByEncryptedAccount, apiToken, "," + encryptedKey));
                var donorAccountId = donorAccount[0]["dp_RecordID"].ToString();
                var updateParms = new Dictionary<string, object>
                {
                    {"Donor_Account_ID", donorAccountId},
                    {"Processor_Account_ID", sourceId},
                    {"Processor_ID", customerId}
                };
                _ministryPlatformService.UpdateRecord(_donorAccountsPageId, updateParms, ApiLogin());
                 return donorAccountId;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("UpdateDonorAccount failed.  Donor Account: {0}", encryptedKey), ex);
            }
        }

        public void SetupConfirmationEmail(int programId, int donorId, decimal donationAmount, DateTime setupDate, string pymtType, int pledgeId = 0)
        {
            var apiToken = ApiLogin();
            string pledgeName = null;
            if (pledgeId > 0)
            {
                var contact = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Get<MpContact>("Pledges", pledgeId, "Donor_ID_Table_Contact_ID_Table.Nickname, Donor_ID_Table_Contact_ID_Table.Last_Name");
                pledgeName = contact.Nickname + " " + contact.LastName;
            }
            var program = _programService.GetProgramById(programId);
            //If the communcations admin does not link a message to the program, the default template will be used.
            var communicationTemplateId = program.CommunicationTemplateId != null && program.CommunicationTemplateId != 0
                ? program.CommunicationTemplateId.Value
                : _configurationWrapper.GetConfigIntValue("DefaultGiveConfirmationEmailTemplate");

            SendEmail(communicationTemplateId, donorId, donationAmount, pymtType, setupDate, setupDate, program.Name, EmailReason, null, pledgeName);
        }

        public MpContactDonor GetEmailViaDonorId(int donorId)
        {
            var donor = new MpContactDonor();
            try
            {
                var searchStr = string.Format(",\"{0}\"", donorId);
                var records =
                    WithApiLogin(
                        apiToken => (_ministryPlatformService.GetPageViewRecords("DonorByContactId", apiToken, searchStr)));
                if (records != null && records.Count > 0)
                {
                    var record = records.First();

                    donor.DonorId = record.ToInt("Donor_ID");
                    donor.ProcessorId = record.ToString(DonorProcessorId);
                    donor.ContactId = record.ToInt("Contact_ID");
                    donor.RegisteredUser = true;
                    donor.Email = record.ToString("Email");
                    donor.StatementType = record.ToString("Statement_Type");
                    donor.StatementTypeId = record.ToInt("Statement_Type_ID");
                    donor.StatementFreq = record.ToString("Statement_Frequency");
                    donor.StatementMethod = record.ToString("Statement_Method");
                    donor.Details = new MpContactDetails
                    {
                        EmailAddress = record.ToString("Email"),
                        HouseholdId = record.ToInt("Household_ID")
                    };
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("GetEmailViaDonorId failed.  Donor Id: {0}", donorId), ex);
            }

            return donor;
        }

  

        // TODO Made this virtual so could mock in a unit test.  Probably ought to refactor to a separate class - shouldn't have to mock the class we're testing...
        public virtual void SendEmail(int communicationTemplateId, int donorId, decimal donationAmount, string paymentType, DateTime setupDate, DateTime startDate, string program, string emailReason, string frequency = null, string pledgeName = null)
        {
            var template = _communicationService.GetTemplate(communicationTemplateId);
            var defaultContactId = AppSetting("DefaultGivingContactEmailId");
            var defaultContactEmail = _contactService.GetContactEmail(defaultContactId);
            MpContact contact = _contactService.GetEmailFromDonorId(donorId);
            var comm = new MpCommunication
            {
                
                AuthorUserId = 5,
                DomainId = 1,
                EmailBody = template.Body,
                EmailSubject = template.Subject,
                FromContact =  new MpContact { ContactId = defaultContactId, EmailAddress = defaultContactEmail },
                ReplyToContact = new MpContact { ContactId = defaultContactId, EmailAddress = defaultContactEmail },
                ToContacts = new List<MpContact> {new MpContact{ContactId = contact.ContactId, EmailAddress = contact.EmailAddress}},
                MergeData = new Dictionary<string, object>
                {
                    {"Program_Name", program},
                    {"Donation_Amount", donationAmount.ToString("N2")},
                    {"Donation_Date", setupDate.ToString("MM/dd/yyyy h:mmtt", _dateTimeFormat)},
                    {"Payment_Method", paymentType},
                    {"Decline_Reason", emailReason},
                    {"Start_Date",  startDate.ToString("MM/dd/yyyy", _dateTimeFormat)}
                }
            };

            if (!string.IsNullOrWhiteSpace(frequency))
            {
                comm.MergeData["Frequency"] = frequency;
            }

            if (!string.IsNullOrWhiteSpace(pledgeName))
            {
                comm.MergeData["Pledge_Donor"] = pledgeName;
            }

            _communicationService.SendMessage(comm);
        }

        public List<MpDonation> GetDonations(IEnumerable<int> donorIds, string donationYear = null)
        {
            var search = string.Format("{0},,,,,,,,,,{1}", YearSearch(donationYear), DonorIdSearch(donorIds));
            var records = WithApiLogin(token => _ministryPlatformService.GetRecordsDict(_donationDistributionPageId, token, search));

            return MapDonationRecords(records);
        }

        public List<MpDonation> GetDonations(int donorId, string donationYear = null)
        {
            return (GetDonations(new [] {donorId}, donationYear));
        }

        private List<MpDonationStatus> GetDonationStatuses()
        {
            var statuses = WithApiLogin(token => _ministryPlatformService.GetRecordsDict(_donationStatusesPageId, token));

            if (statuses == null || statuses.Count == 0)
            {
                return (new List<MpDonationStatus>());
            }

            var result = statuses.Select(s => new MpDonationStatus
            {
                DisplayOnGivingHistory = s["Display_On_Giving_History"] as bool? ?? true,
                DisplayOnStatement = s["Display_On_Statements"] as bool? ?? false,
                DisplayOnMyTrips = s["Display_On_MyTrips"] as bool? ?? false,
                Id = s["dp_RecordID"] as int? ?? 0,
                Name = s["Donation_Status"] as string
            }).ToList();

            return (result);
        }

        public List<MpDonation> GetSoftCreditDonations(IEnumerable<int> donorIds, string donationYear = null)
        {
            var search = string.Format("{0},,,,,,,,,,,,,,,,,{1}", YearSearch(donationYear), DonorIdSearch(donorIds));
            var records = WithApiLogin(token => _ministryPlatformService.GetRecordsDict(_donationDistributionPageId, token, search));

            return MapDonationRecords(records);
        }

        public List<MpDonation> GetDonationsForAuthenticatedUser(string userToken, bool? softCredit = null, string donationYear = null, bool? includeRecurring = true)
        {
            var search = string.Format("{0},{1}", YearSearch(donationYear), softCredit.HasValue ? softCredit.Value.ToString() : string.Empty);
            //TODO update search string for includeRecurring, instead of looping through list below
            // like this,  var search = string.Format("{0},{1},,,,,,,,,,,,,,,,,,,,{2}", YearSearch(donationYear), softCredit.HasValue ? softCredit.Value.ToString() : string.Empty, includeRecurring.HasValue ? includeRecurring.Value.ToString() : "true");            
            var records = _ministryPlatformService.GetRecordsDict(_myHouseholdDonationDistributions, userToken, search);
            
            if (includeRecurring.HasValue ? includeRecurring.Value : true)
            {
                return MapDonationRecords(records);
            }

            var donations = MapDonationRecords(records);
            var nonRecurringDonations = new List<MpDonation>();

            if (donations != null)
            {
                
                foreach (MpDonation donation in donations)
                {
                    if (!donation.recurringGift)
                    {
                        nonRecurringDonations.Add(donation);
                    }
                }
            }
            return nonRecurringDonations;
        }

        private List<MpDonation> MapDonationRecords(List<Dictionary<string, Object>> records)
        {
            if (records == null || records.Count == 0)
            {
                return null;
            }

            var statuses = GetDonationStatuses();

            var donationMap = new Dictionary<int, MpDonation>();
            foreach (var record in records)
            {
                var donationId = record["Donation_ID"] as int? ?? 0;

                var donation = GetDonationFromMap(donationMap, record, donationId, statuses);
                AddDistributionToDonation(record, donation);
                donationMap[donation.donationId] = donation;
            }

            return donationMap.Values.ToList();
        }

        private static MpDonation GetDonationFromMap(Dictionary<int, MpDonation> donationMap,
                                            Dictionary<string, Object> record,
                                            int donationId,
                                            List<MpDonationStatus> statuses)
        {
            if (donationMap.ContainsKey(donationId))
            {
                return donationMap[donationId];
            }

            var donation = new MpDonation
            {
                donationDate = record["Donation_Date"] as DateTime? ?? DateTime.Now,
                batchId = null,
                donationId = record["Donation_ID"] as int? ?? 0,
                donationNotes = record["Notes"] as string,
                donationStatus = record["Donation_Status_ID"] as int? ?? 0,
                donationStatusDate = record["Donation_Status_Date"] as DateTime? ?? DateTime.Now,
                donorId = record["Donor_ID"] as int? ?? 0,
                paymentTypeId = record["Payment_Type_ID"] as int? ?? 0,
                transactionCode = record["Transaction_Code"] as string,
                softCreditDonorId = record["Soft_Credit_Donor_ID"] as int? ?? 0,
                donorDisplayName = record["Donor_Display_Name"] as string,
                itemNumber = record["Item_Number"] as string,
                recurringGift = record["Is_Recurring_Gift"] as bool? ?? false,
                AccountingCompanyName = record["Company_Name"] as string,
                AccountingCompanyIncludeOnPrintedStatement = record["Show_Online"] as bool? ?? false
            };

            var status = statuses.Find(x => x.Id == donation.donationStatus) ?? new MpDonationStatus();
            donation.IncludeOnGivingHistory = status.DisplayOnGivingHistory;
            donation.IncludeOnPrintedStatement = status.DisplayOnStatement && donation.AccountingCompanyIncludeOnPrintedStatement;

            // Determine whether this payment was processed by Forte (i.e., previous processor before Stripe).
            // If it's legacy, clear out the transaction code so that we don't try to call stripe later with
            // an invalid (from Stripe's perspective) payment ID.
            object legacy;
            if (record.TryGetValue("Is_Legacy", out legacy) && legacy.ToString() == "True")
                donation.transactionCode = null;

            return donation;
        }

        private static void AddDistributionToDonation(Dictionary<string, Object> record, MpDonation donation)
        {

            var amount = Convert.ToInt32((record["Amount"] as decimal? ?? 0) * Constants.StripeDecimalConversionValue);
            donation.donationAmt += amount;

            donation.Distributions.Add(new MpDonationDistribution
            {
                donationDistributionProgram = record["dp_RecordName"] as string,
                donationDistributionAmt = amount
            });
        }

        private static string YearSearch(string year)
        {
            return string.IsNullOrWhiteSpace(year) ? string.Empty : string.Format("\"*/{0}*\"", year);
        }

        private static string DonorIdSearch(IEnumerable<int> ids)
        {
            return string.Join(" or ", ids.Select(id => string.Format("\"{0}\"", id)));
        }

        public int CreateRecurringGiftRecord(int donorId, 
                                             int donorAccountId, 
                                             string planInterval, 
                                             decimal planAmount, 
                                             DateTime startDate, 
                                             string program, 
                                             string subscriptionId, 
                                             int congregationId, 
                                             string sourceUrl = null, 
                                             decimal? predefinedAmount = null)
        {
            // Make sure we're talking in UTC consistently
            startDate = startDate.ToUniversalTime().Date;

            int? dayOfWeek = null;
            int? dayOfMonth = null;
            int frequencyId;
            if (planInterval == "week")
            {
                dayOfWeek = NumericDayOfWeek.GetDayOfWeekID((startDate.DayOfWeek).ToString());
                frequencyId = 1;
            }
            else
            {
                dayOfMonth = startDate.Day;
                frequencyId = 2;
            }
          
            var values = new Dictionary<string, object>
            {
                {"Donor_ID", donorId},
                {"Donor_Account_ID", donorAccountId},
                {"Frequency_ID", frequencyId},
                {"Day_Of_Month", dayOfMonth},    
                {"Day_Of_Week_ID", dayOfWeek},
                {"Amount", planAmount},
                {"Start_Date", startDate},
                {"Program_ID", program},
                {"Congregation_ID", congregationId},
                {"Subscription_ID", subscriptionId},
                {"Source_Url", sourceUrl},
                {"Predefined_Amount", predefinedAmount}
            };

            int recurringGiftId;
            try
            {
                var authorizedUserToken = _apiUserRepository.GetDefaultApiClientToken();
                recurringGiftId = _ministryPlatformService.CreateRecord(_myHouseholdDonationRecurringGifts, values, authorizedUserToken, true);
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("Create Recurring Gift failed.  Donor Id: {0}", donorId), e);
            }

            return recurringGiftId;
        }

        public MpCreateDonationDistDto GetRecurringGiftById(string authorizedUserToken, int recurringGiftId)
        {
            var searchStr = string.Format("\"{0}\",", recurringGiftId);
            MpCreateDonationDistDto createDonation = null;
            try
            {
                var records = _ministryPlatformService.GetPageViewRecords(_myHouseholdRecurringGiftsApiPageView, authorizedUserToken, searchStr);
                if (records != null && records.Any())
                {
                    var record = records.First();
                    createDonation = new MpCreateDonationDistDto
                    {
                        RecurringGiftId = record.ToNullableInt("Recurring_Gift_ID"),
                        DonorId = record.ToInt("Donor_ID"),
                        Frequency = record.ToInt("Frequency_ID"),
                        DayOfWeek = record.ToInt("Day_Of_Week_ID"),
                        DayOfMonth = record.ToInt("Day_Of_Month"),
                        StartDate = record.ToDate("Start_Date").ToUniversalTime().Date,
                        Amount = (int)((record["Amount"] as decimal? ?? 0.00M) * Constants.StripeDecimalConversionValue),
                        ProgramId = record.ToString("Program_ID"),
                        CongregationId = record.ToInt("Congregation_ID"),
                        PaymentType = (int)AccountType.Checking == record.ToInt("Account_Type_ID") ? PaymentType.Bank.abbrv : PaymentType.CreditCard.abbrv,
                        DonorAccountId = record.ToNullableInt("Donor_Account_ID"),
                        SubscriptionId = record.ToString("Subscription_ID"),
                        StripeCustomerId = record.ToString("Processor_ID"),
                        StripeAccountId = record.ToString("Processor_Account_ID"),
                        Recurrence = record.ToString("Recurrence"),
                        ProgramName = record.ToString("Program_Name")
                    };
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("GetRecurringGift failed.  Recurring Gift Id: {0}", recurringGiftId), ex);
            }

            return createDonation;
        }

        public MpCreateDonationDistDto GetRecurringGiftById(int recurringGiftId)
        {
            var token = _apiUserRepository.GetDefaultApiClientToken();
            var columns = new List<string>
            {
                "Recurring_Gifts.[Recurring_Gift_ID]",
                "Donor_ID_Table.[Donor_ID]",
                "Frequency_ID_Table.[Frequency_ID]",
                "Day_Of_Week_ID_Table.[Day_Of_Week_ID]",
                "Recurring_Gifts.[Day_Of_Month]",
                "Recurring_Gifts.[Start_Date]",
                "Recurring_Gifts.[Amount]",
                "Program_ID_Table.[Program_ID]",
                "Program_ID_Table_Congregation_ID_Table.[Congregation_ID]",
                "Donor_Account_ID_Table_Account_Type_ID_Table.[Account_Type_ID]",
                "Donor_Account_ID_Table.[Donor_Account_ID]",
                "Recurring_Gifts.[Subscription_ID]",
                "Donor_Account_ID_Table.[Processor_ID]",
                "Donor_Account_ID_Table.[Processor_Account_ID]",
                "Program_ID_Table.[Program_Name]",
            };
            var filter = $"Recurring_Gifts.[Recurring_Gift_ID] = {recurringGiftId}";
            var results = _ministryPlatformRestRepository.UsingAuthenticationToken(token)
                .Search<MpCreateDonationDistDto>(filter, columns);
            if (results != null && results.Any())
            {
                var result = results.First();
                result.StartDate = result.StartDate?.ToUniversalTime().Date;
                result.Amount = (int) ((result.Amount as decimal? ?? 0.00M) *
                                       Constants.StripeDecimalConversionValue);
                result.PaymentType = (int) AccountType.Checking == Convert.ToInt32(result.PaymentType)
                    ? PaymentType.Bank.abbrv
                    : PaymentType.CreditCard.abbrv;
                return result;
            }
            return null;
        }

        public MpCreateDonationDistDto GetRecurringGiftForSubscription(string subscription, string optionalSourceId = "")
        {
            string searchStr;
            if (!string.IsNullOrEmpty(optionalSourceId))
            {
                searchStr = string.Format(",{0},,,,,,,,,,{1}", subscription, optionalSourceId);
            }
            else
            {
                searchStr = string.Format(",\"{0}\",", subscription);
            }
            MpCreateDonationDistDto createDonation = null;
            try
            {
                var records =
                    WithApiLogin(
                        apiToken => (_ministryPlatformService.GetPageViewRecords(_recurringGiftBySubscription, apiToken, searchStr)));
                if (records != null && records.Count > 0)
                {
                    var record = records.First();
                    createDonation = new MpCreateDonationDistDto
                    {
                        DonorId = record.ToInt("Donor_ID"),
                        Amount = record["Amount"] as decimal? ?? 0,
                        ProgramId = record.ToString("Program_ID"),
                        CongregationId = record.ToInt("Congregation_ID"),
                        PaymentType = (int)AccountType.Checking == record.ToInt("Account_Type_ID") ? PaymentType.Bank.abbrv : PaymentType.CreditCard.abbrv,
                        RecurringGiftId = record.ToNullableInt("Recurring_Gift_ID"),
                        DonorAccountId = record.ToNullableInt("Donor_Account_ID"),
                        SubscriptionId = record.ToString("Subscription_ID"),
                        Frequency = record.ToInt("Frequency_ID"),
                        ConsecutiveFailureCount = record.ToInt("Consecutive_Failure_Count"),
                        StripeCustomerId = record.ToString("Processor_ID"),
                        StripeAccountId = record.ToString("Processor_Account_ID")
                    };
                }
                
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("GetRecurringGift failed.  Subscription Id: {0}", subscription), ex);
            }

           return createDonation;
        }

        public List<MpRecurringGift> GetRecurringGiftsForAuthenticatedUser(string userToken)
        {
            var records = _ministryPlatformService.GetRecordsDict(_myHouseholdDonationRecurringGifts, userToken);
            return records.Select(MapRecordToRecurringGift).ToList();
        }

        public void ProcessRecurringGiftDecline(string subscriptionId, string error)
        {
            var recurringGift = GetRecurringGiftForSubscription(subscriptionId);
            UpdateRecurringGiftFailureCount(recurringGift.RecurringGiftId.Value, recurringGift.ConsecutiveFailureCount + 1);
            
            var acctType = GetDonorAccountPymtType(recurringGift.DonorAccountId.Value);
            var paymentType = PaymentType.GetPaymentType(acctType).name;
            var templateId = recurringGift.ConsecutiveFailureCount >= 2 ? PaymentType.GetPaymentType(acctType).recurringGiftCancelEmailTemplateId : PaymentType.GetPaymentType(acctType).recurringGiftDeclineEmailTemplateId;
            var frequency = recurringGift.Frequency == 1 ? "Weekly" : "Monthly";
            var program = _programService.GetProgramById(Convert.ToInt32(recurringGift.ProgramId));
            var startDate = recurringGift.StartDate.HasValue ? recurringGift.StartDate.Value : DateTime.Now;
            var amt = decimal.Round(recurringGift.Amount, 2, MidpointRounding.AwayFromZero);

            SendEmail(templateId, recurringGift.DonorId, amt, paymentType, DateTime.Now, startDate, program.Name, error, frequency);
        }

        public int GetDonorAccountPymtType(int donorAccountId)
        {
            var donorAccount = _ministryPlatformService.GetRecordDict(_donorAccountsPageId, donorAccountId, ApiLogin());

            if (donorAccount == null)
            {
               throw new ApplicationException(
                   string.Format("Donor Account not found.  Donor Account Id: {0}", donorAccountId));
            }
            
            return donorAccount.ToInt("Account_Type_ID");
        }
        
        // ReSharper disable once FunctionComplexityOverflow
        private MpRecurringGift MapRecordToRecurringGift(Dictionary<string, object> record)
        {
            return new MpRecurringGift
            {
                RecurringGiftId = record["Recurring_Gift_ID"] as int? ?? 0,
                DonorID = record["Donor_ID"] as int? ?? 0,
                EmailAddress = record["User_Email"] as string,
                Frequency = (record["Frequency"] as string ?? string.Empty).Trim(),
                Recurrence = record["Recurrence"] as string,
                StartDate = (record["Start_Date"] as DateTime? ?? DateTime.Now).ToUniversalTime().Date,
                EndDate = record["End_Date"] as DateTime?,
                Amount = record["Amount"] as decimal? ?? 0,
                ProgramID = record["Program_ID"] as int? ?? 0,
                ProgramName = record["Program_Name"] as string,
                CongregationName = record["Congregation_Name"] as string,
                AccountTypeID = record["Account_Type_ID"] as int? ?? 0,
                AccountNumberLast4 = record["Account_Number"] as string,
                InstitutionName = record["Institution_Name"] as string,
                SubscriptionID = record["Subscription_ID"] as string,
                ProcessorAccountId = record["Processor_Account_ID"] as string,
                ProcessorId = record["Processor_ID"] as string
            };
        }

        public MpDonorStatement GetDonorStatement(int contactId)
        {
            var records = GetContactDonor(contactId);

            if (records == null || records.DonorId == 0)
            {
                return null;
            }

            var postalStatementId = _configurationWrapper.GetConfigValue("PostalMailStatement");
            var statementMethod = new MpDonorStatement();
            statementMethod.DonorId = records.DonorId;
            
            statementMethod.Paperless = records.StatementMethodId.ToString() != postalStatementId;
            return statementMethod;
        }

        public void UpdateDonorStatement(MpDonorStatement statement)
        {
            var token = ApiLogin();
            var onlineStatementId = _configurationWrapper.GetConfigValue("EmailOnlineStatement");
            var postalStatementId = _configurationWrapper.GetConfigValue("PostalMailStatement");

            var dictionary = new Dictionary<string, object>
            {
                {"Donor_ID", statement.DonorId},
                {"Statement_Method_ID", statement.Paperless ? onlineStatementId : postalStatementId},
            };

            try
            {
                _ministryPlatformRestRepository.UsingAuthenticationToken(token).UpdateRecord("Donors", statement.DonorId, dictionary);
            }
            catch (Exception e)
            {
                var msg = string.Format("Error updating statement method attribute, donor: {0} paperless: {1}",
                                        statement.DonorId, statement.Paperless);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }
    }
}

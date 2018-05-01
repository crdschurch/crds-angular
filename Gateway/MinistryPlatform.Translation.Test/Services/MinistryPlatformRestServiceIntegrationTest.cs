using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Childcare;
using MinistryPlatform.Translation.Models.GoCincinnati;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Models.Rules;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;

namespace MinistryPlatform.Translation.Test.Services
{
    [Category("IntegrationTests")]
    public class MinistryPlatformRestServiceIntegrationTest
    {
        private MinistryPlatformRestRepository _fixture;

        private string _authToken;

        [TestFixtureSetUp]
        public void SetupAll()
        {
            var config = new ConfigurationWrapper();
            var authRestClient = new HttpClient
            {
                BaseAddress = new Uri(config.GetEnvironmentVarAsString("MP_OAUTH_BASE_URL"))
            };

            var mpRestClient = new HttpClient
            {
                BaseAddress = new Uri(config.GetEnvironmentVarAsString("MP_REST_API_ENDPOINT"))
            };
            var authenticationRepository = new AuthenticationRepository(authRestClient, mpRestClient);
            var apiUserRepository = new ApiUserRepository(config, authenticationRepository, mpRestClient);
            _authToken = apiUserRepository.GetDefaultApiUserToken();
        }

        [SetUp]
        public void SetUp()
        {
            var ministryPlatformRestClient = new RestClient(Environment.GetEnvironmentVariable("MP_REST_API_ENDPOINT"));
            var authenticationRestClient = new RestClient(Environment.GetEnvironmentVariable("MP_OAUTH_BASE_URL"));
            _fixture = new MinistryPlatformRestRepository(ministryPlatformRestClient, authenticationRestClient);
        }       

        [Test]
        public void TestChildcareDashboardProcedure()
        {
            Console.WriteLine("TestCallingAStoredProcedure");
            var parms = new Dictionary<string, object>()
            {
                {"@Domain_ID", 1},
                {"@Contact_ID", 2186211}
            };
            var results = _fixture.UsingAuthenticationToken(_authToken).GetFromStoredProc<MpChildcareDashboard>("api_crds_getChildcareDashboard", parms);
            foreach (var p in results)
            {               
                Console.WriteLine("Result\t{0}", p.FirstOrDefault());
            }
        }

        [Test]
        public void TestGetPotentialMatches()
        {
            var firstName = "Philip";
            var lastName = "Smith";
            var email = "p.smith6590@gmail.com";

            string filter3 = $"First_Name = '{firstName}' AND Last_Name = '{lastName}'";
            string filter4 = $"(First_Name = '{firstName}' AND Last_Name = '{lastName}') OR Email_Address = '{email}'";
            var columns = new List<string>
            {
                "First_Name",
                "Last_Name",
                "Email_Address"
            };
            try
            {
                var records2 = _fixture.UsingAuthenticationToken(_authToken).Search<MpMyContact>(filter4, columns);
                foreach (var p in records2)
                {
                    Console.WriteLine("Result\t{0}", p.Contact_ID);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        
        [Test]
        public void TestChildRsvpdProcedure()
        {
            Console.WriteLine("TestCallingAStoredProcedure");
            var parms = new Dictionary<string, object>()
            {
                {"@ContactId", 7658083},
                {"@EventGroupID", 175596}
            };
            var results = _fixture.UsingAuthenticationToken(_authToken).GetFromStoredProc<MPRspvd>("api_crds_ChildRsvpd", parms);
            foreach (var p in results)
            {
                Console.WriteLine("Result\t{0}", p.FirstOrDefault().Rsvpd);
            }
        }

        [Test]
        public void TestChildcareEmailProcedure()
        { 
            Console.WriteLine("TestCallingAStoredProcedure");
            var parms = new Dictionary<string, object>()
            {                
                {"@DaysOut", 7}
            };
            var results = _fixture.UsingAuthenticationToken(_authToken).GetFromStoredProc<MpContact>("api_crds_ChildcareReminderEmails", parms);
            foreach (var p in results)
            {
                Console.WriteLine("Result\t{0}", p.FirstOrDefault().EmailAddress);
            }
        }

        [Test]
        public void TestChildcareRequestDatesProcedure()
        {
            Console.WriteLine("TestChildcareRequestDatesProcedure");
            var parms = new Dictionary<string, object>()
            {
                {"@ChildcareRequestID", 179}
            };
            var results = _fixture.UsingAuthenticationToken(_authToken).PostStoredProc("api_crds_DeleteDatesForChildcareRequest", parms);

            Console.WriteLine("Results\t" + results.ToString());

        }

        [Test]
        public void TestEndDateGroup()
        {
            Console.WriteLine("TestEndDateGroup");
            var groupId = 177074;
            var reasonEnded = 1;
            var fields = new Dictionary<string, object>
            {
                {"Group_ID", groupId },
                {"End_Date", DateTime.Today},
                {"Reason_Ended", reasonEnded}
            };
            _fixture.UsingAuthenticationToken(_authToken).UpdateRecord("Groups", groupId, fields);
        }

        [Test]
        public void TestTripParticipantProcedure()
        {
            Console.WriteLine("TestTripParticipantProcedure");
            var fields = new Dictionary<string, object>
            {
                {"@PledgeCampaignID", 10000000},
                {"@ContactId", 2186211 }
            };
            var results = _fixture.UsingAuthenticationToken(_authToken).GetFromStoredProc<MpPledge>("api_crds_Add_As_TripParticipant", fields);
            Console.WriteLine("Result\t" + results.ToString());
        }

        [Test]
        public void TestGetEvents()
        {
            const int eventId = 4532768;
            Console.WriteLine("Getting Event");
            var results = _fixture.UsingAuthenticationToken(_authToken).Search<MpCamp>($"Event_ID={eventId}");

            Assert.AreEqual(results.Count, 1);

            foreach (var p in results)
            {
                Console.WriteLine("Event\t{0}", p);
            }

        }

        [Test]
        public void TestGetEvents2()
        {
            var eventId = 4525285;

            var mpevent = _fixture.UsingAuthenticationToken(_authToken).Get<MpEvent>(eventId);
            mpevent.PrimaryContact = _fixture.UsingAuthenticationToken(_authToken).Get<MpContact>(mpevent.PrimaryContactId);

            Assert.AreNotEqual(mpevent.PrimaryContact.ContactId, null);
        }

        [Test]
        public void TestRegisterParticipant()
        {
            Console.WriteLine("TestRegisterParticipant");
            var eventParticipantId = 7676452;
            var participantStatus = 2;
            var fields = new Dictionary<string, object>
            {
                {"Event_Participant_ID", eventParticipantId},
                {"End_Date", null},
                {"Participation_Status_ID", participantStatus}
            };
            _fixture.UsingAuthenticationToken(_authToken).UpdateRecord("Event_Participants", eventParticipantId, fields);
        }       

        [Test]
        public void TestGetGoTripsWithForms()
        {
            var pledgeCampaignId = 2502;
            var columnList = new List<string>
            {
                "Pledge_Campaigns.Pledge_Campaign_ID",
                "Pledge_Campaigns.Campaign_Name",
                "Pledge_Campaign_Type_ID_Table.Campaign_Type",
                "Pledge_Campaigns.Start_Date",
                "Pledge_Campaigns.[End_Date]",
                "Pledge_Campaigns.[Campaign_Goal]",
                "Registration_Form_Table.[Form_ID]",
                "Registration_Form_Table.[Form_Title]",
                "Pledge_Campaigns.[Registration_Start]",
                "Pledge_Campaigns.[Registration_End]",
                "Pledge_Campaigns.[Registration_Deposit]",
                "Pledge_Campaigns.[Youngest_Age_Allowed]",
                "Event_ID_Table.[Event_Start_Date]",
                "Pledge_Campaigns.[Nickname]",
                "Event_ID_Table.[Event_ID]",
                "Pledge_Campaigns.[Program_ID]",
                "Pledge_Campaigns.Maximum_Registrants"
            };

            var results = _fixture.UsingAuthenticationToken(_authToken).Search<MpPledgeCampaign>($"Pledge_Campaigns.[Pledge_Campaign_ID] = {pledgeCampaignId}", columnList);
            Assert.AreEqual(1, results.Count);
            Assert.IsNotNull(results[0].MaximumRegistrants);
        }

        [Test]
        public void TestGetPaymentType()
        {
            Console.WriteLine("TestGetPaymentType");
            var p = _fixture.UsingAuthenticationToken(_authToken).Get<MyPaymentType>(2);
            Console.WriteLine("Payment_Type\t{0}", p);
        }

        [Test]
        public void ShouldGetDataFromTableByName()
        {
            var contact = _fixture.UsingAuthenticationToken(_authToken).Get<MpContact>("Pledges", 656098, "Donor_ID_Table_Contact_ID_Table.Nickname, Donor_ID_Table_Contact_ID_Table.Last_Name");
            Assert.AreEqual("Connie", contact.Nickname);
            Assert.AreEqual("McNerney", contact.LastName);
        }

        [Test]
        public void GradeGroupsInCurrentCampEvents()
        {
            var columnList = new List<string>
            {
                "Event_ID_Table_Event_Type_ID_Table.[Event_Type_ID]",
                "Group_ID_Table.[Group_ID]",
                "Group_ID_Table_Group_Type_ID_Table.[Group_Type_ID]"
            };

            var date = DateTime.Today;

            var filter = "Event_ID_Table_Event_Type_ID_Table.[Event_Type_ID] = 8 AND Group_ID_Table_Group_Type_ID_Table.[Group_Type_ID] = 4 " +
                         $"AND '{date}' between Event_ID_Table.[Registration_Start] and Event_ID_Table.[Registration_End]";
            var groups = _fixture.UsingAuthenticationToken(_authToken).Search<MpEventGroup>(filter, columnList);
            foreach (MpEventGroup eg in groups)
            {
                Console.WriteLine(eg);
            };
        }

        [Test]
        public void ContactNotInGradeGroup()
        {
            var storedProcOpts = new Dictionary<string, object>
            {
                {"@ContactId", 1234 },
                {"@EventID", 4525285}
            };
            var result = _fixture.UsingAuthenticationToken(_authToken).GetFromStoredProc<MpStoredProcBool>("api_crds_Grade_Group_Participant_For_Camps", storedProcOpts);
            var l = result.FirstOrDefault();
            foreach (var r in l)
            {
                Assert.IsFalse(r.isTrue);                
            }
        }

        [Test]
        public void ContactInAGradeGroup()
        {
            var storedProcOpts = new Dictionary<string, object>
            {
                {"@ContactId", 7672203},
                {"@EventID", 4525325}
            };
            var result = _fixture.UsingAuthenticationToken(_authToken).GetFromStoredProc<MpStoredProcBool>("api_crds_Grade_Group_Participant_For_Camps", storedProcOpts);
            var l = result.FirstOrDefault();
            foreach (var r in l)
            {
                Assert.IsTrue(r.isTrue);
            }
        }
        
        public void ShouldCreateARecord()
        {
            var payment = new MpPayment
            {
                PaymentTotal = 123.45M,
                ContactId = 3717387,
                PaymentDate = DateTime.Now,
                PaymentTypeId = 11
            };

            var paymentDetail = new MpPaymentDetail
            {
                Payment = payment,
                PaymentAmount = 123.45M,
                InvoiceDetailId = 19
            };
            var resp = _fixture.UsingAuthenticationToken(_authToken).Post(new List<MpPaymentDetail> {paymentDetail});

        }

        [Test]
        public void ShouldUpdate2GenericRecord()
        {
            var tableName = "Invoices";

            var dict = new Dictionary<string, object>();
            dict.Add("Invoice_ID",8);
            dict.Add("Invoice_Status_ID", 2);

            var dict2 = new Dictionary<string, object>();
            dict2.Add("Invoice_ID", 9);
            dict2.Add("Invoice_Status_ID", 2);

            var thelist = new List<Dictionary<string, object>>();
            thelist.Add(dict);
            thelist.Add(dict2);

            var resp = _fixture.UsingAuthenticationToken(_authToken).Put(tableName,thelist);
        }

        [Test]
        public void TestGetWithFilter()
        {
            Console.WriteLine(" TestGetWithFilter");
            var invoiceId = 8;
            var fields = new Dictionary<string, object>
            {
                {"Invoice_Number", invoiceId }
            };
            var results = _fixture.UsingAuthenticationToken(_authToken).Get<MpPayment>("Payments", fields);
            Console.WriteLine("Result\t" + results.ToString());
        }

        [Test]
        public void TestGetASingleIntValue()
        {
            var contactId = 7681520;
            var eventId = 4525325;
            var tableName = "Event_Participants";
            var searchString = $"Event_ID_Table.Event_ID={eventId} AND Participant_ID_Table_Contact_ID_Table.Contact_ID={contactId}";
            var column = "Event_Participant_ID";
            var results = _fixture.UsingAuthenticationToken(_authToken).Search<int>(tableName, searchString, column, null, false);
        }

        [Test]
        public void TestGetASingleStringValue()
        {
            var contactId = 7681520;
            var eventId = 4525325;
            var tableName = "Event_Participants";
            var searchString = $"Event_ID_Table.Event_ID={eventId} AND Participant_ID_Table_Contact_ID_Table.Contact_ID={contactId}";
            var column = "Event_ID_Table.Event_Title";
            var results = _fixture.UsingAuthenticationToken(_authToken).Search<string>(tableName, searchString, column, null, false);
        }

        [Test]
        public void TestRegistrantMessage()
        {
            var searchString = $"Events.[Event_ID]={452345685}";
            _fixture.UsingAuthenticationToken(_authToken).Search<int>("Events", searchString, "Registrant_Message", null, false);
        }

        [Test]
        public void TestProcessingFeeProgramId()
        {
            var res = _fixture.UsingAuthenticationToken(_authToken).Search<int>("GL_Account_Mapping", "GL_Account_Mapping.Program_ID=127 AND GL_Account_Mapping.Congregation_ID=5", "Processor_Fee_Mapping_ID_Table.Program_ID", null, false);
            Assert.AreEqual(127, res);
        }

        [Test]
        public void postPayment()
        {
            var paymentDetail = new MpPaymentDetail()
            {
                InvoiceDetailId = 154,
                PaymentAmount = 2.0M,
                Payment = new MpPayment()
                {
                    ContactId = 2186211,
                    Currency = "usd",
                    InvoiceNumber = "8",
                    TransactionCode = "23423598",
                    PaymentDate = DateTime.Now,
                    PaymentTotal = 2.0M,
                    PaymentTypeId = 4,
                    PaymentStatus = 2 // Deposited
                }
            };
            var paymentList = new List<MpPaymentDetail>
            {
                paymentDetail
            };
            var result = _fixture.UsingAuthenticationToken(_authToken).PostWithReturn<MpPaymentDetail, MpPaymentDetailReturn>(paymentList);
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.First().PaymentId);
        }

        [Test]
        public void TestUpdatePayment()
        {
            var parms = new Dictionary<string, object>
            {
                {"Payment_ID", 8},
                {"Batch_ID", 399484}
            };
            var parmList = new List<Dictionary<string, object>> {parms};
            var results = _fixture.UsingAuthenticationToken(_authToken).Put("Payments", parmList);

            Assert.IsTrue(results > 0);
        }

        [Test]
        public void TestSearchAllPaymentTypes()
        {
            Console.WriteLine("TestSearchAllPaymentTypes");
            var results = _fixture.UsingAuthenticationToken(_authToken).Search<MyPaymentType>();

            foreach (var p in results)
            {
                Console.WriteLine("Payment_Type\t{0}", p);
            }
        }

        [Test]
        public void TestSearchPaymentTypes()
        {
            Console.WriteLine("TestSearchPaymentTypes");
            var results = _fixture.UsingAuthenticationToken(_authToken).Search<MyPaymentType>("Payment_Type_Id > 5");

            foreach (var p in results)
            {
                Console.WriteLine("Payment_Type\t{0}", p);
            }
        }

        [Test]
        public void TestSearchPaymentTypesSelectColumns()
        {
            Console.WriteLine("TestSearchPaymentTypesSelectColumns");
            var results = _fixture.UsingAuthenticationToken(_authToken).Search<MyPaymentType>("Payment_Type_Id > 5", "Payment_Type_Id,Payment_Type");

            foreach (var p in results)
            {
                Console.WriteLine("Payment_Type\t{0}", p);
            }
        }

        [Test]
        public void TestSearchPledgesByContactAndCampaign()
        {
            Console.WriteLine("TestSearchPledgesByContactAndCampaign");
            var results = _fixture.UsingAuthenticationToken(_authToken).Search<MpPledge>("Donor_ID_Table_Contact_ID_Table.Contact_ID=2186211 AND Pledge_Campaign_ID_Table.Pledge_Campaign_ID=10000000 AND Pledge_Status_ID_Table.Pledge_Status_ID=1",
                "Pledges.Pledge_ID,Donor_ID_Table.Donor_ID,Pledge_Campaign_ID_Table.Pledge_Campaign_ID,Pledge_Campaign_ID_Table.Campaign_Name,Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Pledge_Campaign_Type_ID,Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Campaign_Type,Pledge_Campaign_ID_Table.Start_Date,Pledge_Campaign_ID_Table.End_Date,Pledge_Status_ID_Table.Pledge_Status_ID,Pledge_Status_ID_Table.Pledge_Status,Pledges.Total_Pledge");

            foreach (var p in results)
            {
                Console.WriteLine("CampaignName:\t{0}", p.CampaignName);
                Console.WriteLine("DonorId:\t{0}", p.DonorId);
                Console.WriteLine("PledgeId:\t{0}", p.PledgeId);
                Console.WriteLine("PledgeDonations:\t{0}", p.PledgeDonations);
            }
        }

        [Test]
        public void GetFamilyMembersOfContactId()
        {
            Console.WriteLine("TestCallingAStoredProcedure");
            var parms = new Dictionary<string, object>()
            {
                {"@Contact_ID", 2186211}
            };
            var results = _fixture.UsingAuthenticationToken(_authToken).GetFromStoredProc<MpContact>("api_crds_All_Family_Members", parms);
            foreach (var p in results)
            {
                Console.WriteLine("Result\t{0}", p.FirstOrDefault());
            }
        }

        [Test]
        public void GetEventParticipant()
        {
            var eventId = 4525285;
            var contactId = 7646177;
            var filter = $"Event_ID_Table.[Event_ID]={eventId} AND Participant_ID_Table_Contact_ID_Table.[Contact_ID] = {contactId}";
            var columns = new List<string>
            {
                "Participant_ID_Table_Contact_ID_Table.[Contact_ID]",
                "Event_ID_Table.[Event_ID]",
                "Event_Participant_ID",
                "Event_ID_Table.Event_Title",
                "Participation_Status_ID",
                "End_Date"
            };
            var participants = _fixture.UsingAuthenticationToken(_authToken).Search<MpEventParticipant>(filter, columns);
        }

        [Test]
        public void GetGenderRules()
        {
            var searchString = $"Ruleset_ID = 1";
            var genderRules = _fixture.UsingAuthenticationToken(_authToken).Search<MPGenderRule>(searchString);
        }

        [Test]
        public void GetProductRuleset()
        {
            const string searchString = "Product_ID_Table.[Product_ID] = 8";
            const string columnList = "Product_ID_Table.[Product_ID],Ruleset_ID_Table.[Ruleset_ID],cr_Product_Ruleset.[Start_Date],cr_Product_Ruleset.[End_Date]";
            var res = _fixture.UsingAuthenticationToken(_authToken).Search<MPProductRuleSet>(searchString, columnList);
            Assert.IsNotEmpty(res);
        } 
        
        public void findCongregation()
        {
            var searchString = $"Congregations.[Congregation_Name]='Oakley'";
            var result = _fixture.UsingAuthenticationToken(_authToken).Search<MpCongregation>(searchString);
        }

        [Test]
        public void findProgramTemplateFromEvent()
        {
            var filter = $"Events.[Event_ID] = 4532768";
            const string column = "Online_Registration_Product_Table_Program_ID_Table_Communication_ID_Table.[Communication_ID]";
            var result = _fixture.UsingAuthenticationToken(_authToken).Search<int>("Events", filter, column, null, false);
            Assert.AreEqual(2006, result);            
        }

        [Test]
        public void findOtherHouseholdMembers()
        {
            var filter = $"Contact_Households.Household_ID=1709940";
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
            var result = _fixture.UsingAuthenticationToken(_authToken).Search<MpContactHousehold>(filter, columns);
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(result.First().EndDate);
        }
        
        [Test]
        public void GetAllUncancelledInvoicesForEventParticipant()
        {
            var filter = new Dictionary<string, object> { { "Event_Participant_ID", 7720246 } };
            var result =  _fixture.UsingAuthenticationToken(_authToken)
                .Get<MpInvoiceDetail>("Invoice_Detail", filter);
            Assert.IsFalse(result.Where(i => i.InvoiceStatusId != 7).ToList().Any());
        }

        [Test]
        public void GetProject()
        {
            const string filter = "Project_ID=564 AND Initiative_ID_Table.[Volunteer_Signup_Start_Date]<=GetDate() AND Initiative_ID_Table.[Volunteer_Signup_End_Date]>=GetDate()";
            var columns = new List<string>
            {
                "Project_ID",
                "Project_Name",
                "Project_Status_ID",
                "Location_ID",
                "Project_Type_ID",
                "Organization_ID",
                "cr_Projects.Initiative_ID",
                "Address_ID_Table.Address_ID",
                "Address_ID_Table.[City]",
                "Address_ID_Table.[State/Region] AS [State]",

            };
            var result = _fixture.UsingAuthenticationToken(_authToken).Search<MpProject>(filter, columns, null, true);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetGroupConnectorWithProjectId()
        {
            const int projectId = 564;
            var groupConnectors = _fixture.UsingAuthenticationToken(_authToken).Search<MpGroupConnector>($"Project_ID = {projectId}");

            Assert.NotNull(groupConnectors);
        }

        [Test]
        public void GetRegistrationsForProjectId()
        {
            var searchString = "Group_Connector_ID_Table_Project_ID_Table.[Project_ID] = 26";
            var columnList =
                "Group_Connector_ID_Table_Project_ID_Table.[Project_ID], Registration_ID_Table_Participant_ID_Table_Contact_ID_Table.[Nickname], Registration_ID_Table_Participant_ID_Table_Contact_ID_Table.[Last_Name], Registration_ID_Table_Participant_ID_Table_Contact_ID_Table.[Mobile_Phone], Registration_ID_Table_Participant_ID_Table_Contact_ID_Table.[Email_Address], Registration_ID_Table.[Spouse_Participation], Registration_ID_Table.[_Family_Count]";
            var registrations = _fixture.UsingAuthenticationToken(_authToken).Search<MpProjectRegistration>(searchString, columnList);

            Assert.NotNull(registrations);
        }

        [Test]
        public void ShouldSetHouseholdCongregation()
        {

            var household = new MpHousehold
            {
                Household_ID = 1709940,
                Congregation_ID = 21,
                Address_ID = 2928137
            };
            var result = _fixture.UsingAuthenticationToken(_authToken).Update<MpHousehold>(household);
            Assert.NotNull(result);
        }

        [Test]
        public void ShouldGetParticpantByContactId()
        {
            const int id = 2186211;
            var searchString = $"Contact_ID_Table.[Contact_ID]={id}";
            var columnList = new List<string>
            {
                "Contact_ID_Table.Contact_ID",
                "Participants.[Participant_ID]",
                "Contact_ID_Table.Email_Address",
                "Contact_ID_Table.Nickname as [NickName]",
                "Contact_ID_Table.Display_Name",
                "Contact_ID_Table.Nickname",
                "Contact_ID_Table.__Age as [Age]",
                "Participants.Attendance_Start_Date",
                "Participant_Type_ID_Table.Participant_Type",
                "Group_Leader_Status_ID_Table.Group_Leader_Status_ID"
            };
            var participant = _fixture.UsingAuthenticationToken(_authToken).Search<MpParticipant>(searchString, columnList);
            Assert.IsNotNull(participant);
            Assert.AreEqual(participant.Count, 1);
            Assert.IsNotEmpty(participant.First().EmailAddress);
            Assert.AreEqual(participant.First().ContactId, id);
            Assert.AreNotEqual(0, participant.First().ParticipantId);
            Assert.AreNotEqual(0, participant.First().Age);
            Assert.IsNotEmpty(participant.First().PreferredName);
            Assert.IsNotEmpty(participant.First().DisplayName);
            Assert.IsNotEmpty(participant.First().Nickname);
            Assert.IsNotNull(participant.First().AttendanceStart);
            Assert.IsTrue(participant.First().ApprovedSmallGroupLeader);
            Assert.AreEqual("Attendee", participant.First().Role);
            Assert.AreEqual(2, participant.First().GroupLeaderStatus);
        }
    }

    [MpRestApiTable(Name = "Payment_Types")]
    public class MyPaymentType
    {
        [JsonProperty(PropertyName = "Payment_Type_ID")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "Payment_Type")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "Payment_Type_Code")]
        public string Code { get; set; }
        [JsonProperty(PropertyName = "__ExternalPaymentTypeID", NullValueHandling = NullValueHandling.Ignore)]
        public int? LegacyId { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, Name: {1}, Description: {2}, Code: {3}, LegacyId: {4}", Id, Name, Description, Code, LegacyId);
        }
    }
}

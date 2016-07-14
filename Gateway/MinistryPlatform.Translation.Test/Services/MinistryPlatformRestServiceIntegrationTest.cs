﻿using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models.Attributes;
using MinistryPlatform.Translation.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            var auth = AuthenticationRepository.authenticate(Environment.GetEnvironmentVariable("API_USER"), Environment.GetEnvironmentVariable("API_PASSWORD"));
            _authToken = auth["token"].ToString();
        }

        [SetUp]
        public void SetUp()
        {
            var restClient = new RestClient(Environment.GetEnvironmentVariable("MP_REST_API_ENDPOINT"));
            _fixture = new MinistryPlatformRestRepository(restClient);
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
            var results = _fixture.UsingAuthenticationToken(_authToken).GetFromStoredProc<ChildcareDashboard>("api_crds_getChildcareDashboard", parms);
            foreach (var p in results)
            {
                Console.WriteLine("Result\t{0}", p);
            }
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
        public void TestGetPaymentType()
        {
            Console.WriteLine("TestGetPaymentType");
            var p = _fixture.UsingAuthenticationToken(_authToken).Get<MyPaymentType>(2);
            Console.WriteLine("Payment_Type\t{0}", p);
        }
    }

    [MpRestApiTable(Name = "Childcare_Dashboard")]
    public class ChildcareDashboard
    {
        [JsonProperty(PropertyName = "Display_Name")]
        public string DisplayName;

        [JsonProperty(PropertyName = "Group_ID")]
        public int GroupId;

        [JsonProperty(PropertyName = "Group_Name")]
        public string GroupName;

        [JsonProperty(PropertyName = "Group_Type_ID")]
        public int GroupTypeId;

        [JsonProperty(PropertyName = "Event_ID")]
        public int EventID;

        [JsonProperty(PropertyName = "Event_Title")]
        public string EventTitle;

        [JsonProperty(PropertyName = "Cancelled")]
        public bool Cancelled;

        [JsonProperty(PropertyName = "Event_Start_Date")]
        public DateTime EventStartDate;

        [JsonProperty(PropertyName = "Event_End_Date")]
        public DateTime EventEndDate;

        [JsonProperty(PropertyName = "Congregation_ID")]
        public int CongregationID;

        //"Group_ID": 10000001,
        //"Group_Name": "(t) Fathers Oakley CG",
        //"Group_Type_ID": 8,
        //"Event_ID": 4515705,
        //"Event_Title": "Childcare - Test",
        //"Cancelled": false,
        //"Event_Start_Date": "2016-07-10T18:00:00",
        //"Event_End_Date": "2016-07-10T19:00:00",
        //"Congregation_ID": 1
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

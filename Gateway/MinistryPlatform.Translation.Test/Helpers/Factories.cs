using System;
using System.Linq;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Rules;
using MinistryPlatform.Translation.Repositories.Rules;

namespace MinistryPlatform.Translation.Test.Helpers
{
    public class Factories
    {

        public static void GenderRule()
        {
            if (!FactoryGirl.NET.FactoryGirl.DefinedFactories.Contains(typeof(GenderRule)))
            {
                FactoryGirl.NET.FactoryGirl.Define<GenderRule>(() => new GenderRule(DateTime.Now.AddDays(-30), null, 1));
            }
        }

        public static void RegistrationRule()
        {
            if (!FactoryGirl.NET.FactoryGirl.DefinedFactories.Contains(typeof(RegistrationRule)))
            {
                FactoryGirl.NET.FactoryGirl.Define<RegistrationRule>(() => new RegistrationRule(DateTime.Now.AddDays(-30), null, 200));
            }
        }

        public static void MPProductRuleset()
        {
            if (!FactoryGirl.NET.FactoryGirl.DefinedFactories.Contains(typeof(MPProductRuleSet)))
            {
                FactoryGirl.NET.FactoryGirl.Define<MPProductRuleSet>(() => new MPProductRuleSet
                {
                    ProductId = 23,                  
                    RulesetId = 24,
                    EndDate = DateTime.Now.AddDays(5),                    
                    StartDate = DateTime.Now.AddDays(-30)
                });
            }
        }

        public static void EventParticipant()
        {
            if (!FactoryGirl.NET.FactoryGirl.DefinedFactories.Contains(typeof(MpEventParticipant)))
            {
                FactoryGirl.NET.FactoryGirl.Define<MpEventParticipant>(() => new MpEventParticipant
                {
                    EventTitle = "Ok Fake Title",
                    ChildcareRequired = false,
                    ContactId = 8767,
                    EndDate = null,
                    EventId = 98,
                    EventParticipantId = 987,
                    EventStartDateTime = new DateTime(2017, 10, 11),
                    GroupId = 12,
                    GroupName = "Fake Group Name",
                    GroupParticipantId = 3456,
                    ParticipantEmail = "SomeEmail23@gmail.com",
                    ParticipantStatus = 2,
                    ParticipantId = 435
                });
            }
        }

        public static void MpCongregation()
        {
            if (!FactoryGirl.NET.FactoryGirl.DefinedFactories.Contains(typeof(MpCongregation)))
            {
                FactoryGirl.NET.FactoryGirl.Define<MpCongregation>(() => new MpCongregation
                {
                    CongregationId = 12,
                    LocationId = 4,
                    Name = "Oakley"
                });
            }
        }
    }
}
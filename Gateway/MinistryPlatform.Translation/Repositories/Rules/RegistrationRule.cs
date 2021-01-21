using System;
using System.Collections.Generic;
using System.IO;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models.Rules;
using MinistryPlatform.Translation.Repositories.Interfaces.Rules;

namespace MinistryPlatform.Translation.Repositories.Rules
{
    public class RegistrationRule : IRule
    {
        private readonly MPRegistrationRule _registrationRule;

        public RegistrationRule(DateTime startDate, DateTime? endDate, int maxRegistrants)
        {
            _registrationRule = new MPRegistrationRule
            {
                StartDate = startDate,
                EndDate = endDate,
                MaximumRegistrants = maxRegistrants
            };
        }

        public bool RuleIsActive()
        {
            var now = DateTime.Now;
            return now >= _registrationRule.StartDate && (_registrationRule.EndDate == null || now < _registrationRule.EndDate);
        }

        public MPRuleResult RulePasses(Dictionary<string, object> data)
        {
            var result = new MPRuleResult();

            if (!RuleIsActive())
            {
                result.RulePassed = true;
                result.Message = "Rule Not Active";
                return result;
            }

            try
            {
                var registrants = data.ToInt("registrantCount", true);
                result.RulePassed = (registrants <= _registrationRule.MaximumRegistrants);
                result.Message = result.RulePassed ? "Rule Passed" : $"Exceeded Maximum of {_registrationRule.MaximumRegistrants}";
                return result;
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Invalid data provided to registration rule.", e);
            }
        }
    }
}

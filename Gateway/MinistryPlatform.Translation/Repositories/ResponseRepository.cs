using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Repositories
{
    public class ResponseRepository : BaseRepository, IResponseRepository
    {
        private readonly IAuthenticationRepository _authenticationService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IMinistryPlatformService _ministryPlatformService;

        private readonly int _signupToServeRemindersId;

        public ResponseRepository(
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IMinistryPlatformService ministryPlatformService,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _authenticationService = authenticationService;
            _configurationWrapper = configurationWrapper;
            _ministryPlatformService = ministryPlatformService;

            _signupToServeRemindersId = _configurationWrapper.GetConfigIntValue("SignupToServeReminders");
        }

        public List<MPServeReminder> GetServeReminders(String token)
        {
            var dict = _ministryPlatformService.GetPageViewRecords(_signupToServeRemindersId, token, "", "", 0);

            return dict.Select(rec =>
            {
                var mp = new MPServeReminder()
                {
                    Event_End_Date = (DateTime)rec["Event_End_Date"],
                    Event_Start_Date = (DateTime)rec["Event_Start_Date"],
                    Event_Title = (String)rec["Event_Title"],
                    Opportunity_Contact_Id = (int)rec["Opportunity_Contact_ID"],
                    Opportunity_Email_Address = (String)rec["Opportunity_Contact_Email_Address"],
                    Opportunity_Title = (String)rec["Opportunity_Title"],
                    Shift_End = rec.ToNullableTimeSpan("Shift_End"),
                    Shift_Start = rec.ToNullableTimeSpan("Shift_Start"),
                    Signedup_Contact_Id = (int)rec["Contact_ID"],
                    Signedup_Email_Address = (String)rec["Email_Address"]
                };
                if (rec["Communication_ID"] != null)
                {
                    mp.Template_Id = (int)rec["Communication_ID"];
                }
                return mp;
                    
            }).ToList();
        }  
    }
}

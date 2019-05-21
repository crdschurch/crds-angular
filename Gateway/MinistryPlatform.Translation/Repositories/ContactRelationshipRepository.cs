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
    public class ContactRelationshipRepository : BaseRepository, IContactRelationshipRepository
    {
        private readonly int _getMyCurrentRelationships = Convert.ToInt32((AppSettings("MyContactCurrentRelationships")));

        private IMinistryPlatformService _ministryPlatformService;

        public ContactRelationshipRepository(
            IMinistryPlatformService ministryPlatformService,
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            this._ministryPlatformService = ministryPlatformService;
        }

        public IEnumerable<MpContactRelationship> GetMyImmediateFamilyRelationships(int contactId)
        {
            var viewRecords = _ministryPlatformService.GetSubpageViewRecords("MyContactFamilyRelationshipViewId",
                                                                             contactId,
                                                                             ApiLogin());

            return viewRecords.Select(viewRecord => new MpContactRelationship
            {
                Contact_Id = viewRecord.ToInt("Contact_ID"),
                Email_Address = viewRecord.ToString("Email_Address"),
                Last_Name = viewRecord.ToString("Last Name"),
                Preferred_Name = viewRecord.ToString("Preferred Name"),
                Participant_Id = viewRecord.ToInt("Participant_ID"),
                Relationship_Id = viewRecord.ToInt("Relationship_ID"),
                Age = viewRecord.ToInt("Age"),
                HighSchoolGraduationYear = viewRecord.ToInt("HS_Graduation_Year")
            }).ToList();
        }

        public IEnumerable<MpRelationship> GetMyCurrentRelationships(int contactId)
        {
            try
            {
                var viewRecords = _ministryPlatformService.GetSubpageViewRecords("ContactRelationshipsIds",
                                                                                 contactId,
                                                                                 ApiLogin());

                return viewRecords.Select(record => new MpRelationship
                {
                    RelationshipID = record.ToInt("Relationship_ID"),
                    RelatedContactID = record.ToInt("Related_Contact_ID"),
                    EndDate = record.ToNullableDate("End_Date"),
                    StartDate = record.ToDate("Start_Date")
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("GetMyCurrentRelationships Failed", ex);
            }
        }

        public IEnumerable<MpContactRelationship> GetMyCurrentRelationships(int contactId, string param2)
        {
            var viewRecords = _ministryPlatformService.GetSubpageViewRecords(_getMyCurrentRelationships,
                                                                             contactId,
                                                                             ApiLogin());

            return viewRecords.Select(viewRecord => new MpContactRelationship
            {
                Contact_Id = viewRecord.ToInt("Contact_ID"),
                Email_Address = viewRecord.ToString("Email_Address"),
                Last_Name = viewRecord.ToString("Last Name"),
                Preferred_Name = viewRecord.ToString("Preferred Name"),
                Participant_Id = viewRecord.ToInt("Participant_ID"),
                Relationship_Id = viewRecord.ToInt("Relationship_ID")
            }).ToList();
        }

        public int AddRelationship(MpRelationship relationship, int toContact)
        {
            try
            {
                var dict = new Dictionary<string, object>
                {
                    {"Relationship_ID", relationship.RelationshipID},
                    {"Related_Contact_ID", relationship.RelatedContactID},
                    {"Start_Date", relationship.StartDate},
                    {"End_Date", relationship.EndDate}
                };
                return _ministryPlatformService.CreateSubRecord(_configurationWrapper.GetConfigIntValue("ContactRelationships"),
                                                                toContact,
                                                                dict,
                                                                ApiLogin(),
                                                                true);
            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }
}
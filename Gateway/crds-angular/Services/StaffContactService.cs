using System.Collections.Generic;
using System.Linq;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class StaffContactService : IStaffContactService
    {
        private readonly IContactRepository _contactService;

        public StaffContactService(IContactRepository contactService)
        {
            _contactService = contactService;
        }

        public List<PrimaryContactDto> GetStaffContacts()
        {
            return GetContacts(true);
        }

        public List<PrimaryContactDto> GetPrimaryContacts()
        {
            return GetContacts(false);
        }

        private List<PrimaryContactDto> GetContacts(bool staffOnly)
        {
            var mpContacts = _contactService.PrimaryContacts(staffOnly);
            return mpContacts.Select(mpContact => new PrimaryContactDto
            {
                ContactId = mpContact.ToInt("Contact_ID"),
                DisplayName = mpContact.ToString("Display_Name"),
                Email = mpContact.ToString("Email_Address"),
                FirstName = mpContact.ToString("First_Name"),
                LastName = mpContact.ToString("Last_Name")
            }).ToList();
        }
    }
}
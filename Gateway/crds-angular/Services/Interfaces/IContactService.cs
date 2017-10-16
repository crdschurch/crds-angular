using System.Collections.Generic;
using crds_angular.Models.Crossroads;

namespace crds_angular.Services.Interfaces
{
    public interface IStaffContactService
    {
        List<PrimaryContactDto> GetStaffContacts();
        List<PrimaryContactDto> GetPrimaryContacts();
    }
}
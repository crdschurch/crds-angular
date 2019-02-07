using System.Collections.Generic;
using crds_angular.Models.Crossroads.Camp;
using Crossroads.Web.Auth.Models;

namespace crds_angular.Services.Interfaces
{
    public interface ICampService
    {
        CampDTO GetCampEventDetails(int eventId);
        CampReservationDTO SaveCampReservation(CampReservationDTO campReservation, int eventId, AuthDTO token);
        void SaveCamperEmergencyContactInfo(List<CampEmergencyContactDTO> emergencyContacts, int eventId, int contactId, AuthDTO token);
        List<MyCampDTO> GetMyCampInfo(AuthDTO token);
        List<CampWaiverDTO> GetCampWaivers(int eventId, int contactId);
        void SaveWaivers(AuthDTO token, int eventId, int contactId, List<CampWaiverResponseDTO> waivers);
        CampReservationDTO GetCamperInfo(AuthDTO token, int eventId, int contactId);   
        List<CampFamilyMember> GetEligibleFamilyMembers(int eventId, AuthDTO token);
        void SaveCamperMedicalInfo(MedicalInfoDTO medicalInfo, int contactId, AuthDTO token);
        MedicalInfoDTO GetCampMedicalInfo(int eventId, int contactId, AuthDTO token);
        List<CampEmergencyContactDTO> GetCamperEmergencyContactInfo(int eventId, int contactId, AuthDTO token);
        ProductDTO GetCampProductDetails(int eventId, int contactId, AuthDTO token);
        void SaveInvoice(CampProductDTO campProductDto, AuthDTO token);
        bool SendCampConfirmationEmail(int eventId, int invoiceId, int paymentId, AuthDTO token);
        void SetCamperAsRegistered(int eventId, int contactId);
    }
}

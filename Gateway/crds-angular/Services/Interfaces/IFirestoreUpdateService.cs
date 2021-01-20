using System.Threading.Tasks;

namespace crds_angular.Services.Interfaces
{
    public interface IFirestoreUpdateService
    {
        string SendProfilePhotoToFirestore(int participantId);
        void DeleteProfilePhotoFromFirestore(int participantId);
        Task ProcessMapAuditRecords();

        Task<bool> PersonPinToFirestoreAsync(int participantid, bool showOnMap, string pinType);
    }
}

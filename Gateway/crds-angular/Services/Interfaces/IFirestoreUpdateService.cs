using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crds_angular.Services.Interfaces
{
    public interface IFirestoreUpdateService
    {
        string SendProfilePhotoToFirestore(int participantId);
        void DeleteProfilePhotoFromFirestore(int participantId);
        Task ProcessMapAuditRecords();
    }
}

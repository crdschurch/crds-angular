using System.IO;

namespace crds_angular.Services.Interfaces
{
    public interface IImageService
    {
        Stream GetContactImage(int contactId);
        Stream GetParticipantImage(int participantId);
    }
}

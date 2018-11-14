using System;
using System.IO;
using System.Linq;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class ImageService : MinistryPlatformBaseService, IImageService
    {
        private readonly IApiUserRepository _apiUserRepository;
        private readonly MPInterfaces.IMinistryPlatformService _mpService;
        private readonly IContactRepository _contactRepository;

        public ImageService(IApiUserRepository apiUserRepository, MPInterfaces.IMinistryPlatformService mpService, IContactRepository contactRepository)
        {
            _apiUserRepository = apiUserRepository;
            _mpService = mpService;
            _contactRepository = contactRepository;
        }

        private Stream GetImage(int fileId, string fileName)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();

            var imageStream = _mpService.GetFile(fileId, apiToken);
            return imageStream == null ? null : imageStream;
        }

        public Stream GetContactImage(int contactId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            Stream result = null;
            try
            {
                var files = _mpService.GetFileDescriptions("Contacts", contactId, apiToken);
                var file = files.FirstOrDefault(f => f.IsDefaultImage);
                if (file != null)
                {
                    result = GetImage(file.FileId, file.FileName);
                }
            }
            catch (Exception)
            {
                // If the file is not present on the file system, GetImage() will throw an exception
                // but we want to treat that as "not found" instead of an exception
            }

            return result;
        }

        public Stream GetParticipantImage(int participantId)
        {
            //get the contact id
            int contactId = _contactRepository.GetContactIdByParticipantId(participantId);
            return (GetContactImage(contactId));
        }
    }
}
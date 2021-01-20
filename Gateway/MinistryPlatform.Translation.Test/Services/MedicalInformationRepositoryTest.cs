using System.Collections.Generic;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class MedicalInformationRepositoryTest
    {
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IApiUserRepository> _apiUserRepository;

        private IMedicalInformationRepository _fixture;

        [SetUp]
        public void SetUp()
        {
            _apiUserRepository = new Mock<IApiUserRepository>();
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>();
            _fixture = new MedicalInformationRepository(_ministryPlatformRestRepository.Object, _apiUserRepository.Object);

            const string token = "token";
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
        }

        [Test]
        public void ShouldUpdateCreateAndDeleteMedications()
        {
            var mockMeds = MockMedications();
            _ministryPlatformRestRepository.Setup(m => m.Post(new List<MpMedication> {mockMeds[1]}));
            _ministryPlatformRestRepository.Setup(m => m.Put(new List<MpMedication> {mockMeds[0]}));
            _ministryPlatformRestRepository.Setup(m => m.Delete<MpMedication>(new List<int> { mockMeds[2].MedicalInformationMedicationId }));

            _fixture.UpdateOrCreateMedications(mockMeds);

            _apiUserRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();
        }

        private static List<MpMedication> MockMedications()
        {
            return new List<MpMedication>
            {
                new MpMedication
                {
                    MedicalInformationMedicationId = 1,
                    MedicalInformationId = 2,
                    MedicationName = "Placebo",
                    MedicationTypeId = 1,
                    DosageAmount = "All of it",
                    DosageTimes = "Whenever",
                    Deleted = false
                },
                new MpMedication
                {
                    MedicalInformationMedicationId = 0,
                    MedicalInformationId = 2,
                    MedicationName = "New medication",
                    MedicationTypeId = 1,
                    DosageAmount = "Some of it",
                    DosageTimes = "As Needed",
                    Deleted = false
                },
                new MpMedication
                {
                    MedicalInformationMedicationId = 2,
                    MedicalInformationId = 2,
                    MedicationName = "Old Medication",
                    MedicationTypeId = 1,
                    DosageAmount = "None of it",
                    DosageTimes = "Never",
                    Deleted = true
                }
            };
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using crds_angular.Models.Crossroads.Camp;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class CampService : ICampService
    {
        private readonly IContactRepository _contactService;
        private readonly ICampRepository _campService;
        

        public CampService(
            IContactRepository contactService,
            ICampRepository campService)
        {
            _contactService = contactService;
            _campService = campService;
        }

        public CampDTO GetCampEventDetails(int eventId)
        {
            var campEvent = _campService.GetCampEventDetails(eventId);
            var campEventInfo = new CampDTO();
            foreach (var record in campEvent)
            {
                campEventInfo.EventId = record.EventId;
                campEventInfo.EventTitle = record.EventTitle;
                campEventInfo.EventType = record.EventType;
                campEventInfo.StartDate = record.StartDate;
                campEventInfo.EndDate = record.EndDate;
                campEventInfo.OnlineProductId = record.OnlineProductId;
                campEventInfo.RegistrationEndDate = record.RegistrationEndDate;
                campEventInfo.RegistrationStartDate = record.RegistrationStartDate;
                campEventInfo.ProgramId = record.ProgramId;
            }
            return campEventInfo;
        }

        public void SaveCampReservation(CampReservationDTO campReservation, int eventId, string token)
        {
            var parentContact = _contactService.GetMyProfile(token);
            var minorContact = new MpMinorContact
            {
                FirstName = campReservation.FirstName,
                LastName = campReservation.LastName,
                MiddleName = campReservation.MiddleName,
                BirthDate = campReservation.BirthDate,
                Gender = campReservation.Gender,
                PreferredName = campReservation.PreferredName,
                SchoolAttending = campReservation.SchoolAttending,
                HouseholdId = parentContact.Household_ID,
                HouseholdPositionId = 2
            };

            var newMinorContact = _campService.CreateMinorContact(minorContact);
            int contactId = newMinorContact[0].RecordId;
            _campService.AddAsCampParticipant(contactId, eventId);
        }
    }
}

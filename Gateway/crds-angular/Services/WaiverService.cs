using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using crds_angular.Models.Crossroads.Waivers;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class WaiverService : IWaiverService
    {
        private readonly IWaiverRepository _waiverRepository;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IContactRepository _contactRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IEventParticipantRepository _eventParticipantRepository;
        private readonly IConfigurationWrapper _confiurationWrapper;


        public WaiverService(IWaiverRepository waiverRepository, 
            IAuthenticationRepository authenticationRepository, 
            IInvitationRepository invitationRepository, 
            IContactRepository contactRepository, 
            ICommunicationRepository communicationRepository, 
            IEventParticipantRepository eventParticipantRepository,
            IConfigurationWrapper configurationWrapper)
        {
            _waiverRepository = waiverRepository;
            _authenticationRepository = authenticationRepository;
            _invitationRepository = invitationRepository;
            _contactRepository = contactRepository;
            _communicationRepository = communicationRepository;
            _eventParticipantRepository = eventParticipantRepository;
            _confiurationWrapper = configurationWrapper;
        }

        public IObservable<WaiverDTO> GetWaiver(int waiverId)
        {            
            return _waiverRepository.GetWaiver(waiverId).Select(w => new WaiverDTO
            {
                WaiverId = w.WaiverId,
                WaiverName = w.WaiverName,
                WaiverText = w.WaiverText,
                WaiverStartDate = w.WaiverStartDate,
                WaiverEndDate = w.WaiverEndDate
            });
        }

        public IObservable<WaiverDTO> EventWaivers(int eventId, string token)
        {
            var contactId = _authenticationRepository.GetContactId(token);

            var waivers = _waiverRepository.GetEventWaivers(eventId).Select(w => new WaiverDTO
            {
                WaiverId = w.WaiverId,
                Required = w.Required,
                WaiverName = w.WaiverName,
                WaiverText = w.WaiverText,
            });
            var evtParWaivers = _waiverRepository.GetEventParticipantWaiversByContact(eventId, contactId);         

            var activeWaiversDto = waivers.SelectMany(w =>
            {
                return evtParWaivers.Where(ep => ep.WaiverId == w.WaiverId).Select(x => new WaiverDTO
                {
                    WaiverId = w.WaiverId,
                    Required = w.Required,
                    WaiverName = w.WaiverName,
                    WaiverText = w.WaiverText,
                    Accepted = x.Accepted
                });
            });

            var inactiveWaiversDto = waivers.Where(w =>!evtParWaivers.Any(ep => ep.WaiverId == w.WaiverId).Wait());
            return activeWaiversDto.Merge(inactiveWaiversDto);
        }

        public IObservable<int> SendAcceptanceEmail(ContactInvitation contactInvitation)
        {
            var templateId = _confiurationWrapper.GetConfigIntValue("WaiverEmailTemplateId");
            var template = _communicationRepository.GetTemplate(templateId);               

            // get the event name for the waiver...
            return _eventParticipantRepository.GetEventParticpantByEventParticipantWaiver(contactInvitation.Invitation.SourceId).Select(ep =>
            {
                var mergeData = new Dictionary<string, object>
                {
                    {"Event_Name", ep.EventTitle },
                    {"Confirmation_Url", $"https://{_confiurationWrapper.GetConfigValue("BaseUrl")}/waivers/accept/{contactInvitation.Invitation.InvitationGuid}" }
                };

                var comm = _communicationRepository.GetTemplateAsCommunication(templateId,
                                                                                template.FromContactId,
                                                                                template.FromEmailAddress,
                                                                                template.ReplyToContactId,
                                                                                template.ReplyToEmailAddress,
                                                                                contactInvitation.Contact.ContactId,
                                                                                contactInvitation.Contact.EmailAddress,
                                                                                mergeData);
                return _communicationRepository.SendMessage(comm);
            });
        }

        public IObservable<ContactInvitation> CreateWaiverInvitation(int waiverId, int eventParticipantId, string token)
        {           
            var contactId = _authenticationRepository.GetContactId(token);
            return _contactRepository.GetSimpleContact(contactId).SelectMany(con =>
            {
               return _waiverRepository.CreateEventParticipantWaiver(waiverId, eventParticipantId, contactId).SelectMany(eventParticipantWaiver =>
               {
                   // create private invite
                   var mpInvitation = new MpInvitation
                   {
                       SourceId = eventParticipantWaiver.EventParticipantWaiverId,
                       EmailAddress = con.EmailAddress,
                       InvitationType = _confiurationWrapper.GetConfigIntValue("WaiverInvitationType"),
                       RecipientName = $"{con.Nickname ?? con.FirstName} {con.LastName}",
                       RequestDate = DateTime.Now
                   };

                   return _invitationRepository.CreateInvitationAsync(mpInvitation).Select(invite => new ContactInvitation
                   {
                       Contact = con,
                       Invitation = invite
                   });
               });
           });
        }

        public IObservable<WaiverDTO> AcceptWaiver(string guid)
        {
            return _invitationRepository.GetOpenInvitationAsync(guid).SelectMany(invitation =>
            {
                return _waiverRepository.AcceptEventParticpantWaiver(invitation.SourceId).SelectMany(w =>
                {
                    _invitationRepository.MarkInvitationAsUsed(guid);
                    return _waiverRepository.GetWaiver(w.WaiverId).Select(w2 => new WaiverDTO
                        {
                            Accepted = w.Accepted,
                            SigneeContactId = w.SignerId,
                            WaiverId = w.WaiverId,
                            WaiverName = w2.WaiverName,
                            WaiverText = w2.WaiverText,
                            WaiverStartDate = w2.WaiverStartDate
                        });
                });
            });
        }
    }

}
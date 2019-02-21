using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IParticipantRepository
    {
        MpParticipant GetParticipant(int contactId);
        MpParticipant GetParticipantRecord(string token);
        List<MpResponse> GetParticipantResponses(int participantId);
        void UpdateParticipant(MpParticipant participant);
        void UpdateParticipantHostStatus(MpParticipant participant);
        void UpdateParticipant(Dictionary<string, object> getDictionary);
        int CreateParticipantRecord(int contactId);
        void UpdateHuddleGroupParticipantStatus();
    }
}

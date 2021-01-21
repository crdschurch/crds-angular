using System;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Exceptions
{
    public class GroupFullException : Exception
    {
        private MpGroup group;

        public GroupFullException(MpGroup group)
            : base("Group is full: " + group.Participants.Count + " > " + group.TargetSize)
        {
            this.group = group;
        }
    }
}

using CommandLine;
using CommandLine.Text;

namespace Crossroads.ScheduledDataUpdate
{
    public class Options
    {
        [Option('H', "help", Required = false, DefaultValue = false, MutuallyExclusiveSet = "OpMode",
          HelpText = "Get help on running this program.")]
        public bool HelpMode { get; set; }

        [Option("AutoCompleteTasks", Required = false, DefaultValue = false, MutuallyExclusiveSet = "OpMode",
          HelpText = "Execute 'Auto Complete Tasks' to complete outstanding event/room tasks for users")]
        public bool AutoCompleteTasksMode { get; set; }

        [Option("RoomReservationRejectionNotification", Required = false, DefaultValue = false, MutuallyExclusiveSet = "OpMode",
            HelpText = "Execute 'Room Reservation Rejection Notification' to send emails to to people whose room reservation request has been rejected")]
        public bool RoomReservationRejectionNotification { get; set; }

        [Option("SmallGroupInquiryReminder", Required = false, DefaultValue = false, MutuallyExclusiveSet = "OpMode",
          HelpText = "Execute 'Small Group Inquiry Reminder' to send emails to group leaders who have pending inquiries on their groups")]
        public bool SmallGroupInquiryReminderMode { get; set; }

        [Option("ConnectAwsRefreshMode", Required = false, DefaultValue = false, MutuallyExclusiveSet = "OpMode",
          HelpText = "Execute 'Connect AWS Refresh' to refresh connect groups that have been approved by staff")]
        public bool ConnectAwsRefreshMode { get; set; }

        [Option("CorkboardAwsRefreshMode", Required = false, DefaultValue = false, MutuallyExclusiveSet = "OpMode",
          HelpText = "Execute 'Corkboard AWS Refresh' to refresh corkboard items in mongo with AWS")]
        public bool CorkboardAwsRefreshMode { get; set; }

        [Option("ArchivePendingGroupInquiriesMode", Required = false, DefaultValue = false, MutuallyExclusiveSet = "OpMode",
            HelpText = "Execute 'api_crds_Archive_Pending_Group_Inquiries_Older_Than_90_Days'")]
        public bool ArchivePendingGroupInquiriesMode { get; set; }

        [Option("HuddleStatusParticipantUpdateMode", Required = false, DefaultValue = false, MutuallyExclusiveSet = "OpMode",
            HelpText = "Execute 'Huddle Status Participant Updates'")]
        public bool HuddleStatusParticipantUpdateMode { get; set; }

        [Option("ConnectMapUpdateMode", Required = false, DefaultValue = false, MutuallyExclusiveSet = "OpMode",
            HelpText = "Execute 'Sync Connect Pins to Firestore'")]
        public bool ConnectMapUpdateMode { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}

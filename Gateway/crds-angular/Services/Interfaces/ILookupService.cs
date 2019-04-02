
namespace crds_angular.Services.Interfaces {
  public  interface ILookupService
  {
      string GetMeetingDayFromId(int? meetingDayId);

        string GetMeetingFrequencyFromId(int? meetingFrequencyId);
  }
}

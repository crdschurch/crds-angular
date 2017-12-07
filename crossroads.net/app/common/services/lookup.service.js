(function () {
  module.exports = LookupService;

  LookupService.$inject = ['$resource'];

  function LookupService($resource) {
    return {
      Congregations: $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/crossroadslocations`),
      ChildcareLocations: $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/childcarelocations`),
      Ministries: $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/ministries`),
      GroupsByCongregationAndMinistry:
              $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/group/:congregationId/:ministryId`),
      ChildcareTimes: $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/childcaretimes/:congregationId`),
      Sites: $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/sites`),
      Genders: $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/genders`),
      // EventTypes: $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/eventtypes`),
      EventTypesForEventTool: $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/eventtypes?filter=event-tool`),
      DaysOfTheWeek: $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/meetingdays`),
      MeetingFrequencies: $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/lookup/meetingdays`)
    };
  }
}());

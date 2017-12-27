import CONSTANTS from 'crds-constants';
import ReminderService from '../../../app/live_stream/services/reminder.service';
import Reminder from '../../../app/live_stream/models/reminder';
import StreamStatusService from '../../../app/live_stream/services/stream-status.service';

describe('Stream Status Service', () => {
  let expectedHrsToEvent,
      hrsToNextEvent,
      streamStatusService,
      eventStartingAfterCurrentTime,
      doesEventStartAfterCurrentTime,
      isBroadcasting,
      upcomingStatus,
      streamStatus,
      didStreamStatusChange,
      isEventLive,
      isAnyEventLive,
      roundedHrsToNextEvnt;

  let baseTime = new Date("October 1, 2016 12:00:00"); // set to 10/1/2016 - month appears to be 0 based index however

  let events = [
    {
      "start": moment(baseTime).subtract(4, 'hours'),
      "end": moment(baseTime).add(3, 'hours'),
      "title": "Saturday Rehearsal Broadcasting"
    },
    {
      "start": moment(baseTime).subtract(5, 'hours'),
      "end": moment(baseTime).subtract(6, 'hours'),
      "title": "Saturday Rehearsal Done"
    },
    {
      "start": moment(baseTime).add(1, 'hour'),
      "end": moment(baseTime).add(2, 'hours'),
      "title": "Saturday Rehearsal Upcoming"
    },
    {
      "start": moment(baseTime).subtract(10, 'hours'),
      "end": moment(baseTime).subtract(9, 'hours'),
      "title": "Saturday Rehearsal Done"
    }
  ];

  let eventsWithLiveEvent = [
    {
      "start": moment(baseTime).subtract(4, 'hours'),
      "end": moment(baseTime).add(3, 'hours'),
      "title": "Saturday Rehearsal Broadcasting"
    },
    {
      "start": moment(baseTime).subtract(5, 'hours'),
      "end": moment(baseTime).subtract(6, 'hours'),
      "title": "Saturday Rehearsal Done"
    },
    {
      "start": moment(baseTime).add(1, 'hour'),
      "end": moment(baseTime).add(2, 'hours'),
      "title": "Saturday Rehearsal Upcoming"
    }
  ];

  let eventsWithoutLiveEvent = [
    {
      "start": moment(baseTime).add(3, 'hours'),
      "end": moment(baseTime).add(4, 'hours'),
      "title": "Saturday Rehearsal Broadcasting"
    },
    {
      "start": moment(baseTime).subtract(5, 'hours'),
      "end": moment(baseTime).subtract(6, 'hours'),
      "title": "Saturday Rehearsal Done"
    },
    {
      "start": moment(baseTime).add(1, 'hour'),
      "end": moment(baseTime).add(2, 'hours'),
      "title": "Saturday Rehearsal Upcoming"
    }
  ];

  let liveEvent = {
    "start": moment(baseTime).subtract(1, 'hour'),
    "end": moment(baseTime).add(1, 'hour'),
    "title": "Live Event"
  };

  let upcomingEvent = {
    "start": moment(baseTime).add(3, 'hours'),
    "end": moment(baseTime).add(4, 'hours'),
    "title": "Upcoming Event"
  };


  beforeEach(angular.mock.module(CONSTANTS.MODULES.LIVE_STREAM));

  beforeEach(inject(function ($injector) {
    streamStatusService = $injector.get('StreamStatusService');
  }));

  beforeEach(() => jasmine.clock().mockDate(baseTime));

  describe('Stream Status Service', () => {

    it('should indicate that the event starts after the current time', () => {
      eventStartingAfterCurrentTime = events[2];
      doesEventStartAfterCurrentTime = streamStatusService.doesEventStartAfterCurrentTime(eventStartingAfterCurrentTime);
      expect(doesEventStartAfterCurrentTime).toBe(true);
    });

    it('get hours until next streaming event starts', () => {
      hrsToNextEvent = streamStatusService.getHoursToNextEvent(events);
      roundedHrsToNextEvnt = Math.round(hrsToNextEvent); //avoid js floating point errors
      expectedHrsToEvent = 1;
      expect(roundedHrsToNextEvnt).toEqual(expectedHrsToEvent);
    });

    it('should show stream status as upcoming', () => {
      isBroadcasting = false;
      streamStatus = streamStatusService.determineStreamStatus(events, isBroadcasting);
      upcomingStatus = CONSTANTS.STREAM_STATUS.UPCOMING;
      expect(streamStatus).toEqual(upcomingStatus);
    });

    it('should indicate that stream status changed', () => {
      isBroadcasting = false;
      didStreamStatusChange = streamStatusService.didStreamStatusChange(events, isBroadcasting);
      expect(didStreamStatusChange).toEqual(true);
    });

    it('should indicate that event IS live', () => {
      isEventLive = streamStatusService.isEventCurrentlyLive(liveEvent);
      expect(isEventLive).toEqual(true);
    });

    it('should indicate that event is NOT live', () => {
      isEventLive = streamStatusService.isEventCurrentlyLive(upcomingEvent);
      expect(isEventLive).toEqual(false);
    });

    it('should indicate that one of the events IS live', () => {
      isAnyEventLive = streamStatusService.isBroadcasting(eventsWithLiveEvent);
      expect(isAnyEventLive).toEqual(true);
    });

    it('should indicate that NONE of the events are live', () => {
      isAnyEventLive = streamStatusService.isBroadcasting(eventsWithoutLiveEvent);
      expect(isAnyEventLive).toEqual(false);
    });

})

});

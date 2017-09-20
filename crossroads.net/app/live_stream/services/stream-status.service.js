import CONSTANTS from 'crds-constants';
import moment from 'moment';
import Event from '../models/event';


export default class StreamStatusService {

  constructor($rootScope, $resource) {
    this.rootScope = $rootScope;
    this.resource = $resource;
    this.streamStatus = undefined;
  }

  getStatus() {
    return this.streamStatus;
  }

  presetStreamStatus(eventsPromise) {
    eventsPromise.then((events) => {
      const isBroadcasting = this.isBroadcasting(events);
      this.streamStatus = this.determineStreamStatus(events, isBroadcasting);
    });
  }

  setStreamStatus(events, isBroadcasting) {
    const streamStatus = this.determineStreamStatus(events, isBroadcasting);
    const isChanged = this.didStreamStatusChange(events, isBroadcasting);

    if (isChanged) {
      this.streamStatus = status;
      this.rootScope.$broadcast('streamStatusChanged', streamStatus);
    }
  }

  didStreamStatusChange(events, isBroadcasting) {
    const oldStreamStatus = this.streamStatus;
    const newStreamStatus = this.determineStreamStatus(events, isBroadcasting);

    return newStreamStatus !== oldStreamStatus;
  }

  determineStreamStatus(events, isBroadcasting) {
    let streamStatus;
    const hrsToNextEvent = this.getHoursToNextEvent(events);

    const isStreamSoon = (CONSTANTS.PRE_STREAM_HOURS > hrsToNextEvent && hrsToNextEvent !== false);

    if (isBroadcasting) {
      streamStatus = CONSTANTS.STREAM_STATUS.LIVE;
    } else if (isStreamSoon) {
      streamStatus = CONSTANTS.STREAM_STATUS.UPCOMING;
    } else {
      streamStatus = CONSTANTS.STREAM_STATUS.OFF;
    }

    return streamStatus;
  }

  getHoursToNextEvent(events) {
    let hoursUntilNextEvent = false;
    if (events.length > 0) {
      const eventsStartingAfterCurrentTime = this.filterOutEventsStartingBeforeCurrentTime(events);
      const nextEvent = _.sortBy(eventsStartingAfterCurrentTime, ['event', 'start'])[0];

      if (nextEvent !== undefined) {
        const currentTime = moment();
        const timeNextEvenStarts = (typeof nextEvent.start === moment) ? nextEvent.start : moment(nextEvent.start);
        const timeUntilNextEvent = moment.duration(timeNextEvenStarts.diff(currentTime));
        hoursUntilNextEvent = timeUntilNextEvent.asHours();
      }
    }

    return hoursUntilNextEvent;
  }

  filterOutEventsStartingBeforeCurrentTime(events) {
    const eventsStartingAfterCurrentTime = [];

    for (let i = 0; i < events.length; i++) {
      const iteratedEvent = events[i];
      const doesEventStartAfterCurrentTime = this.doesEventStartAfterCurrentTime(iteratedEvent);
      if (doesEventStartAfterCurrentTime) {
        eventsStartingAfterCurrentTime.push(events[i]);
      }
    }

    return eventsStartingAfterCurrentTime;
  }

  doesEventStartAfterCurrentTime(event) {
    const currentTime = moment();
    const eventStartTime = (typeof event.start === moment) ? event.start : moment(event.start);

    return eventStartTime.isAfter(currentTime);
  }

  isBroadcasting(events) {
    let areAnyEventsBroadcasting = false;

    for (let i = 0; i < events.length; i++) {
      const iteratedEvent = events[i];
      const isEventLive = this.isEventCurrentlyLive(iteratedEvent);
      if (isEventLive) {
        areAnyEventsBroadcasting = true;
      }
    }

    return areAnyEventsBroadcasting;
  }

  isEventCurrentlyLive(event) {
    const currentTime = moment();
    const eventStartTime = (typeof event.start === moment) ? event.start : moment(event.start);
    const eventEndTime = (typeof event.end === moment) ? event.end : moment(event.end);

    return eventStartTime.isBefore(currentTime) && eventEndTime.isAfter(currentTime);
  }

}

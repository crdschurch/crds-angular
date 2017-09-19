import CONSTANTS from 'crds-constants';
import moment from 'moment';
import Event from '../models/event';


export default class StreamStatusService {

  constructor($rootScope, $q, $resource) {
    this.rootScope = $rootScope;
    this.q = $q;
    this.resource = $resource;
    this.streamStatus = undefined;
    this.url = __STREAMSPOT_ENDPOINT__;
    this.ssid = __STREAMSPOT_SSID__;
    this.headers = {
      'Content-Type': 'application/json',
      'x-API-Key': __STREAMSPOT_API_KEY__
    };
    this.time = 0;
  }

  getStatus() {
    return this.streamStatus;
  }

  presetStreamStatus() {
    const deferred = this.q.defer();

    const url = `${this.url}broadcaster/${this.ssid}/broadcasts/upcoming`;

    return this.resource(url, {}, { get: { method: 'GET', headers: this.headers } })
        .get()
        .$promise
        .then((response) => {
          const events = response.data.broadcasts;
          const formattedEvents = this.formatEvents(events);
          const isBroadcasting = this.isBroadcasting(formattedEvents);
          this.streamStatus = this.determineStreamStatus(formattedEvents, isBroadcasting);
          deferred.resolve(formattedEvents);
        });
  }

  formatEvents(events) {
    return _
        .chain(events)
        .sortBy('start')
        .map((object) => {
          const event = Event.build(object);
          if (event.isBroadcasting() || event.isUpcoming()) {
            return event;
          }
        })
        .compact()
        .value();
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

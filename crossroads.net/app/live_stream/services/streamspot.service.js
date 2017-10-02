
import Event from '../models/event';

export default class StreamspotService {

  constructor($resource, $rootScope, StreamStatusService, $state, $location) {
    this.resource = $resource;
    this.rootScope = $rootScope;
    this.streamStatusService = StreamStatusService;
    this.headers = {
      'Content-Type': 'application/json',
      'x-API-Key': __STREAMSPOT_API_KEY__
    };
    this.url = __STREAMSPOT_ENDPOINT__;
    this.ssid = __STREAMSPOT_SSID__;
    this.events = this.getEvents();
    this.state = $state;
    this.location = $location;
    this.isBroadcasting = false;
  }

  broadcast() {
    const events = this.parseEvents();
    const event = _(events).first();
    this.streamStatusService.setStreamStatus(events, event.isBroadcasting());
    this.rootScope.$broadcast('isBroadcasting', event.isBroadcasting());
    this.rootScope.$broadcast('nextEvent', event);
  }

  getEvents() {
    const url = `${this.url}broadcaster/${this.ssid}/broadcasts/upcomingPlusCurrent`;
    return this.resource(url, {}, { get: { method: 'GET', headers: this.headers } })
      .get()
      .$promise
      .then((response) => {
        this.eventResponse = response.data.broadcasts || [];

        if (response.data.current)      // current event in progress?
          this.eventResponse.unshift(response.data.current);

        const events = this.parseEvents();
        if (events.length > 0) {
          this.broadcast();
          setInterval(() => {
            this.broadcast();
            this.rootScope.$apply();
          }, 1000);
        }
        return events;
      });
  }

  parseEvents() {
    return _
      .chain(this.eventResponse)
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

  get(url) {
    return this.resource(url, {}, { get: { method: 'GET', headers: this.headers } })
      .get().$promise;
  }

  getBroadcaster() {
    return this.get(`${this.url}broadcaster/${this.ssid}?players=true`);
  }

  getPlayers() {
    return this.get(`${this.url}broadcaster/${this.ssid}/players`);
  }

  getBroadcasting() {
    return this.get(`${this.url}broadcaster/${this.ssid}/broadcasting`);
  }

  checkBroadcasting() {
    let debug = false;
    if (this.location !== undefined) {
      const params = this.location.search();
      debug = params.debug;
    }
    if (debug === 'true') {
      this.isBroadcasting = true;
    } else {
      this.getBroadcasting().then((response) => {
        this.isBroadcasting = false;
        if (response !== undefined && response.data !== undefined) {
          this.isBroadcasting = response.data.isBroadcasting;
        }
        if (this.isBroadcasting === false) {
          this.state.go('live');
        }
      });
    }
  }

}

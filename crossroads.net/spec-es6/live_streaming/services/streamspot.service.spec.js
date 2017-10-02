import CONSTANTS from 'crds-constants';
import StreamspotService from '../../../app/live_stream/services/streamspot.service';
import Event from '../../../app/live_stream/models/event';

describe('Live Streaming Streamspot Service', () => {
  let resource,
      baseTime,
      httpBackend,
      service,
      authRequestHandler;

  baseTime = new Date(2016, 9, 1); // set to 10/1/2016 - month appears to be 0 based index however

  let events = {
    "success": true,
    "data": {
      "broadcasts": [       // all future events
        {
          "start": moment(baseTime).add(1, 'hour').format('YYYY-MM-DD H:mm:ss'),
          "end": moment(baseTime).add(2, 'hours').format('YYYY-MM-DD H:mm:ss'),
          "title": "Saturday Rehearsal Upcoming"
        },
        {
          "start": moment(baseTime).add(3, 'hour').format('YYYY-MM-DD H:mm:ss'),
          "end": moment(baseTime).add(4, 'hours').format('YYYY-MM-DD H:mm:ss'),
          "title": "Saturday Rehearsal Upcoming 2"
        },
      ],
      "current": {          // current event in progress (null if nothing in progress)
        "start": moment(baseTime).subtract(1, 'hour').format('YYYY-MM-DD H:mm:ss'),
        "end": moment(baseTime).add(1, 'hour').format('YYYY-MM-DD H:mm:ss'),
        "title": "Saturday Rehearsal Broadcasting"
      },
      "next": {             // next event not yet started
        "start": moment(baseTime).add(1, 'hour').format('YYYY-MM-DD H:mm:ss'),
        "end": moment(baseTime).add(2, 'hours').format('YYYY-MM-DD H:mm:ss'),
        "title": "Saturday Rehearsal Upcoming"
      }
    }
  };

  const eventsEndpoint = `${__STREAMSPOT_ENDPOINT__}broadcaster/${__STREAMSPOT_SSID__}/broadcasts/upcomingPlusCurrent`

  beforeEach(angular.mock.module(CONSTANTS.MODULES.LIVE_STREAM));

  beforeEach(inject(function($injector) {
    resource    = $injector.get('$resource');
    httpBackend = $injector.get('$httpBackend');

    service = new StreamspotService(resource);
    
    jasmine.clock().mockDate(baseTime);

  }));

  afterEach(() => {
    httpBackend.verifyNoOutstandingExpectation();
    httpBackend.verifyNoOutstandingRequest();
    jasmine.clock().mockDate();
  });

  describe('Events', () => {
    it('should get events', () => {
      httpBackend.expectGET(eventsEndpoint).respond(200, events);
      httpBackend.flush();
    });

    it('should return upcoming events', () => {
      httpBackend.expectGET(eventsEndpoint).respond(200, events);

      httpBackend.flush();

      expect(service.parseEvents() instanceof Array).toBeTruthy();

      let titles = service.parseEvents().map((event) => { return event.title });
      expect(titles).not.toContain('Saturday Rehearsal Done');

      expect(_.first(service.parseEvents()).title).toBe('Saturday Rehearsal Broadcasting');
      
    });
  })
});
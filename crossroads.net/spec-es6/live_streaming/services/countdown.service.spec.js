
import constants from 'crds-constants';
import CountdownService from '../../../app/live_stream/services/countdown.service';
import StreamspotService from '../../../app/live_stream/services/streamspot.service';
import Event from '../../../app/live_stream/models/event';

describe('Countdown Service', () => {
  let fixture,
      streamspotService,
      rootScope;

  let baseTime = new Date("October 1, 2016 12:00:00"); // set to 10/1/2016 - month appears to be 0 based index however

  beforeEach(() => jasmine.clock().mockDate(baseTime));

  beforeEach(inject(function ($injector) {
    rootScope = $injector.get('$rootScope');

    fixture = new CountdownService(rootScope, StreamspotService);
  }));

  it('should return padded string for numbers less than 10', () => {
    expect(fixture.pad(1)).toBe('01');
    expect(fixture.pad(10)).toBe('10');
  })

  it('should populate the countdown object', () => {
    let start = moment().add(2, 'days').subtract(1, 'second').format('YYYY-MM-DD HH:mm:ss');
    let end = moment().add(2, 'days').subtract(1, 'second').format('YYYY-MM-DD HH:mm:ss');

    fixture.event = new Event('title', start, end);
    fixture.parseEvent();

    let tz = 'America/New_York';
    let format = 'YYYY-MM-DD H:mm:ss';

    let isNowDst = moment.tz(moment(), format, tz).isDST();
    let isStartDst = moment.tz(start, format, tz).isDST();

    let daysUntil = '01';
    let hoursUntil = '23';
    let minutesUntil = '59';

    // we are coming out of DST and gaining an hour
    if ( fixture.countdown.overlapDst === -1 ) {
      daysUntil = '02';
      hoursUntil = '00';
    }
    // we are going into DST and losing an hour
    else if ( fixture.countdown.overlapDst === 1 ) {
      daysUntil = '01';
      hoursUntil = '22';
    }

    expect(fixture.countdown.days).toBe(daysUntil);
    expect(fixture.countdown.hours).toBe(hoursUntil);
    expect(fixture.countdown.minutes).toBe('59');

  })

})

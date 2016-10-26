
import constants from 'crds-constants';
import CountdownService from '../../../app/live_stream/services/countdown.service';
import StreamspotService from '../../../app/live_stream/services/streamspot.service';
import Event from '../../../app/live_stream/models/event';

describe('Countdown Service', () => {
  let fixture,
      streamspotService,
      rootScope;

  beforeEach(angular.mock.module(constants.MODULES.GROUP_TOOL));

  beforeEach(inject(function ($injector) {
    rootScope = $injector.get('$rootScope');

    fixture = new CountdownService(rootScope, StreamspotService);
  }));

  it('should return padded string for numbers less than 10', () => {
    expect(fixture.pad(1)).toBe('01');
    expect(fixture.pad(10)).toBe('10');
  })

  it('should populate the countdown object', () => {
    let start = moment().add(2, 'days').format('YYYY-MM-DD HH:mm:ss');
    let end = moment().add(2, 'days').format('YYYY-MM-DD HH:mm:ss');

    fixture.event = new Event('title', start, end);
    fixture.parseEvent();

    expect(fixture.countdown.days).toBe('01');
    expect(fixture.countdown.hours).toBe('23');
    expect(fixture.countdown.minutes).toBe('59');
  })

})
require('../../app/common/common.module');
require('../../app/app');

describe('Common Analytics Service', () => {
  let fixture;
  let analytics;

  beforeEach(() => {
    angular.mock.module('crossroads.common', ($provide) => {
      analytics = jasmine.createSpyObj('$analytics',
        ['eventTrack', 'setUserProperties', 'setAlias']);
      $provide.value('$analytics', analytics);
    });
  });

  beforeEach(
      inject((AnalyticsService) => {
        fixture = AnalyticsService;
      })
  );

  it('should call eventTrack with "Forgot Password"', () => {
    fixture.trackForgotPassword();
    expect(analytics.eventTrack).toHaveBeenCalledWith('ForgotPassword', { Source: 'CrossroadsNet' });
  });

  it('trackStream should call eventTrack with "Play"', () => {
    const event = 'Play';
    fixture.trackStream(event);
    expect(analytics.eventTrack).toHaveBeenCalledWith('Play', { category: 'Streaming', label: 'Live Streaming Play' });
  });

  it('trackYouTube should call eventTrack with "Ended"', () => {
    const event = 'Ended';
    const videoId = 123123;
    fixture.trackYouTube(event, videoId);
    expect(analytics.eventTrack).toHaveBeenCalledWith('Ended', { category: 'video', label: videoId });
  });

  it('trackAudio should call eventTrack with "Paused"', () => {
    const event = 'Paused';
    const serviceId = 123123;
    fixture.trackAudio(event, serviceId);
    expect(analytics.eventTrack).toHaveBeenCalledWith('Paused', { category: 'audio', label: serviceId });
  });

  it('should call identify', () => {
    fixture.identifyLoggedInUser(1234, 'email@email.com', 'first', 'last');
    expect(analytics.setUserProperties).toHaveBeenCalledWith({userId: 1234, Email: 'email@email.com', FirstName: 'first', LastName: 'last' });
  });

  it('should call alias', () => {
    fixture.newUserRegistered(1234);
    expect(analytics.setAlias).toHaveBeenCalledWith(1234);
    expect(analytics.setUserProperties).toHaveBeenCalledWith({userId: 1234, Email: undefined, FirstName: undefined, LastName: undefined });
  });

  it('should call alias with properties', () => {
    fixture.newUserRegistered(1234, 'email@email.com', 'first', 'last');
    expect(analytics.setAlias).toHaveBeenCalledWith(1234);
    expect(analytics.setUserProperties).toHaveBeenCalledWith({userId: 1234, Email: 'email@email.com', FirstName: 'first', LastName: 'last' });
  });
});

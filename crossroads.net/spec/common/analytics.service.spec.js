require('../../app/common/common.module');
require('../../app/app');

describe('Common Analytics Service', () => {
  let fixture;
  let analytics;

  beforeEach(() => {
    angular.mock.module('crossroads.common', ($provide) => {
      analytics = jasmine.createSpyObj('$analytics',
        ['eventTrack']);
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
    expect(analytics.eventTrack).toHaveBeenCalledWith('ForgotPassword');
  });

  it('should call identify', () => {
    spyOn(analytics, 'setUserProperties');
    fixture.identify(1234);
    expect(analytics.setUserProperties).toHaveBeenCalledWith(1234);
  });

  it('should call alias', () => {
    spyOn(analytics, 'setAlias');
    fixture.alias(1234);
    expect(analytics.setAlias).toHaveBeenCalledWith(1234);
  });
});

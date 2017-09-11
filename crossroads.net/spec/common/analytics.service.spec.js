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
    expect(analytics.eventTrack).toHaveBeenCalledWith('ForgotPassword');
  });

  it('should call identify', () => {
    fixture.identifyLoggedInUser(1234, 'email@email.com', 'first', 'last');
    expect(analytics.setUserProperties).toHaveBeenCalledWith(1234, 'email@email.com', 'first', 'last');
  });

  it('should call alias', () => {
    fixture.newUserRegistered(1234);
    expect(analytics.setAlias).toHaveBeenCalledWith(1234);
  });
});

export default class AnalyticsService {
  /* @ngInject */
  constructor($analytics) {
    this.analytics = $analytics;
  }

  trackForgotPassword() {
    this.analytics.eventTrack('ForgotPassword');
  }

  newUserRegistered(userId) {
    this.analytics.setAlias(userId);
  }

  identifyLoggedInUser(userId, email, firstName, lastName) {
    const props = {
      userId,
      Email: email,
      FirstName: firstName,
      LastName: lastName
    };

    this.analytics.setUserProperties(props);
  }
}

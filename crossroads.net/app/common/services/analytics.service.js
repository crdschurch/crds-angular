export default class AnalyticsService {
  /* @ngInject */
  constructor($analytics) {
    this.analytics = $analytics;
  }

  trackForgotPassword() {
    this.analytics.eventTrack('ForgotPassword');
  }

  alias(userId) {
    this.analytics.setAlias(userId);
  }

  identify(userId) {
    this.analytics.setUserProperties(userId);
  }
}

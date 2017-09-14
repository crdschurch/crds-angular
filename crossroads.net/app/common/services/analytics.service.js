export default class AnalyticsService {
  /* @ngInject */
  constructor($analytics) {
    this.analytics = $analytics;
  }

  trackForgotPassword() {
    this.analytics.eventTrack('ForgotPassword', { Source: 'CrossroadsNet' });
  }

  trackStream(event) {
    this.analytics.eventTrack(event, { category: 'Streaming', label: `Live Streaming ${event}` });
  }

  trackYouTube(event, videoId) {
    this.analytics.eventTrack(event, { category: 'video', label: videoId });
  }

  trackAudio(event, serviceId) {
    this.analytics.eventTrack(event, { category: 'audio', label: serviceId });
  }

  newUserRegistered(userId, email, firstName, lastName) {
    this.analytics.setAlias(userId);
    this.identifyLoggedInUser(userId, email, firstName, lastName);
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

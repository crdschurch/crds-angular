function MyTripCard($log, TripsUrlService, $cookies, $state) {
  function link(scope) {
    function goalMet(totalRaised, goal) {
      return (totalRaised >= goal);
    }

    function cardName(first, last) {
      return `${first} ${last}`;
    }

    function showWaiver(contactId) {
      const loggedInContact = $cookies.get('userId');
      const age = scope.trip.userAge;
      return age >= 18 && contactId === parseInt(loggedInContact, 10);
    }

    function signWaiver(waiverId, eventParticipantId, eventName) {
      $state.go('sign-waiver', { waiverId, eventParticipantId, title: 'Trip Waiver', eventName });
    }

    function showIPromise(contactId) {
      const loggedInContact = $cookies.get('userId');
      return Number(loggedInContact) === contactId;
    }

    function signIPromise(eventParticipantId) {
      $state.go('trippromise', { eventParticipantId });
    }

    /* eslint-disable no-param-reassign */
    scope.cardName = cardName;
    scope.goalMet = goalMet;
    scope.showIPromise = showIPromise;
    scope.signIPromise = signIPromise;
    scope.shareUrl = TripsUrlService.ShareUrl(scope.trip.eventParticipantId); // eslint-disable-line new-cap
    scope.showWaiver = showWaiver;
    scope.signWaiver = signWaiver;
    /* eslint-enable */
  }

  return {
    restrict: 'EA',
    transclude: true,
    templateUrl: 'mytrips/mytripCard.html',
    scope: {
      trip: '=',
      waivers: '='
    },
    link
  };
}
MyTripCard.$inject = ['$log', 'TripsUrlService', '$cookies', '$state'];
module.exports = MyTripCard;

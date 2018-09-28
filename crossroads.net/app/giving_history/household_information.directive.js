// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function() {
  'use strict()';

  module.exports = HouseholdInformation;

  HouseholdInformation.$inject = ['$log'];

  function HouseholdInformation($log) {
    return {
      restrict: 'EA',
      transclude: true,
      templateUrl: 'templates/household_information.html',
      scope: {
        profile: '='
      },
      link: link
    };

    function link(scope, el, attr) {
      scope.$watch('profile', function(newValue) {
        scope.profile.householdName = getHouseholdName(newValue);
      });
    }

    // Returns a household name composed of nicknames separated by '&', with groupings of last names also
    // separated by '&'.  Some examples:
    //   1) Bob Jones, Sandy Smith = Bob Jones & Sandy Smith
    //   2) Bob Jones, Sandy Jones = Bob & Sandy Jones
    //   3) Bob Jones, Sandy Smith, Erin Jones = Bob & Erin Jones & Sandy Smith
    function getHouseholdName(profile) {
      var householdMembers = profile.householdMembers;

      // Find "me" in the household members
      var me = _.find(householdMembers, { ContactId: profile.contactId });

      // If I'm not there (not sure this is possible...) or if I'm a statement type
      // of Individual, then return the name
      if (me === undefined || me.StatementTypeId === 1) {
        return (profile.nickName + ' ' + profile.lastName);
      }

      // Make a "bucketed" map of last names with associated household members
      // for those with statement type of Family
      householdMembers = _.groupBy(_.filter(householdMembers, { StatementTypeId: 2 }), function(member) {
        return (member.LastName.toLocaleUpperCase());
      });

      // Set the initial household name, having nicknames with my last name appearing first
      var householdName = buildLastNameEntry(householdMembers[me.LastName.toLocaleUpperCase()]);

      // Remove my last name from the household member map
      delete householdMembers[me.LastName.toLocaleUpperCase()];

      // Add names for every other last name to the household name
      _.forEach(householdMembers, function(members) {
        householdName = householdName + ' & ' + buildLastNameEntry(members);
      });

      return (householdName);
    }

    function buildLastNameEntry(householdMembers) {
      // Pluck each nickname out of the array of members, join them with '&', and append the last name
      return _.pluck(householdMembers, 'Nickname').join(' & ') + ' ' + householdMembers[0].LastName;
    }
  }
})();
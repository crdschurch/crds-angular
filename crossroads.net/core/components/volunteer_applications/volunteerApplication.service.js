(function(){
  'use strict()';

  module.exports = VolunteerApplication;

  VolunteerApplication.$inject = ['Page', 'Opportunity'];

  function VolunteerApplication(Page, Opportunity){

    return {

      /**
       * Decide which application to show
       * @returns one of adult, student or error
       */
      show : function(type, person){
        if (person.age >= 16 && type === 'adult') {
          return true;
        } else if ((person.age >= 10) && (person.age <= 15) && (type === 'student')) {
          return true;
        } else if (type === 'error') {
          return true;
        }
        return false;
      },
      /**
       * Sometimes the contact won't have a middlename
       * this method tries to handle that gracefully
       */
      middleInitial: function(person){
        if (person.middleName !== null && person.middleName !== undefined && person.middleName.length > 0) {
          return person.middleName.substring(0, 1);
        }
        return person.middleName;
      },
      /**
       * Get the Opportunity Response
       */
      getResponse: function(opportunityId, contactId){
        return Opportunity.GetResponse.get({
          id: opportunityId,
          contactId: contactId
        }).$promise;
      },
      /**
       * Gets the kids club application from the CMS
       */
      getPageInfo: function(){
        return Page.get({url: '/volunteer-application/kids-club/'});
      },
    };

  }

})();

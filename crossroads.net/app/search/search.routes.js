(function() {
  'use strict';

  module.exports = SearchRoutes;

  SearchRoutes.$inject = ['$stateProvider', '$urlMatcherFactoryProvider', '$locationProvider'];

  function SearchRoutes($stateProvider, $urlMatcherFactory, $locationProvider) {

    $stateProvider
    .state('search', {
      parent: 'noSideBar',
      url: '/search?type&q',
      controller: 'SearchController as search',
      templateUrl: 'search/search-results.html',
      data: {
        type: '',
        renderLegacyStyles: false
      },
      resolve:{
        $stateParams: '$stateParams',
        type: function($stateParams) {
          return $stateParams.type;
        },

        searchString: function($stateParams) {
          return $stateParams.q;
        }
      }
    });
  }

})();

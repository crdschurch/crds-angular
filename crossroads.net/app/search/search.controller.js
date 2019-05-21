(function() {
  'use strict';

  module.exports = SearchController;
  SearchController.$inject = ['$log',
  '$state',
  '$scope',
  '$filter',
  'ResponsiveImageService',
  'Search',
  'type',
  'searchString'];

  function SearchController(
    $log,
    $state,
    $scope,
    $filter,
    ResponsiveImageService,
    Search,
    type,
    searchString) {

    var vm = this;

    vm.type = type;
    $scope.searchString = searchString;
    vm.search = search;
    vm.getLink = getLink;
    vm.results = {hits: {found: 0}};
    vm.loading = false;
    vm.showResults = false;
    vm.error = false;

    doSearch();

    function search() {
      $state.go('search', {type: vm.type, q:$scope.searchString});
    }

    function doSearch() {
      if (!$scope.searchString) {
        vm.showResults = false;
        return;
      }

      vm.showResults = true;
      vm.loading = true;
      var filter = '';
      var parser = '';
      var size = 10000;
      var query = $scope.searchString;
      switch (vm.type){
        case 'media':
          filter = '(or type:\'Series\' type:\'Message\' type:\'Video\' type:\'Music\' type:\'MediaResource\')';
          break;
        case 'corkboard':
          filter = '(or type:\'NEED\' type:\'ITEM\' type:\'EVENT\' type:\'JOB\')';
          break;
      }

      if (query.indexOf('tags:') >= 0 || query.indexOf('speakers:') >= 0) {
        parser = 'structured';
      }

      Search.execute({q: query, fq: filter, 'q.parser': parser, size: size})
        .$promise.then(function(response) {
          if (response.error) {
            vm.showResults = false;
            vm.error = true;
          }

          vm.results = response;
          ResponsiveImageService.updateResponsiveImages();
          vm.loading = false;
        });
    }

    function getLink(item) {
      if (isMedia(item)) {
        return getMediaLink(item);
      } else if (isCorkboard(item)) {
        return getCorkboardLink(item);
      } else {
        return item.link;
      }
    }

    function isMedia(item) {
      return (
        item.type === 'Series' ||
        item.type === 'Message' ||
        item.type === 'Video' ||
        item.type === 'Music'
      );
    }

    function getMediaLink(item) {
      var title = $filter('replaceNonAlphaNumeric')(item.title);
      switch (item.type) {
        case 'Series':
          return '/series/' + item.id + '/' + title;
        case 'Message':
          return '/message/' + item.id + '/' + title;
        case 'Video':
        case 'Music':
          return '/media/' + item.id + '/' + title;
        default:

      }
    }

    function isCorkboard(item) {
      return (
        item.type === 'NEED' ||
        item.type === 'ITEM' ||
        item.type === 'EVENT' ||
        item.type === 'JOB'
      );
    }

    function getCorkboardLink(item) {
      return '/corkboard/detail/' + item.link;
    }
  }
})();

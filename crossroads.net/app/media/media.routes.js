(function() {
  'use strict';

  module.exports = MediaRoutes;

  MediaRoutes.$inject = ['$stateProvider', '$urlMatcherFactoryProvider'];

  function MediaRoutes($stateProvider, $urlMatcherFactory) {

    $stateProvider
      .state('media', {
        abstract: true,
        parent: 'noSideBar',
        url: '',
        controller: 'MediaController as media',
        template: '<ui-view/>',
        resolve: {
          Media: 'Media',
          Series: function(Media) {
            return Media.Series().get().$promise;
          },

          SingleMedia: function(Media) {
            return Media.SingleMedia().get().$promise;
          }
        },
        data: {
          meta: {
            title: 'Media',
            description: '',
            type: 'website',
            card: 'summary'
          }
        }
      })
      .state('media.seriesSingle', {
        url: '/series/{id:int}/:title?',
        controller: 'SingleSeriesController as series',
        templateUrl: 'templates/seriesSingle.html',
        resolve: {
          $stateParams: '$stateParams',
          $state: '$state',
          Selected: function(Media, Series, $stateParams, $state) {
            var singleSeries = _.find(Series.series, function(obj) {
              return (obj.id === $stateParams.id);
            });

            if (!singleSeries) {
              // Doing this here instead of controller to prevent flicker of unbound page
              $state.go('content', {link: '/page-not-found/'}, {location: 'replace'});
              return;
            }

            return singleSeries;
          },

          Meta: function(Selected, $state) {
            $state.next.data.meta = {
             title: Selected.title,
             description: Selected.description,
             type: 'article',
             card: 'summary',
             image: Selected.image
           };
            return $state.next.data.meta;
          },

          Messages: function(Media, Selected) {
            var item = Media.Messages({seriesId: Selected.id}).get().$promise;
            return item;
          }
        }
      })
      .state('media-single', {
        parent: 'screenWidth',
        url: '/media/single',
        controller: 'MediaController as media',
        templateUrl: 'templates/mediaSingle.html',
        resolve: {
          //Empty Resolve to ensure loading spinner works
        }
      })
      .state('messageSingle', {
        parent: 'screenWidth',
        url: '/message/:id/:title?',
        controller: 'SingleMediaController as singleMedia',
        templateUrl: 'templates/mediaSingle.html',
        data: {
          meta: {}
        },
        resolve: {
          Media: 'Media',
          $stateParams: '$stateParams',
          $state: '$state',
          ItemProperty: function() {
            return 'message';
          },

          SingleMedia: function(Media, $stateParams, $state) {
            var item = Media.Messages({id: $stateParams.id}).get().$promise;
            item.then(redirectIfItemNotFound);
            return item;

            // Doing this here instead of controller to prevent flicker of unbound page
            function redirectIfItemNotFound(data) {
              var media = data.message;
              if (!media) {
                $state.go('content', {link: '/page-not-found/'}, {location: 'replace'});
              }
            }
          },

          Meta: function(SingleMedia, $state) {
            var message = SingleMedia.message;
            var image = _.get(message, 'messageVideo.still') || _.get(message, 'messageAudio.still');

            $state.next.data.meta = {
              title: message.title,
              description: message.description,
              type: 'article',
              card: 'summary',
              image: image
            };
            return $state.next.data.meta;
          },

          ParentMedia: function(Media, SingleMedia) {
            var message = SingleMedia.message;
            if (!message) {
              return null;
            }

            var parent = message.series;
            return parent;
          },

          ImageURL: function(Meta) {
            return _.get(Meta, 'image.filename');
          }
        }
      })
      .state('mediaSingle', {
        parent: 'screenWidth',
        url: '/media/{id:int}/:title?',
        controller: 'SingleMediaController as singleMedia',
        templateUrl: 'templates/mediaSingle.html',
        data: {
          meta: {}
        },
        resolve: {
          Media: 'Media',
          $rootScope: '$rootScope',
          $stateParams: '$stateParams',
          $state: '$state',
          ItemProperty: function(SingleMedia) {
            return Object.keys(SingleMedia)[0];
          },

          SingleMedia: function(Media, $stateParams, $state) {
            var item = Media.SingleMedia({id: $stateParams.id}).get().$promise;
            item.then(redirectIfItemNotFound);
            return item;

            // Doing this here instead of controller to prevent flicker of unbound page
            function redirectIfItemNotFound(data) {
              var media = data[Object.keys(data)[0]];
              if (!media) {
                $state.go('content', {link: '/page-not-found/'}, {location: 'replace'});
              }
            }
          },

          Meta: function(SingleMedia, $state) {
            $state.next.data.meta = {
              title: SingleMedia[Object.keys(SingleMedia)[0]].title,
              description: SingleMedia[Object.keys(SingleMedia)[0]].description,
              type: 'article',
              card: 'summary',
              image: SingleMedia[Object.keys(SingleMedia)[0]].still
            };
            return $state.next.data.meta;
          },

          ParentItemProperty: function() {
            return null;
          },

          ParentMedia: function() {
            return null;
          },

          ImageURL: function(SingleMedia) {
            return _.get(SingleMedia[Object.keys(SingleMedia)[0]], 'still.filename');
          }
        }
      });
  }
})();

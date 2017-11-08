
LiveStreamRouter.$inject = ['$httpProvider', '$stateProvider'];

export default function LiveStreamRouter($httpProvider, $stateProvider) {
  $httpProvider.defaults.useXDomain = true;

  $stateProvider
    .state('live', {
      parent: 'screenWidth',
      url: '/live',
      template: '<landing></landing>',
      data: {
        meta: {
          title: 'Live',
          description: ''
        }
      },
      resolve: {
        preloadStreamStatus: (StreamStatusService, StreamspotService) =>
            StreamStatusService.presetStreamStatus(StreamspotService.events)
      }
    })
    .state('livestream', {
      parent: 'noHeaderOrFooter',
      url: '/live/stream',
      template: '<stream></stream>',
      data: {
        meta: {
          title: 'Live',
          description: ''
        }
      },
      resolve: {
        preloadStreamStatus: StreamspotService => StreamspotService.checkBroadcasting()
      }
    })
  ;
}

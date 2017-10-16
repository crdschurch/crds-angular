export default function WaiversRoutes($stateProvider) {
  $stateProvider
    .state('sign-waiver', {
      parent: 'centeredContentPage',
      url: '/waivers/sign/:waiverId/:eventParticipantId?title&eventName',
      template: '<sign-waiver></sign-waiver>',
      data: {
        isProtected: true,
        meta: {
          title: 'Sign Waiver',
          description: ''
        }
      },
      resolve: {
        $log: '$log',
        $rootScope: '$rootScope',
        $state: '$state',
        loggedin: crds_utilities.checkLoggedin,
        waiversService: 'WaiversService'
      }
    })
    .state('accept-waiver', {
      parent: 'noHeaderOrFooter',
      url: '/waivers/accept/:guid',
      controller: ($log, $rootScope, $state, waiversService) => {
        const { guid } = $state.params;

        waiversService.acceptWaiver(guid).then(() => {
          $rootScope.$emit('notify', $rootScope.MESSAGES.waiverSuccess);
          $state.go('mytrips');
        }).catch((err) => {
          $log.error(err);
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          $state.go('content', { link: '/servererror/' });
        });
      },
      data: {
        isProtected: false,
        meta: {
          title: 'Accept Waiver',
          description: ''
        }
      },
      resolve: {
        $log: '$log',
        $rootScope: '$rootScope',
        $state: '$state',
        waiversService: 'WaiversService'
      }
    });
}

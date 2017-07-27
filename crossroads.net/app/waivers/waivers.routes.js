export default function WaiversRoutes($stateProvider) {
  $stateProvider
    .state('sign-waiver', {
      parent: 'centeredContentPage',
      url: '/waivers/sign/:waiverId/:eventParticipantId',
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
      template: '<accept-waiver></accept-waiver>',
      controller: ($log, $rootScope, $state, waiversService) => {
        const { guid } = $state.params;

        waiversService.acceptWaiver(guid).then(() => {
          // growl message?
          $state.go('mytrips');
        }).catch((err) => {
          $log.error(err);
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);

          // TODO: Show something on the screen
        });
      },
      data: {
        isProtected: true,
        meta: {
          title: 'Accept Waiver',
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
    });
}

MyServeRouter.$inject = ['$httpProvider', '$stateProvider'];

export default function MyServeRouter($httpProvider, $stateProvider) {
  $stateProvider
    .state('serve-signup', {
      parent: 'noSideBar',
      url: '/serve-signup',
      controller: 'MyServeController as serve',
      templateUrl: 'my_serve/myserve.html',
      data: {
        isProtected: true,
        meta: {
          title: 'Signup to Serve',
          description: ''
        }
      },
      params: {
        messageSent: null
      },
      resolve: {
        loggedin: crds_utilities.checkLoggedin,
        ServeOpportunities: 'ServeOpportunities',
        /*@ngInject*/
        leader: function (ServeTeamService) {
          return ServeTeamService.getIsLeader();
        },
        $cookies: '$cookies',
        Groups: function (ServeOpportunities, $cookies) {
          return ServeOpportunities.ServeDays.query({
            id:   $cookies.get('userId')
          }).$promise;
        }
      },
      onEnter: function ($state, $stateParams, $cookies, $rootScope) {
        // show sent growl if redirect from message sent success route
        if($stateParams.messageSent) {
          $rootScope.$emit('notify', $rootScope.MESSAGES.emailSent);
        }
      },
    })
    .state('serve-signup.message-success', {
      parent: 'noSideBar',
      url: '/serve-signup/message/success',
      onEnter: function ($state, $stateParams, $cookies) {
        $state.go('serve-signup', { messageSent: true});
      },
    })
    .state('serve-signup.message', {
      parent: 'noSideBar',
      url: '/serve-signup/message/:groupId',
      template: '<serve-team-message></serve-team-message>',
      resolve: {
        /*@ngInject*/
        leader: function (ServeTeamService) {
          return ServeTeamService.getIsLeader().then((data) => { ServeTeamService.isLeader = data.isLeader; });
        },
      },
      data: {
        isProtected: true,
        meta: {
          title: 'Send Message',
          description: ''
        }
      }
    });
}

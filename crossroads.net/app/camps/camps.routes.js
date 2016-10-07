export default function CampRoutes($stateProvider) {

  $stateProvider
    .state('crossroads-camp', {
      parent: 'noSideBar',
      url: '/camps/:campId',
      template:'<crossroads-camp></crossroads-camp>',
      data: {
        isProtected: true,
        meta: {
          title: 'Camp Signup',
          description: 'Join us for camp!'
        }
      },
      resolve: {
        loggedin: crds_utilities.checkLoggedin,
        $cookies: '$cookies',
        $stateParams: '$stateParams'
      }
  });
}

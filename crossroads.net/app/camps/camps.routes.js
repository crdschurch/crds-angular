function getCampInfo(CampsService, $stateParams) {
  const id = $stateParams.campId;
  return CampsService.getCampInfo(id);
}

export default function CampRoutes($stateProvider) {
  $stateProvider
    .state('camps-dashboard', {
      parent: 'noSideBar',
      url: '/mycamps',
      template: '<camps-dashboard dashboard="$resolve.dashboard"></camps-dashboard>',
      data: {
        isProtected: true,
        meta: {
          title: 'Camps Dashboard',
          description: 'What camps are you signed up for?'
        }
      },
      resolve: {
        loggedin: crds_utilities.checkLoggedin,
        campsService: 'CampsService',
        dashboard: campsService => campsService.getCampDashboard()
      }
    })
    .state('campsignup', {
      parent: 'noSideBar',
      url: '/camps/:campId',
      template: '<crossroads-camp></crossroads-camp>',
      data: {
        isProtected: true,
        meta: {
          title: 'Camp Signup',
          description: 'Join us for camp!'
        }
      },
      resolve: {
        loggedin: crds_utilities.checkLoggedin,
        campsService: 'CampsService',
        getCampInfo,
        $stateParams: '$stateParams',
        family: (campsService, $stateParams) => {
          const id = $stateParams.campId;
          return campsService.getCampFamily(id);
        }
      }
    })
    .state('campsignup.family', {
      url: '/family',
      template: '<camps-family></camps-family>'
    })
    .state('campsignup.application', {
      url: '/:page/:contactId',
      template: '<camps-application-page></camps-application-page>'
    })
    ;
}

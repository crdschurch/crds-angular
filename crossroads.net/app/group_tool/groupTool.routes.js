import CONSTANTS from 'crds-constants';

GroupToolRouter.$inject = ['$httpProvider', '$stateProvider'];
export default function GroupToolRouter($httpProvider, $stateProvider) {

$httpProvider.defaults.useXDomain = true;
$httpProvider.defaults.headers.common['X-Use-The-Force'] = true;
  $stateProvider
    .state('grouptool.leaderresources', {
      parent: 'noSideBar',
      url: '/groups/leader/resources',
      template: '<group-tool-cms></group-tool-cms>',
      data: {
        meta: {
          title: 'Leader Resources',
          description: ''
        }
      },
    })
    ;
}
import controller  from './groupToolCms.controller';

GroupToolCmsComponent.$inject = [ ];

export default function GroupToolCmsComponent() {

  let GroupToolCmsComponent = {
      restrict: 'E',
      bindings: {
        url: '@',
      },
      bindToController: true,
      controller: controller,
      controllerAs: 'cms',
      templateUrl: 'cms/groupToolCms.html',
    };

  return GroupToolCmsComponent;
}
(function () {
    angular.module('crossroads.core').
        factory('User', ['$resource','$log', UserService]);

    //By declaring api/user a resource, angular provides us with helpful verbs to perform CRUD operations. (save/update)


    function UserService($resource, $log) {
        var User = $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/user');
        var newuser = new User();
        return newuser;
    }

})()

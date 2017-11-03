require('./content.html');
require('../templates/noHeaderOrFooter.html');
require('../templates/noSideBar.html');
require('../templates/rightSideBar.html');
require('../templates/leftSideBar.html');
require('../templates/screenWidth.html');
require('../templates/headerOnly.html');
require('../templates/homePage.html');
require('../templates/centeredContentPage.html');
require('../templates/goCincinnati.html');
require('../templates/brave.html');

var app = angular.module("crossroads.core");
app.controller("ContentCtrl", ['$rootScope', '$scope', '$state', '$stateParams', '$log', 'ContentPageService', '$sce', require("./content_controller")]);
app.directive('dynamicContent', require('./dynamicContent.directive'));

(function () {
  'use strict';
  module.exports = CorkboardController;

  CorkboardController.$inject = ['$log', '$state', '$rootScope', '$scope', '$stateParams',
    '$window', 'MESSAGES', 'CORKBOARD_EVENTS', 'CORKBOARD_TEMPLATES', 'CORKBOARD_ADMIN_ROLE_ID', 'selectedItem',
    'CorkboardPostTypes', 'CorkboardListings', 'CorkboardSession', 'ContactAboutPost', 'Session'];

  function CorkboardController($log, $state, $rootScope, $scope, $stateParams, $window,
    MESSAGES, CORKBOARD_EVENTS, CORKBOARD_TEMPLATES, CORKBOARD_ADMIN_ROLE_ID, selectedItem,
    CorkboardPostTypes, CorkboardListings, CorkboardSession, ContactAboutPost, Session) {

    $rootScope.MESSAGES = MESSAGES;
    $rootScope.location = $window.location;

    var vm = this;

    vm.cancelReply = cancelReply;
    vm.canRemove = canRemove;
    vm.flagAsInappropriate = 'Flag as Inappropriate';
    vm.flaggedAsInappropriate = 'Flagged as Inappropriate';
    vm.flagging = false;
    vm.flagConfirm = flagConfirm;
    vm.flagPost = flagPost;
    vm.flagState = vm.flagAsInappropriate;
    vm.params = $stateParams;
    vm.postTypes = CorkboardPostTypes;
    vm.filteredPosts = getFilteredList(CorkboardSession.posts);
    // list of post types to be used to display the filter and create buttons on the corkboard home page
    // in the same order as the original design
    vm.postTypeList = _.sortBy(vm.postTypes, 'index');
    //Exposed for testing, use remove from view
    vm.removeConfirm = removeConfirm;
    vm.removing = false;
    vm.removePost = removePost;
    vm.reply = reply;
    vm.replyText = '';
    vm.sending = false;
    vm.selectedItem = selectedItem;
    vm.showReply = showReply;
    vm.showReplySection = $stateParams.showReplySection ? true : false;

    // paging stuff
    vm.itemsPerPage = 25;
    vm.maxPageNumber = calculateMaxPages(vm.itemsPerPage, vm.filteredPosts.length);
    vm.pageNumber = 0;

    var currentPage = parseInt($state.params.page) || 0;
    setCurrentPage(currentPage)

    if (vm.pageNumber > 0 && currentPage > vm.pageNumber) {
      // requested page number is too high, redirect to a valid page
      $state.go($state.current, { page: vm.pageNumber }, { reload: true });
      return;
    }

    function calculateMaxPages(itemsPerPage, totalItems) {
      return Math.ceil(totalItems / itemsPerPage);
    }

    function setCurrentPage(pageNumber) {
      var oldNumber = vm.pageNumber;
      vm.pageNumber = Math.max(1, Math.min(vm.maxPageNumber, pageNumber));

      return oldNumber != vm.pageNumber ? true : false;
    }

    $scope.isFirstPage = function() {
      return vm.pageNumber <= 1 ? true : false;
    };

    $scope.isLastPage = function() {
      return vm.pageNumber >= vm.maxPageNumber ? true : false;
    };
    
    $scope.gotoPrevPage = function() {
      vm.pageNumber -= 1;
      $state.go($state.current, { page: vm.pageNumber  });
    };

    $scope.gotoNextPage = function() {
      vm.pageNumber += 1;
      $state.go($state.current, { page: vm.pageNumber  });
    };


    //Datepicker STUFF
    vm.hstep = 1;
    vm.mstep = 15;
    vm.isMeridian = true;
    vm.openDatePicker = openDatePicker;

    function openDatePicker($event) {
      $event.preventDefault();
      $event.stopPropagation();

      vm.opened = true;
    }

    //END Datepicker STUFF


    var unregisterStateNotFound = $rootScope.$on('$stateNotFound', function (event, unfoundState, fromState, fromParams) {
      if (unfoundState.toParams.link) {
        $window.location = addLeadingSlashIfNecessary(unfoundState.toParams.link);
      } else {
        $window.location = addLeadingSlashIfNecessary(unfoundState.to);
      }
    });

    $scope.$on('$destroy', function(event, data) {
      unregisterStateNotFound();
    });

    $scope.$on('$stateChangeStart', function(event, toState, toParams, fromState, fromParams) {
      // This is a workaround.  Sometimes ui-router will reload the controller on state change
      // and sometimes not (e.g., browser Back button).  Ensure that vm.pageNumber is updated
      // properly even when the controller is not reloaded.
      if (toState.name == 'corkboard.root' || toState.name == 'corkboard.filtered') {
        var pageNumber = parseInt(toParams.page) || 0;
        setCurrentPage(pageNumber);
      }
    });

    $scope.$on(CORKBOARD_EVENTS.postAdded, function (event, data) {
      CorkboardListings.InvalidateCache();
      CorkboardSession.posts.unshift(data);
      $state.go('corkboard.root', null, { inherit: false, reload: true });
    }
    );

    $scope.$on(CORKBOARD_EVENTS.postCanceled, function (event, data) {
      $state.go('corkboard.root', null, { inherit: false, reload: true });
    });

    $scope.getStartIndex = function() {
      return (vm.pageNumber - 1) * vm.itemsPerPage;
    };

    function getFilteredList(posts) {
      var searchType = vm.params.type ? vm.params.type.toUpperCase() : null;

      var result = _.filter(posts, function(item) {
        if (!searchType || item.PostType.toUpperCase() === searchType)
          return true;
      });
      
      return result;
    }

    function flagConfirm() {
      if (confirm('Are you sure you want to flag this post?')) {
        return flagPost();
      }
    }

    function flagPost() {
      vm.flagState = vm.flaggedAsInappropriate;
      vm.flagging = true;
      var promise = CorkboardListings.flag().post({ id: vm.selectedItem._id.$oid }).$promise;
      promise.then(function (post) {
        vm.selectedItem.FlagCount++;

        // Remove angular properties that are added by saving individual object, that are not needed by the CorkboardListings
        delete post.$promise;
        delete post.$resolved;

        vm.flagging = false;
      }, function (error) {

        vm.selectedItem.removed = false;
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        vm.flagging = false;
        vm.flagState = vm.flagAsInappropriate;
      });

      return promise;
    }

    function canRemove() {
      return ($rootScope.userid == vm.selectedItem.ContactId) ||
        ($rootScope.roles && $rootScope.roles.filter(
          function (e) {
            return e.Id === CORKBOARD_ADMIN_ROLE_ID;
          }).length > 0);
    }

    function removeConfirm() {
      if ($window.confirm('Are you sure you want to delete this post?')) {
        removePost();
      }
    }

    function removePost() {
      vm.removing = true;
      vm.selectedItem.Removed = true;
      var promise = CorkboardListings.post().save(vm.selectedItem).$promise;
      promise.then(function (post) {
        // Remove angular properties that are added by saving individual object, that are not needed by the CorkboardListings
        delete post.$promise;
        delete post.$resolved;

        var itemToRemove = _.find(CorkboardSession.posts, function (item) {
          return (item._id.$oid === vm.selectedItem._id.$oid);
        });

        CorkboardListings.InvalidateCache();
        CorkboardSession.posts.splice(CorkboardSession.posts.indexOf(itemToRemove), 1);

        $state.go('corkboard.root', null, { inherit: false, reload: true });
        $rootScope.$emit('notify', $rootScope.MESSAGES.corkboardRemoveSuccess);
        vm.removing = false;
      }, function (error) {

        vm.selectedItem.removed = false;
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        vm.removing = false;
      });

      return promise;
    }

    function showReply() {

      // verify that the user is logged in before allowing a reply
      if (!$rootScope.userid) {
        Session.addRedirectRoute('corkboard.reply', $state.params);
        $state.go('login');
      } else {
        vm.showReplySection = true;
      }

    }

    function reply() {
      vm.sending = true;
      if (vm.replyForm.$invalid) {
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        vm.sending = false;
      } else {
        var link = __APP_SERVER_ENDPOINT__ + 'corkboard/detail/' + vm.selectedItem._id.$oid;

        var replyPost = {
          templateId: CORKBOARD_TEMPLATES.replyToTemplateId,
          fromContactId: crds_utilities.getCookie('userId'),
          replyToContact: crds_utilities.getCookie('userId'),
          toContactId: vm.selectedItem.ContactId,
          mergeData: { Title: vm.selectedItem.Title, Description: vm.selectedItem.Description, ReplyText: vm.replyText, Link: link }
        };

        var promise = ContactAboutPost.post().save(replyPost).$promise;
        promise.then(function () {
          vm.showReplySection = true;
          $rootScope.$emit('notify', $rootScope.MESSAGES.corkboardReplySuccess);
          $state.go('corkboard.root', null, { inherit: false, reload: true });
          vm.sending = false;
        }, function (error) {

          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          vm.sending = false;
        });
      }
    }

    function cancelReply() {
      if (vm.replyForm.$dirty) {

        if (!confirm('Are you sure you want to cancel this reply?')) {
          return;
        }
      }

      vm.showReplySection = false;
      vm.replyText = '';
      vm.replyForm.$setPristine();
    }

    function addLeadingSlashIfNecessary(link) {
      if (_.startsWith(link, '/') === false) {
        return '/' + link;
      }

      return link;
    }
  }
})();

require('../../app/corkboard');

describe('CorkboardController', function () {

  beforeEach(angular.mock.module('crossroads.corkboard'));

  var crdsApiEndpoint = window.__env__.CRDS_API_ENDPOINT || 'http://gatewayint.crossroads.net/gateway/';
  var crdsCorkboardApiEndpoint = window.__env__.CRDS_CORKBOARD_API_ENDPOINT || "http://gatewayint.crossroads.net/corkboardAPI/";
  var crdsCoreEndpoint = window.__env__.CRDS_CORE_ENDPOINT || "http://int.crossroads.net/";
  var $rootScope;
  var $scope;
  var $log;
  var $httpBackend;
  var selectedItem;
  var CorkboardPostTypes;
  var $controller;
  var $state;
  var Session;
  var $window;
  var $q;

  var corkboardListings;
  var mockAdminRootScope = getAdminRootScope();
  var mockCreatorRootScope = getCreatorRootScope();
  var mockNonCreatorRootScope = getNonCreatorRootScope();
  var mockNotLoggedInRootScope = getNotLoggedInRootScope();
  var mockEmailRequest = getEmailRequest();
  var mockForm = getForm();
  var mockItems = getItems();
  var mockContentSiteConfigService = {};
  mockContentSiteConfigService.siteconfig = { title: '' };

  beforeEach(angular.mock.module(function ($provide) {
    $provide.value('items', mockItems);
    $provide.value('selectedItem', getSelectedItem());
    $provide.value('ContentSiteConfigService', mockContentSiteConfigService);
  }));

  var $controller, $log, $httpBackend, $scope, selectedItem, CorkboardPostTypes, $state, Session, $window, ContactAboutPost;
  beforeEach(inject(function (_$controller_, _$rootScope_, _$log_, _$state_, _Session_, _$window_, $injector, _CorkboardListings_, _$q_) {
    $log = _$log_;
    $httpBackend = $injector.get('$httpBackend');

    $q = _$q_;
    $httpBackend.whenGET(/SiteConfig*/).respond('');
    corkboardListings = _CorkboardListings_;
    $rootScope = _$rootScope_;
    $scope = _$rootScope_.$new();
    selectedItem = $injector.get('selectedItem');
    CorkboardPostTypes = $injector.get('CorkboardPostTypes');
    $controller = _$controller_('CorkboardController', {
      $scope: $scope,
      $rootScope: $rootScope,
      CorkboardListings: corkboardListings
    });
    $state = _$state_;
    Session = _Session_;
    $window = _$window_;
    ContactAboutPost = $injector.get('ContactAboutPost');
  }));

  it("Should have hStep = 1", function () {
    expect($controller.hstep).toBe(1);
  });

  it("Should call save endpoint", function () {
    $controller.removePost();
    $controller.selectedItem.Removed = true;

    $httpBackend.expectPOST(crdsCorkboardApiEndpoint + 'api/posts', $controller.selectedItem).respond(200);
    $httpBackend.expectGET(crdsCorkboardApiEndpoint + 'api/posts').respond([]);
    expect($controller.selectedItem.Removed).toBeTruthy();
  });

  describe('Admin User', function () {
    beforeEach(inject(function (_$controller_) {
      $rootScope = _.merge($rootScope, mockAdminRootScope);
      $controller = _$controller_('CorkboardController', { $scope: $scope });
    }));

    it("Should have remove rights", function () {
      expect($controller.canRemove()).toBeTruthy();
    });
  });

  describe('Creator User', function () {
    beforeEach(inject(function (_$controller_) {
      $rootScope = _.merge($rootScope, mockCreatorRootScope);
      $controller = _$controller_('CorkboardController', { $scope: $scope, $rootScope: $rootScope });
    }));

    it("Should have remove rights", function () {
      expect($controller.canRemove()).toBeTruthy();
    });
  });

  describe('Non Creator User', function () {
    beforeEach(inject(function (_$controller_) {
      $rootScope = _.merge($rootScope, mockNonCreatorRootScope);
      $controller = _$controller_('CorkboardController', { $scope: $scope, $rootScope: $rootScope });
    }));

    it("Should NOT have remove rights", function () {
      expect($controller.canRemove()).toBeFalsy();
    });

    it("Should show reply section", function () {
      $controller.showReply();
      expect($controller.showReplySection).toBeTruthy();
    })

    it("Should not call save if invalid Reply Form", function () {
      spyOn(ContactAboutPost, 'post');

      $controller.replyForm = getForm();
      $controller.replyForm.$invalid = true;

      $controller.reply();

      expect(ContactAboutPost.post).not.toHaveBeenCalled();
    });

    it("Should call save if Reply Form is valid", function () {
      $controller.replyForm = getForm();
      $controller.reply();

      $httpBackend.expectPOST(crdsApiEndpoint + 'api/sendemail', mockEmailRequest).respond(200);
      $httpBackend.expectGET(crdsCorkboardApiEndpoint + 'api/posts').respond([]);
    });

  });

  describe('Not Logged In User', function () {
    beforeEach(inject(function (_$controller_) {
      $rootScope = _.merge($rootScope, mockNotLoggedInRootScope);
      $controller = _$controller_('CorkboardController', { $scope: $scope, $rootScope: $rootScope });
    }));

    it("Should NOT have remove rights", function () {
      expect($controller.canRemove()).toBeFalsy();
    });

    it("Should redirect to login route", function () {
      spyOn($state, 'go');
      $controller.showReply();
      expect($state.go).toHaveBeenCalledWith('login');
    });

    it("Should save off redirect route", function () {
      spyOn(Session, 'addRedirectRoute');
      spyOn($state, 'go');
      $controller.showReply();
      expect(Session.addRedirectRoute).toHaveBeenCalledWith('corkboard.reply', $state.params);
    });

    it("Should prompt for confirming the cancel if form is dirty", function () {
      spyOn($window, 'confirm').and.callFake(function () {
        return false;
      });

      $controller.replyForm = mockForm;

      $controller.cancelReply();
      expect($window.confirm).toHaveBeenCalled();
    });

    it("Should not prompt for confirming the cancel if form is not dirty", function () {
      spyOn($window, 'confirm').and.callFake(function () {
        return false;
      });

      $controller.replyForm = mockForm;
      $controller.replyForm.$dirty = false;

      $controller.cancelReply();
      expect($window.confirm).not.toHaveBeenCalled();
    });

    it("Should set showReplySection to false on Cancel", function () {
      spyOn($window, 'confirm').and.callFake(function () {
        return false;
      });

      $controller.showReplySection = true;
      $controller.replyForm = mockForm;

      $controller.cancelReply();
      expect($controller.showReplySection).toBeFalsy();
    });

    it("Should set replyText to empty string on Cancel", function () {
      spyOn($window, 'confirm').and.callFake(function () {
        return false;
      });

      $controller.replyForm = mockForm;
      $controller.replyText = "This is the reply text";

      $controller.cancelReply();
      expect($controller.replyText).toBe("");
    });

    it("Should increment flagCount and set flagSate on post being flagged", function () {
      $controller.selectedItem.FlagCount = 2;

      spyOn(corkboardListings, 'flag').and.callFake(function () {
        return {
          post: function () {
            var deferred = $q.defer();
            deferred.resolve(result);

            var result = {
              $promise: deferred.promise,
              $resolved: true
            };

            return result;
          }
        }
      });

      $controller.flagPost();
      $rootScope.$apply();

      expect($controller.flagState).toBe($controller.flaggedAsInappropriate);
      expect($controller.selectedItem.FlagCount).toBe(3);
    });

    it("Should call flag service on post being flagged", function () {
      spyOn(corkboardListings, 'flag').and.callFake(function () {
        return {
          post: function () {
            var deferred = $q.defer();
            deferred.resolve(result);

            var result = {
              $promise: deferred.promise,
              $resolved: true
            };

            return result;
          }
        }
      });

      $controller.flagPost();
      $rootScope.$apply();

      expect(corkboardListings.flag).toHaveBeenCalled();
    });

    it("Should not update flagCount or set flagSate on failed API to flag a post", function () {
      $controller.selectedItem.FlagCount = 2;

      spyOn(corkboardListings, 'flag').and.callFake(function () {
        return {
          post: function () {
            var deferred = $q.defer();
            deferred.reject();

            var result = {
              $promise: deferred.promise
            };

            return result;
          }
        }
      });

      $controller.flagPost();
      $rootScope.$apply();

      expect($controller.flagState).not.toBe($controller.flaggedAsInappropriate);
      expect($controller.flagState).toBe($controller.flagAsInappropriate);
      expect($controller.selectedItem.FlagCount).toBe(2);
    });

  });

  function getAdminRootScope() {
    return {
      "userid": 1822076,
      "username": " Dan",
      "useremail": "drye@crossroads.net",
      "roles": [{ "Id": 2, "Name": "Administrators" },
      { "Id": 13, "Name": "Email Quota 5000" },
      { "Id": 28, "Name": "Check In Only" },
      { "Id": 34, "Name": "DBT Full Rights" },
      { "Id": 35, "Name": "Classroom Manager Only" },
      { "Id": 37, "Name": "Portal Content Manager" },
      { "Id": 67, "Name": "CorkboardAdmin" }]
    };
  }

  function getCreatorRootScope() {
    return {
      "userid": 2562379,
      "username": " Dan",
      "useremail": "drye@crossroads.net",
      "roles": []
    };
  }

  function getNonCreatorRootScope() {
    return {
      "userid": 1822076,
      "username": " Dan",
      "useremail": "drye@crossroads.net",
      "roles": []
    };
  }

  function getNotLoggedInRootScope() {
    return {};
  }

  function getForm() {
    return {
      $dirty: true,
      $invalid: false,
      $setPristine: function () {
        this.$dirty = false;
      }
    };
  }

  function getItems() {
    return [{ "_id": { "$oid": "559bdd6c966686116894e13c" }, "Time": "2015-06-25T00:00:00Z", "Title": "Super Ultra Mega Party", "Location": "SPACE", "Date": "2015-07-01T04:00:00Z", "Description": "This event is in the past! YOU GOTTA GO BACK IN TIME AND BLOW UP THE OCEAN", "PostType": "EVENT", "ContactId": "2562379", "Removed": false, "DatePosted": { "$date": 1436278124112 } }, { "_id": { "$oid": "559ae73f9666aa0720a0bda2" }, "Title": "This is for Charlie's eye's only", "Description": "Super secret email testing message, DO NOT DELETE, until you are done.", "PostType": "NEED", "ContactId": "1822076", "Removed": false, "DatePosted": { "$date": 1436215102866 } }, { "_id": { "$oid": "559ad5899666a60720541adc" }, "Title": "I need to know that YOU can't delete this post!", "Description": "I MUST KNOW to pass this case.", "PostType": "NEED", "ContactId": "2398505", "Removed": false, "DatePosted": { "$date": 1436210569426 } }];
  }

  function getSelectedItem() {
    return { "_id": { "$oid": "559bdd6c966686116894e13c" }, "Time": "2015-06-25T00:00:00Z", "Title": "Super Ultra Mega Party", "Location": "SPACE", "Date": "2015-07-01T04:00:00Z", "Description": "This event is in the past! YOU GOTTA GO BACK IN TIME AND BLOW UP THE OCEAN", "PostType": "EVENT", "ContactId": "2562379", "Removed": false, "DatePosted": { "$date": 1436278124112 }, FlagCount: 2 };
  }

  function getEmailRequest() {
    return { "templateId": 11419, "fromContactId": "", "toContactId": "2562379", "mergeData": { "Title": "Super Ultra Mega Party", "Description": "This event is in the past! YOU GOTTA GO BACK IN TIME AND BLOW UP THE OCEAN", "ReplyText": "", "Link": crdsCoreEndpoint + "corkboard/detail/559bdd6c966686116894e13c" } };
  }

});

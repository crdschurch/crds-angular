require('../../app/app');

describe('Serve Modal Controller', function() {

  var controller, 
      $scope, 
      $log, 
      $httpBackend, 
      $modalInstance, 
      mockModalInstance;

  var mockDates = {
    'fromDate': new Date(), 
    'toDate' : new Date() 
  };

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide){
    mockModalInstance = jasmine.createSpyObj('$modalInstance', ['close']);
    mockModalInstance.close.and.callFake(function(obj){
      return true;
    });
    
    $provide.value('dates', mockDates); 
    $provide.value('$modalInstance', mockModalInstance); 
  }));

  beforeEach(inject(function(_$controller_, _$log_, $injector){
    $log = _$log_;
    $httpBackend = $injector.get('$httpBackend');
    $modalInstance = $injector.get('$modalInstance');
    $scope = {};
    controller = _$controller_('ServeModalController', { $scope: $scope });
    controller.filterdates = setupFormErrors(); 
  }));

  it('should have an error when the TO date is prior to the FROM date', function(){
    controller.toDate = new Date();
    controller.toDate.setDate(controller.toDate.getDate() -1);
    controller.fromDate = new Date();
    var ret = controller.readyFilterByDate();
    expect(ret).toBe(false);
    expect(controller.filterdates.todate.$error.fromDate).toBe(true);
  });

  it('should have an error when the FROM date is after the TO date', function(){
    controller.fromDate = formatDate(new Date(), 16);
    controller.toDate = formatDate(new Date(), 10);
    var ret = controller.readyFilterByDate();
    expect(ret).toBe(false);
    expect(controller.filterdates.fromdate.$error.fromDateToLarge).toBe(true);
  }); 

  function setupFormErrors(){
    return {
      todate: { 
        $error : { 
          fromDate: false,
          date: false,
          required: false
        }
      },
      fromdate : {
        $error: {
          required: false, 
          date: false,
          fromDateToLarge: false
        }
      }
    };
   }

  function formatDate(date, days){
    if(days === undefined){
      days = 0; 
    }
    var d = moment(date);
    d.add(days, 'd');
    return d.format('MM/DD/YYYY');
  }

});

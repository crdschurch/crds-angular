require('../../../../app/app');

describe('CommitmentList Directive', function() {
  var $compile;
  var $rootScope;
  var scope;
  var templateString;
  var commitmentList;
  var ImageService;

  beforeEach(angular.mock.module('crossroads.profile'));

  beforeEach(angular.mock.module(function($provide) {
    ImageService = { PledgeCampaignImageBaseURL: 'pledgecampaign/' };
    $provide.value('ImageService', ImageService);

    $provide.value('$state', { get: function() {} });
  }));

  beforeEach(
      inject(function(_$compile_, _$rootScope_) {
        $compile = _$compile_;
        $rootScope = _$rootScope_;

        scope = $rootScope.$new();
        scope.commitmentListInput = [
          {
            pledge_campaign_id: 1,
            pledge_campaign: 2,
            donor_display_name: 'Name',
            campaign_start_date: new Date(),
            campaign_end_date: new Date(),
            pledge_donations: 123,
            total_pledge: 456
          }
        ];

        templateString =
            '<commitment-list ' +
            ' commitment-list-input="commitmentListInput"></commitment-list>';
      })
  );

});

// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function() {
  'use strict()';

  module.exports = CommitmentList;

  CommitmentList.$inject = ['ImageService'];

  function CommitmentList(ImageService) {
    return {
      restrict: 'EA',
      transclude: true,
      templateUrl: 'templates/commitment_list.html',
      scope: {
        commitmentListInput: '=',
      },
      link: link
    };

    function link(scope) {
      scope.pledge_campaign_base_url = ImageService.PledgeCampaignImageBaseURL;

      scope.$watch('commitmentListInput', function(pledgeCommitments) {
        scope.pledgeCommitments = pledgeCommitments;
      });

      scope.commitmentMet = function(donations, commitment) {
        return (donations >= commitment);
      };

      scope.commitmentPercent = function(donations, commitment) {
        var percent = (donations / commitment) * 100;
        if (percent >= 100) {
          return 100;
        } else {
          return Math.round(percent);
        }
      };

      scope.commitmentRemaining = function(donations, commitment) {
        var remaining = commitment - donations;
        if (remaining < 0)
          remaining = 0;

        return remaining;
      };
    }
  }
})();

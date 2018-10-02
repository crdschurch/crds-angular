(function() {
    "use strict";

    module.exports = LeaveYourMark;

    LeaveYourMark.$inject = ['$resource'];

    function LeaveYourMark($resource) {
        return {
            campaignSummary: $resource(__GATEWAY_CLIENT_ENDPOINT__ + "api/campaign/summary/:pledgeCampaignId", {
                pledgeCampaignId: "@pledgeCampaignId"
            }, {
                query: {
                    method: 'GET',
                    isArray: true
                }})
        }
    }
}())

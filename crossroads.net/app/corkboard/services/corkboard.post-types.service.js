(function () {
'use strict';

module.exports = CorkboardPostTypes;

CorkboardPostTypes.$inject = [];

function CorkboardPostTypes() {
  return postTypes;
}

var postTypes = {};

postTypes.NEED = {
    index: 1,
    class: 'corkboard-need',
    icon: 'icon-lifebuoy',
    text: 'Need',
    href: '#lifebuoy',
    createButtonText: 'I have a need',
    createSref: 'corkboard.create-need',
    filterButtonText: 'Needs Posted',
    filterName: 'need',
    form: 'editPost'
  };
postTypes.ITEM = {
    index: 2,
    class: 'corkboard-item',
    icon: 'icon-bag',
    text: 'Item',
    href: '#bag',
    createButtonText: 'I have something to give',
    createSref: 'corkboard.create-give',
    filterButtonText: 'Items Posted',
    filterName: 'item',
    form: 'itemForm'
  };
postTypes.EVENT = {
    index: 3,
    class: 'corkboard-event',
    icon: 'icon-calendar',
    text: 'Event',
    href: '#calendar',
    createButtonText: "I'm posting an event",
    createSref: 'corkboard.create-event',
    filterButtonText: 'Upcoming Events',
    filterName: 'event',
    form: 'eventForm'
  };
postTypes.JOB = {
    index: 4,
    class: 'corkboard-job',
    icon: 'icon-megaphone4',
    text: 'Job',
    href: '#megaphone4',
    createButtonText: "I'm hiring",
    createSref: 'corkboard.create-job',
    filterButtonText: 'Job Listings',
    filterName: 'job',
    form: 'jobForm'
  };
})();

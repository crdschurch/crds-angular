(function() {
  'use strict';

  module.exports = ChildcareEvents;

  ChildcareEvents.$inject = [];

  /**
   * Stores a single event or a collection of events
   * that should be resolved in the routes of childcare
   */
  function ChildcareEvents() {

    var childCareEvents = {
      event: {},
      events: [],
      childcareEvent: {},
      setEvent: setEvent,
      setEvents: setEvents,
      setChildcareEvent: setChildcareEvent
    };

    function setEvent(event) {
      if (typeof event === 'object') {
        childCareEvents.event = event;
      } else {
        throw(new Error('this must be a single object'));
      }
    }

    function setEvents(events) {
      if (events.constructor === Array) {
        childCareEvents.events = events;
      } else {
        throw(new Error('this must be an array '));
      }
    }

    function setChildcareEvent(event) {
      if (typeof event === 'object') {
        childCareEvents.childcareEvent = event;
      } else {
        throw(new Error('this must be a single object'));
      }
    }

    return childCareEvents;
  }
})();

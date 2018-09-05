(function() {
  'use strict';
  module.exports = PreferredAirportService;

  function PreferredAirportService() {
    const consts = require('crds-constants');
    const preferredAirportAttributeTypeId = consts.ATTRIBUTE_TYPE_IDS.PREFERRED_AIRPORT;

    const COMMON_AIRPORT_NAMES = {
      CVG: 'Cincinnati(CVG)',
      LEX: 'Lexington(LEX)',
      OTHER: 'Other'
    };

    return {
      GetCommonAirportNames: function() {
        return COMMON_AIRPORT_NAMES;
      },

      IsOneOfCommonAirportNames(airportName, commonAirportNames) {
        const isOneOfCommonAirportNames = airportName === commonAirportNames.CVG
                                       || airportName === commonAirportNames.LEX;
        return isOneOfCommonAirportNames;
      },

      GetCommonAirportNameOrOther(airportName, commonAirportNames) {
        var commonAirportName = undefined;

        if (this.IsOneOfCommonAirportNames(airportName, commonAirportNames)){
          commonAirportName = airportName;
        } else if (airportName != null && airportName != ''){
          commonAirportName = 'Other';
        }
        return commonAirportName;
      },

      GetPreferredAirportName(person) {
        var preferredAirportName = undefined;

        const hasPreferredAirportAttribute = person.singleAttributes[preferredAirportAttributeTypeId] != null;

        if(hasPreferredAirportAttribute) {
          preferredAirportName = person.singleAttributes[preferredAirportAttributeTypeId].notes;
        }

        return preferredAirportName;
      },

      SetAttributeNotesAndStartDate(preferredAirportAttribute, preferredAirportName) {
        preferredAirportAttribute.notes = preferredAirportName;
        preferredAirportAttribute.selected = true;
        preferredAirportAttribute.startDate = new Date();

        return preferredAirportAttribute;
      },

      DoesPersonHaveValidPreferredAirportNameAttributeSet(person) {
        var idxOfPreferredAirportAttribute = person.attributeTypes[preferredAirportAttributeTypeId]
          .attributes.findIndex(attr => attr.attributeId === consts.ATTRIBUTE_IDS.PREFERRED_AIRPORT_NAME);

        var hasValidPreferredAirportAttribute = idxOfPreferredAirportAttribute !== -1;

        var preferredAirportNameNotes = person.attributeTypes[preferredAirportAttributeTypeId].attributes[idxOfPreferredAirportAttribute].notes;

        if(preferredAirportNameNotes == null) {
          hasValidPreferredAirportAttribute = false;
        }

        return hasValidPreferredAirportAttribute;
      },

      UpdateOrSetPreferredAirportAttributeType(person, preferredAirportAttributeType) {
        person.attributeTypes[preferredAirportAttributeTypeId] = preferredAirportAttributeType;
        if(person.singleAttributes && person.singleAttributes.hasOwnProperty(`${preferredAirportAttributeTypeId}`)){
          delete person.singleAttributes[preferredAirportAttributeTypeId];
        }

        return person;
      },

      SetUserCustomInputAirportNameIfNotOneOfCommonAirports(airportName, commonAirportNames) {
        let customInputAirportName = null;

        if (!this.IsOneOfCommonAirportNames(airportName, commonAirportNames)) {
          customInputAirportName = airportName;
        }

        return customInputAirportName;
      }

    };
  }
})();

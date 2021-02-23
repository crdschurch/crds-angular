export const groupParticipantListProperties = {
  title: "Group Participant List - properties",
  type: "array",
  items: {
    type: "object",
    properties: {
      congregation: {
        type: "string"
      },
      contactId: {
        type: "number"
      },
      displayName: {
        type: "string"
      },
      email: {
        type: "string"
      },
      groupParticipantId: {
        type: "number"
      },
      groupRoleId: {
        type: "number"
      },
      groupRoleTitle: {
        type: "string"
      },
      isApprovedLeader: {
        type: "boolean"
      },
      lastName: {
        type: "string"
      },
      nickName: {
        type: "string"
      },
      participantId: {
        type: "number"
      },
      singleAttributes: {
        type: ["object", "null"]
      },
      attributeTypes: {
        type: ["object", "null"]
      },
      startDate: {
        type: ["string", "null"]
      }
    }
  }
}

export const groupParticipantListContract = {
  title: "Group Participant List - contract",
  type: "array",
  items: {
    type: "object",
    required: ["congregation", "contactId", "displayName", "email", "groupParticipantId", "groupRoleId", "groupRoleTitle", "isApprovedLeader", "lastName", "nickName", "participantId", "singleAttributes", "attributeTypes", "startDate"]
  }
}
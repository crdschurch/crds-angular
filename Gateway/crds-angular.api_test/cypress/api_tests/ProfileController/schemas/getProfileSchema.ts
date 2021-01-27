// This schema verify a property has the correct type if it exists, it does not assert the property must exist
export const getProfilePropertiesSchema = {
  title: "Get Properties - property types",
  type: "object",
  properties: {
    addressId: {
      type: ["number", "null"]
    },
    addressLine1: {
      type: [ "string", "null" ]
    },
    addressLine2: {
      type: [ "string", "null" ]
    },
    age: {
      type: "number"
    },
    passportNumber: {
      type: [ "string", "null" ]
    },
    passportFirstname: {
      type: [ "string", "null" ]
    },
    passportLastname: {
      type: [ "string", "null" ]
    },
    passportMiddlename: {
      type: [ "string", "null" ]
    },
    passportExpiration: {
      type: [ "string", "null" ]
    },
    passportCountry: {
      type: [ "string", "null" ]
    },
    anniversaryDate: {
      type: [ "string", "null" ]
    },
    city: {
      type: [ "string", "null" ]
    },
    congregationId: {
      type: "number"
    },
    contactId: {
      type: "number"
    },
    dateOfBirth: {
      type: "string"
    },
    emailAddress: {
      type: "string"
    },
    employerName: {
      type: [ "string", "null" ]
    },
    firstName: {
      type: "string"
    },
    foreignCountry: {
      type: [ "string", "null" ]
    },
    genderId: {
      type:[ "number", "null" ]
    },
    homePhone: {
      type: [ "string", "null" ]
    },
    householdId: {
      type: "number"
    },
    householdName: {
      type: "string"
    },
    lastName: {
      type: "string"
    },
    maidenName: {
      type: [ "string", "null" ]
    },
    maritalStatusId: {
      type: [ "number", "null" ]
    },
    middleName: {
      type: [ "string", "null" ]
    },
    mobileCarrierId: {
      type: [ "number", "null" ]
    },
    mobilePhone: {
      type: [ "string", "null" ]
    },
    nickName: {
      type: "string"
    },
    newPassword: {
      type: [ "string", "null" ]
    },
    oldEmail: {
      type: [ "string", "null" ]
    },
    oldPassword: {
      type: [ "string", "null" ]
    },
    postalCode: {
      type: [ "string", "null" ]
    },
    county: {
      type: [ "string", "null" ]
    },
    state: {
      type: [ "string", "null" ]
    },
    participantStartDate: {
      type: "string"
    },
    attendanceStartDate: {
      type: [ "string", "null" ]
    },
    householdMembers: {
      type: "array",
      items: {
        title: "Household Member",
        type: "object",
        properties: {
          ContactId: { type: "number" },
          FirstName: { type: "string" },
          Nickname: { type: "string" },
          LastName: { type: "string" },
          DateOfBirth: { type: "string" },
          HouseholdPosition: { type: "string" },
          StatementTypeId: { type: "number" },
          DonorId: { type: "number" },
          Age: { type: "number" },
        }
      },
      minItems: 1
    },
    attributeTypes: {
      title: "Attribute Types",
      type: "object",
    },
    singleAttributes: {
      title: "Single Attributes",
      type: "object"
    }
  }
};

// This schema asserts that a property exists, it does not assert it is of the correct type
export const getProfileContract = {
  title: "Get Profile - contract",
  type: "object",
  required: ["contactId", "emailAddress", "firstName", "lastName", "householdId", "householdName", "householdMembers"],
};
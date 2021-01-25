export const createUserSchemaProperties = {
  title: "Create user schema properties",
  type: "object",
  properties:
  {
    firstname: {
      type: ["string", "null"]
    },
    lastname: {
      type: "string"
    },
    email: {
      type: "string"
    },
    password: {
      type: ["string", "null"]
    },
    contactId: {
      type: "number"
    }
  }
}

export const createUserContract = {
  title: "Create user contract - current functionality",
  type: "object",
  required: ["firstname", "lastname", "email", "password", "contactId"]
}

export const duplicateUserRegistrationPageContract = {
  title: "Angular (legacy) registration page expects a specific error message",
  type: "object",
  required: ["message"],
  properties: {
    message: {
      type: "string",
      pattern: "Duplicate User"
    }
  }
};

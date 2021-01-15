export const badRequestProperties = {
  title: "MP login bad request - property types",
  type: "object",
  properties: {
    message:{
      type: "string"
    },
    errors:{
      type: "array",
      minItems: 0,
      items: {
        type: "string"
      }
    }
  }
};

export const badRequestContract = {
  title: "MP login bad request - contract",
  type: "object",
  required: ["message"]
};
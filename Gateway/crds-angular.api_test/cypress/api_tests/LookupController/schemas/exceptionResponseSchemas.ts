export const exceptionResponseProperties = {
  title: "Exception response - property types",
  type: "object",
  properties: {
    ExceptionMessage:{
      type: "string"
    },
    ExceptionType:{
      type: "string",
    },
    Message: {
      type: "string"
    },
    StackTrace: {
      type: "string"
    }
  }
};

export const exceptionResponseContract = {
  title: "Exception response - contract",
  type: "object",
  required: ["Message"]
};
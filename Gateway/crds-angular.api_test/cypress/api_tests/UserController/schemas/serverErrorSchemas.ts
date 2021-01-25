export const genericServerErrorContract = {
  title: "Generic Server Error 500 response content",
  type: "object",
  required: ["Message"],
  properties: {
    Message: {
      type: "string",
      pattern: "An error has occurred."
    }
  }
};
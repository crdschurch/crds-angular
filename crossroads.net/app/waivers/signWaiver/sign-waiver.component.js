import SignWaiverController from './sign-waiver.controller';
import SignWaiverTemplate from './sign-waiver.html';

const SignWaivers = {
  bindings: {
    title: '<',
    eventName: '<'
  },
  template: SignWaiverTemplate,
  controller: SignWaiverController,
  controllerAs: 'ctrl'
};

export default SignWaivers;

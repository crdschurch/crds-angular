/**
 * CampCardController:
 * Passed in via the component directive:
 *    attendee
 *    startDate
 *    endDate
 *    paymentRemaining
 */
class CampCardController {
  constructor($state) {
    // this.stateParams = $stateParams;
    this.state = $state;
  }

  updateMedical() {
    let contactId = 6989102;
    this.state.go('campsignup.application', { page: 'medical-info', contactId, campId: 4525285 });
  }

  formatDate() {
    let startDateMoment = moment(this.startDate);
    let endDateMoment = moment(this.endDate);
    let monthDayStart = startDateMoment.format('MMMM Do');
    let monthDayEnd = endDateMoment.format('MMMM Do');
    let year = startDateMoment.format('YYYY');
    return `${monthDayStart} - ${monthDayEnd}, ${year}`;
  }
}

export default CampCardController;

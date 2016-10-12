/* ngInject */
class CamperInfoForm {
  constructor(CampsService, LookupService) {
    this.campsService = CampsService;
    this.lookupService = LookupService;

    this.formModel =  {
      firstName: null,
      lastName: null,
      middleName: null,
      preferredName: null,
      birthDate: null,
      gender: null,
      currentGrade: null,
      schoolAttending: null,
      schoolAttendingNext: null,
      crossroadsSite: null,
      roomate: null
    };
  }

  save(campId) {
    console.log(this.formModel);
    return this.campsService.campResource.save({campId}, this.formModel).$promise;
  }

  getModel() {
    return this.formModel;
  }

  getFields() {
    return [
      {
        className: 'row',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'firstName',
            type: 'crdsInput',
            templateOptions: {
              label: 'First Name',
              required: true
            }
          },
          {
            className: 'form-group col-xs-6',
            key: 'middleName',
            type: 'crdsInput',
            templateOptions: {
              label: 'Middle Name',
              required: false
            }
          }
        ]
      },
      {
        className: 'row',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'lastName',
            type: 'crdsInput',
            templateOptions: {
              label: 'Last Name',
              required: true
            }
          },
          {
            className: 'form-group col-xs-6',
            key: 'preferredName',
            type: 'crdsInput',
            templateOptions: {
              label: 'Preferred Name',
              required: false
            }
          }
        ]
      },
      {
        className: 'row',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'birthDate',
            type: 'crdsDatepicker',
            templateOptions: {
              label: 'Birth Date',
              required: true,
              type: 'text',
              datepickerPopup: 'MM/dd/yyyy'
            }
          },
          {
            className: 'form-group col-xs-6',
            key: 'gender',
            type: 'crdsRadio',
            templateOptions: {
              label: 'Gender',
              required: true,
              inline: true,
              valueProp: 'dp_RecordID',
              labelProp: 'dp_RecordName',
              options: []
            },
            controller: /* @ngInject */ ($scope, LookupService) => {
              $scope.to.loading = LookupService.Genders.query().$promise.then((response) => {
                $scope.to.options = response;
                return response;
              }).catch((err) =>  console.error(err));
            }
          }
        ]
      },
      {
        className: 'row',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'currentGrade',
            type: 'crdsSelect',
            templateOptions: {
              label: 'Current Grade',
              required: true,
              options: [
                { grade: '5th'},
                { grade: '6th'},
                { grade: '7th'},
                { grade: '8th'},
                { grade: '9th'},
                { grade: '10th'},
                { grade: '11th'},
                { grade: '12th'}
              ],
              valueProp: 'grade',
              labelProp: 'grade'
            }
          },
          {
            className: 'form-group col-xs-6',
            key: 'schoolAttending',
            type: 'crdsInput',
            templateOptions: {
              label: 'School Currently Attending ',
              required: true
            }
          }
        ]
      },
      {
        className: 'row',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'schoolAttendingNext',
            type: 'crdsInput',
            templateOptions: {
              label: 'School Attending Next Year',
              required: true
            }
          }
        ]
      },
      {
        className: 'row',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'crossroadsSite',
            type: 'crdsSelect',
            templateOptions: {
              label: 'What site do you regularly attend service?',
              required: true,
              valueProp: 'dp_RecordID',
              labelProp: 'dp_RecordName',
              options: []
            },
            controller: /* @ngInject */ ($scope, LookupService) => {
              $scope.to.loading = LookupService.Sites.query().$promise.then((response) => {
                $scope.to.options = response;
                return response;
              });
            }
          },
          {
            className: 'form-group col-xs-6',
            key: 'roomate',
            type: 'crdsInput',
            templateOptions: {
              label: 'Prefered Roommate First and Last Name',
              required: false
            }
          }
        ]
      }
    ];
  }
}
export default CamperInfoForm;

// eslint-disable-next-line import/no-extraneous-dependencies, import/no-unresolved, import/extensions
import crdsConstants from 'crds-constants';
import find from 'lodash/collection/find';

class CamperInfoFormFactory {
  /* ngInject */
  constructor(CampsService, LookupService) {
    this.campsService = CampsService;
    this.lookupService = LookupService;
  }

  createForm() {
    /* eslint-disable no-use-before-define */
    return new CamperInfoForm(this.campsService, this.lookupService);
    /* eslint-enable */
  }
}

export default CamperInfoFormFactory;

class CamperInfoForm {
  constructor(CampsService, LookupService) {
    this.campsService = CampsService;
    this.lookupService = LookupService;

    this.initFormModel();
  }

  initFormModel() {
    this.formModel = {
      contactId: this.campsService.camperInfo.contactId || undefined,
      firstName: this.campsService.camperInfo.firstName || undefined,
      lastName: this.campsService.camperInfo.lastName || undefined,
      middleName: this.campsService.camperInfo.middleName || undefined,
      preferredName: this.campsService.camperInfo.preferredName || undefined,
      mobilePhone: this.campsService.camperInfo.mobilePhone || undefined,
      birthDate: this.campsService.camperInfo.birthDate || undefined,
      gender: this.campsService.camperInfo.gender || undefined,
      currentGrade: this.campsService.camperInfo.currentGrade || undefined,
      schoolAttending: this.campsService.camperInfo.schoolAttending || undefined,
      schoolAttendingNext: this.campsService.camperInfo.schoolAttendingNext || undefined,
      crossroadsSite: this.campsService.camperInfo.crossroadsSite || undefined,
      attributeTypes: this.campsService.camperInfo.attributeTypes || {},
      singleAttributes: this.campsService.camperInfo.singleAttributes || {},
      roommate: this.campsService.camperInfo.roommate || undefined
    };

    // In order to default the T Shirt select, set the shirt size in the form model from the
    // Shirt Size single attribute that is set, if any
    const shirtSizeAttribute = this.formModel.singleAttributes[crdsConstants.ATTRIBUTE_TYPE_IDS.TSHIRT_SIZES];
    if (shirtSizeAttribute && shirtSizeAttribute.attribute) {
      this.formModel.shirtSize = shirtSizeAttribute.attribute.attributeId;
    }
  }

  confirmSite(resp) {
    const match = _.find(resp, site => this.formModel.crossroadsSite === site.dp_RecordID);

    if (!match) {
      this.formModel.crossroadsSite = undefined;
    }
  }

  save(campId) {
    // Find the actual single attribute shirt size from the forml selected attribute id
    const selected = find(this.campsService.shirtSizes, each => each.attributeId === this.formModel.shirtSize);

    // Set the shirt size single attribute on the DTO to be submitted to the API
    this.formModel.singleAttributes[crdsConstants.ATTRIBUTE_TYPE_IDS.TSHIRT_SIZES] = {
      attribute: selected,
      notes: null,
      description: null
    };

    return this.campsService.camperResource.save({ campId }, this.formModel).$promise;
  }

  getModel() {
    return this.formModel;
  }

  // eslint-disable-next-line class-methods-use-this
  getFields() {
    return [
      {
        className: '',
        wrapper: 'campBootstrapRow',
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
            key: 'lastName',
            type: 'crdsInput',
            templateOptions: {
              label: 'Last Name',
              required: true
            }
          }
        ]
      },
      {
        className: '',
        wrapper: 'campBootstrapRow',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'preferredName',
            type: 'crdsInput',
            templateOptions: {
              label: 'Preferred Name',
              required: false
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
              labelProp: 'dp_RecordName',
              valueProp: 'dp_RecordID',
              options: []
            },
            controller: /* @ngInject */ ($scope, LookupService) => {
              /* eslint-disable no-param-reassign */
              $scope.to.loading = LookupService.Genders.query().$promise.then((response) => {
                $scope.to.options = response;
                return response;
              }).catch(err => console.error(err));
              /* eslint-enable */
            }
          }
        ]
      },
      {
        className: '',
        wrapper: 'campBootstrapRow',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'birthDate',
            type: 'crdsDatepicker',
            templateOptions: {
              label: 'Birth Date (mm/dd/yyyy)',
              required: true,
              type: 'text',
              datepickerPopup: 'MM/dd/yyyy'
            }
          },
          {
            className: 'form-group col-xs-6',
            key: 'currentGrade',
            type: 'crdsSelect',
            templateOptions: {
              label: 'Current Grade',
              required: true,
              valueProp: 'groupId',
              labelProp: 'groupName',
              options: []
            },
            controller: /* @ngInject */ ($scope, CampsService) => {
              /* eslint-disable no-param-reassign */
              $scope.to.options = CampsService.campInfo.eligibleGrades;
              /* eslint-enable */
            }
          }
        ]
      },
      {
        className: '',
        wrapper: 'campBootstrapRow',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'schoolAttending',
            type: 'crdsInput',
            templateOptions: {
              label: 'School Currently Attending ',
              required: true
            }
          },
          {
            className: 'form-group col-xs-6',
            key: 'schoolAttendingNext',
            type: 'crdsInput',
            templateOptions: {
              label: 'School Attending Next School Year',
              required: true
            }
          }
        ]
      },
      {
        className: '',
        wrapper: 'campBootstrapRow',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'crossroadsSite',
            type: 'crdsSelect',
            templateOptions: {
              label: 'Usually Attend Services at',
              required: true,
              valueProp: 'dp_RecordID',
              labelProp: 'dp_RecordName',
              options: []
            },
            controller: /* @ngInject */ ($scope, LookupService) => {
              /* eslint-disable no-param-reassign */
              $scope.to.loading = LookupService.Sites.query().$promise.then((response) => {
                $scope.to.options = response;
                this.confirmSite(response);
                return response;
              });
              /* eslint-enable */
            }
          },
          {
            className: 'form-group col-xs-6',
            key: 'roommate',
            type: 'crdsInput',
            templateOptions: {
              label: 'Preferred Small Group Leader or Friend',
              required: false
            }
          }
        ]
      },
      {
        className: '',
        wrapper: 'campBootstrapRow',
        fieldGroup: [
          {
            className: 'form-group col-xs-6',
            key: 'mobilePhone',
            type: 'crdsPhoneNumberInput',
            optionsTypes: ['phoneNumber'],
            templateOptions: {
              label: 'Student Mobile Number',
              required: false,
              helpBlock: 'By providing your mobile number, you are agreeing to receive text message updates from Crossroads.'
            }
          },
          {
            className: 'form-group col-xs-6',
            key: 'shirtSize',
            type: 'crdsSelect',
            templateOptions: {
              label: 'T-Shirt Size',
              required: true,
              valueProp: 'attributeId',
              labelProp: 'name',
              options: []
            },
            controller: /* @ngInject */ ($scope, CampsService) => {
              // eslint-disable-next-line no-param-reassign
              $scope.to.options = CampsService.shirtSizes;
            }
          }
        ]
      }
    ];
  }
}

(function() {
    'use strict';
    module.exports = formlyBuilderRun;

    require('./templates/datepicker.html');

    formlyBuilderRun.$inject = ['formlyConfig'];

    function formlyBuilderRun(formlyConfig) {
        var attributes = [
            'date-disabled',
            'custom-class',
            'show-weeks',
            'starting-day',
            'init-date',
            'min-mode',
            'max-mode',
            'format-day',
            'format-month',
            'format-year',
            'format-day-header',
            'format-day-title',
            'format-month-title',
            'year-range',
            'shortcut-propagation',
            'datepicker-popup',
            'show-button-bar',
            'current-text',
            'clear-text',
            'close-text',
            'close-on-date-selection',
            'datepicker-append-to-body'
        ];

        var bindings = [
            'datepicker-mode',
            'min-date',
            'max-date'
        ];

        var ngModelAttrs = {};

        angular.forEach(attributes, function(attr) {
            ngModelAttrs[camelize(attr)] = { attribute: attr };
        });

        angular.forEach(bindings, function(binding) {
            ngModelAttrs[camelize(binding)] = { bound: binding };
        });

        formlyConfig.setType({
            name: 'datepicker',
            templateUrl: 'templates/datepicker.html',
            wrapper: ['bootstrapLabel', 'bootstrapHasError'],
            defaultOptions: {
                ngModelAttrs: ngModelAttrs,
                templateOptions: {
                    datepickerOptions: {
                        format: 'MM/dd/yyyy',
                        initDate: new Date()
                    }
                }
            },
            controller: ['$scope', function($scope) {
                $scope.datepicker = {};

                $scope.datepicker.opened = false;

                $scope.datepicker.open = function($event) {
                    $scope.datepicker.opened = !$scope.datepicker.opened;
                };
            }]
        });

        ngModelAttrs = {};

        // attributes
        angular.forEach([
            'meridians',
            'readonly-input',
            'mousewheel',
            'arrowkeys'
        ], function(attr) {
            ngModelAttrs[camelize(attr)] = { attribute: attr };
        });

        // bindings
        angular.forEach([
            'hour-step',
            'minute-step',
            'show-meridian'
        ], function(binding) {
            ngModelAttrs[camelize(binding)] = { bound: binding };
        });

        formlyConfig.setType({
            name: 'timepicker',
            template: '<timepicker ng-model="model[options.key]"></timepicker>',
            wrapper: ['bootstrapLabel', 'bootstrapHasError'],
            defaultOptions: {
                ngModelAttrs: ngModelAttrs,
                templateOptions: {
                    datepickerOptions: {}
                }
            }
        });

        formlyConfig.setType({
            name: 'boldcheckbox',
            template: require('./templates/boldCheckbox.html'),
            wrapper: ['bootstrapHasError'],
            apiCheck: check => ({
                templateOptions: {
                    label: check.string
                }
            })
        });

        function camelize(string) {
            string = string.replace(/[\-_\s]+(.)?/g, function(match, chr) {
                return chr ? chr.toUpperCase() : '';
            });
            // Ensure 1st char is always lowercase
            return string.replace(/^([A-Z])/, function(match, chr) {
                return chr ? chr.toLowerCase() : '';
            });
        }
    };
})();

export default class PlaygroundService {
    PlaygroundService(fbMapperConfig, $log) {
        this.fbMapperConfig = fbMapperConfig;
        this.log = $log;
    }

    getFields() {
        return [
            {
                formlyConfig: {
                    key: 'person.firstName',
                    type: 'formlyBuilderInput',
                    templateOptions: {
                        label: 'First Name',
                        required: true
                    }
                },
                prePopulate: false
            }, {
                formlyConfig: {
                    key: 'person.lastName',
                    type: 'formlyBuilderInput',
                    templateOptions: {
                        label: 'Last Name',
                        required: true
                    }
                }
            }, {
                formlyConfig: {
                    key: 'person.nickName',
                    type: 'formlyBuilderInput',
                    templateOptions: {
                        label: 'Preferred Name',
                        required: true
                    }
                }
            }
        ]
    }
}
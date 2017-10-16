
import constants from 'crds-constants';
import StreamController from '../../../app/live_stream/stream/stream.controller';
import StreamspotService from '../../../app/live_stream/services/streamspot.service';
import GeolocationService from '../../../app/live_stream/services/geolocation.service';
import CMSService from '../../../core/services/CMS.service';

describe('Stream Controller', () => {
  let fixture,
      rootScope,
      cmsService,
      location,
      streamspotService,
      geolocationService,
      httpBackend,
      modal,
      results,
      sce,
      timeout,
      document;

  beforeEach(angular.mock.module(constants.MODULES.LIVE_STREAM));

  beforeEach(inject(function ($injector) {
    cmsService = $injector.get('CMSService');
    streamspotService = $injector.get('StreamspotService');
    geolocationService = $injector.get('GeolocationService');
    rootScope = $injector.get('$rootScope');
    modal = $injector.get('$modal');
    location = $injector.get('$location');
    timeout = $injector.get('$timeout');
    sce = $injector.get('$sce');
    document = $injector.get('$document');

    fixture = new StreamController(cmsService, streamspotService, geolocationService, rootScope, modal, location, timeout, sce, document);
    results = [
      {
        "id": 3,
        "title": "Events",
        "created": "2017-05-25T15:33:04+02:00",
        "className": "Section"
      },
      {
        "id": 2,
        "title": "be the church",
        "features": [
          {
            "id": 2,
            "title": "btc1",
            "description": "<p>Feature1</p>",
            "status": null,
            "link": null,
            "version": "6",
            "sections": [
              2
            ],
            "created": "2017-06-01T17:34:37+02:00",
            "className": "Feature"
          },
          {
            "id": 3,
            "title": "both",
            "description": "<p>Feature1</p>",
            "status": null,
            "link": null,
            "version": "6",
            "sections": [
              1,
              2
            ],
            "created": "2017-06-01T17:34:37+02:00",
            "className": "Feature"
          }
        ],
        "manyManyExtraFields": {
          "features": {
            "2": {
              "sortOrder": "1"
            }
          }
        },
        "created": "2017-05-30T20:13:46+02:00",
        "className": "Section"
      },
      {
        "id": 1,
        "title": "don't miss",
        "features": [
          {
            "id": 1,
            "title": "dm1",
            "description": "<p>he he he</p>",
            "status": null,
            "link": null,
            "version": "3",
            "image": {
              "id": 19,
              "name": "animal-wizard-cow.jpg",
              "title": "animal wizard cow",
              "filename": "https://s3.amazonaws.com/crds-cms-uploads/program/stills/animal-wizard-cow.jpg",
              "content": null,
              "showInSearch": "1",
              "cloudStatus": "Live",
              "cloudSize": "175392",
              "cloudMetaJson": "{\"Dimensions\":\"660x655\",\"LastPut\":1477082040}",
              "parent": 15,
              "owner": 1,
              "derivedImages": [
                14,
                15,
                16,
                17
              ],
              "created": "2016-10-21T22:33:59+02:00",
              "className": "CloudImage"
            },
            "sections": [
              1
            ],
            "created": "2016-08-05T03:29:11+02:00",
            "className": "Feature"
          },
          {
            "id": 5,
            "title": "dm2",
            "description": "<p>4</p>",
            "status": null,
            "link": null,
            "version": "6",
            "sections": [
              1
            ],
            "created": "2017-06-01T18:08:34+02:00",
            "className": "Feature"
          },
          {
            "id": 3,
            "title": "both",
            "description": "<p>Feature1</p>",
            "status": null,
            "link": null,
            "version": "6",
            "sections": [
              1,
              2
            ],
            "created": "2017-06-01T17:34:37+02:00",
            "className": "Feature"
          }
        ],
        "manyManyExtraFields": {
          "features": {
            "1": {
              "sortOrder": "1"
            },
            "2": {
              "sortOrder": "4"
            },
            "5": {
              "sortOrder": "3"
            }
          }
        },
        "created": "2017-05-30T20:14:16+02:00",
        "className": "Section"
      }
    ];
  }));

  it('process Digital Program in Dont Miss and Be The Church', () => {
    fixture.sortDigitalProgram(results);
    expect(fixture.dontMiss.length).toBe(3);
    expect(_.first(fixture.dontMiss).title).toBe('dm1');

    expect(fixture.beTheChurch.length).toBe(2);
    expect(_.first(fixture.beTheChurch).title).toBe('btc1');
  });

})

/* global __CRDS_ENV__ */
(function () {
  module.exports = {
    // TODO: Should this be moved to core?
    // MODULE NAMES
    MODULES: {
      CAMPS: 'crossroads.camps',
      CAMPS_APPLICATION_PAGE: 'crossroads.camps.applicationPages',
      CHILDCARE: 'crossroads.childcare',
      CHILDCARE_DASHBOARD: 'crossroads.childcare_dashboard',
      CORE: 'crossroads.core',
      COMMON: 'crossroads.common',
      COMMUNITY_GROUPS: 'crossroads.community',
      CORKBOARD: 'crossroads.corkboard',
      CROSSROADS: 'crossroads',
      FORM_BUILDER: 'crossroads.form_builder',
      FORMLY_BUILDER: 'crossroads.formly_builder',
      GO_VOLUNTEER: 'crossroads.go_volunteer',
      MEDIA: 'crossroads.media',
      LIVE_STREAM: 'crossroads.live_stream',
      GROUP_TOOL: 'crossroads.grouptool',
      MPTOOLS: 'crossroads.mptools',
      ONETIME_SIGNUP: 'crossroads.onetime',
      PROFILE: 'crossroads.profile',
      SEARCH: 'crossroads.search',
      SIGNUP: 'crossroads.signup',
      TRIPS: 'crossroads.trips',
      INVOICES: 'crossroads.invoices',
      WAIVERS: 'crossroads.waivers'
    },
    ATTRIBUTE_TYPE_IDS: {
      ABUSE_HISTORY: 69,
      ALLERGIES: 67,
      COFACILITATOR: 87,
      COPARTICIPANT: 88,
      DIETARY_RESTRICTIONS: 65,
      ETHNICITY: 20,
      EXPERIENCE_ABROAD: 68,
      FREQUENT_FLYERS: 63,
      GROUP_AGE_RANGE: 91,
      GROUP_TYPE: 73,
      INTERNATIONAL_EXPERIENCE: 66,
      MEDICAL_RESTRICTIONS: 100,
      MEDICATION_TAKING: 101,
      PERSONAL: 1,
      SCRUB_TOP_SIZES: 22,
      SCRUB_BOTTOM_SIZES: 23,
      SKILLS: 24,
      SPIRITUAL_JOURNEY: 60,
      START_ATTEND_REASON: 59,
      TRIP_SKILLS: 61,
      TSHIRT_SIZES: 21,
      TRIP_EXPERIENCE: 62,
      UNDIVIDED_FACILITATOR_TRAINING: 85,
      UNDIVIDED_RSVP_KICKOFF: 86
    },
    ATTRIBUTE_IDS: {
      ALL_ALLERGIES: 3971,
      COFACILITATOR: 7086,
      COPARTICIPANT: 7087,
      DELTA_FREQUENT_FLYER: 3958,
      EXPERIENCE_ABROAD: 3972,
      HIGHSCHOOLAGE: 7090,
      MEDICAL_RESTRICTIONS: 9001,
      MEDICATION_TAKING: 9000,
      MIDDLESCHOOLAGE: 7089,
      OBEDIENCE: 3935,
      PREVIOUS_TRIP_EXPERIENCE: 3949,
      RECEIVED_JESUS: 3934,
      REPLICATING: 3936,
      SEARCHING_FOR_ANSWERS: 3933,
      SOUTHAFRICA_FREQUENT_FLYER: 3959,
      START_ATTEND_REASON: 59,
      US_FREQUENT_FLYER: 3980,
      VICTIM_OF_ABUSE: 3973
    },
    NON_CROSSROADS_LOCATIONS: {
      I_DO_NOT_ATTEND_CROSSROADS: 2,
      NOT_SITE_SPECIFIC: 5
    },
    SERVING_RESPONSES: {
      NOT_AVAILABLE: 2,
      AVAILABLE: 1
    },
    SERVING: {
      MAXSTUDENTVOLUNTEERAGE: 17,
      STUDENTVOLUNTEERTEXT: ' (SV)'
    },
    CMS: {
      PAGENAMES: {
        COMMUNITYGROUPS: 'CommunityGroupSignupPage',
        ONETIMEEVENTS: 'OnetimeEventSignupPage'
      },
      FORM_BUILDER: {
        CLASS_NAME: {
          GROUP_PARTICIPANT_FIELD: 'GroupParticipantField',
          PROFILE_FILED: 'ProfileField',
        },
        FIELD_NAME: {
          COFACILITATOR: 'CoFacilitator',
          COPARTICIPANT: 'CoParticipant',
        },
      }
    },
    GROUP: {
      ATTRIBUTE_TYPE_ID: 90,
      AGE_RANGE_ATTRIBUTE_TYPE_ID: 91,
      GROUP_TYPE_ATTRIBUTE_TYPE_ID: 73,
      GROUP_TYPE_ATTRIBUTE_ANYONE: 7007,
      GROUP_TYPE_ATTRIBUTE_MENONLY: 7008,
      GROUP_TYPE_ATTRIBUTE_WOMENONLY: 7009,
      GROUP_TYPE_ATTRIBUTE_COUPLES: 7010,
      MAX_LEADERS: 5,
      MAX_APPRENTICE: 5,
      GROUP_TYPE_ID: {
        UNDIVIDED: 26,
        SMALL_GROUPS: 1,
        MY_SERVE: 9,
        ONSITE_GROUPS: 8
      },
      ROLES: {
        MEMBER: 16,
        LEADER: 22,
        APPRENTICE: 66
      },
      SEARCH_FILTERS_QUERY_PARAM_NAMES: {
        AGE: 'age',
        GROUP_CATEGORY: 'category',
        GROUP_TYPE: 'type',
        KIDS_WELCOME: 'kids',
        GROUP_LOCATION: 'grouplocation',
        MEETING_DAY: 'day',
        MEETING_TIME: 'time',
        MEETING_FREQUENCY: 'frequency',
        LEADER_SITE: 'site'
      },
      EMAIL: {
        COMPOSE_EMAIL_NAME: 'Compose Email',
        COMPOSE_EMAIL_DESCRIPTION_LINE1: 'Opens the email application',
        COMPOSE_EMAIL_DESCRIPTION_LINE2: 'on your computer/phone.',
        COMPOSE_EMAIL_ICON: 'mail5',
        COPY_EMAIL_NAME: 'Copy Addresses',
        COPY_EMAIL_DESCRIPTION_LINE1: 'Copies participant email',
        COPY_EMAIL_DESCRIPTION_LINE2: 'addresses to your clipboard.',
        COPY_EMAIL_ICON: 'file-text-o'
      }
    },
    GEOLOCATION: {
      MODAL_TIMEOUT: 3000,
      FORMS_KEY: '1rupjr7gvqUU203fwjmeUlIiVwCA8BdkD-mP6M6s3wxQ'
    },
    INVITATION: {
      TYPES: {
        GROUP: 1,
        TRIP: 2
      }
    },
    MINISTRY: {
      SPIRITUAL_GROWTH: '8'
    },
    STREAM_STATUS: {
      LIVE: 'Live',
      UPCOMING: 'Upcoming',
      OFF: 'Off'
    },
    PRE_STREAM_HOURS: 15,
    COOKIES: {
      SESSION_ID: `${__CRDS_ENV__}sessionId`,
      REFRESH_TOKEN: `${__CRDS_ENV__}refreshToken`,
      IMPERSONATION_ID: `${__CRDS_ENV__}impersonateUserId`
    },
    EVENTS: {
      ROOM_AVAILABLE: null,
      ROOM_PENDING: 0,
      ROOM_BOOKED: 1,
      EVENT_TYPES: {
        CHILDCARE: 243
      }
    },
    MICROCLIENTS: {
      GROUP_LEADER_FORM: {
        URL: '/group-leader'
      }
    }
  };
}());

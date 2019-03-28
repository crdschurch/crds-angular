function addTrailingSlashIfNecessary(link) {
  if (link === '') {
    return link;
  }

  if (_.endsWith(link, '/') === false) {
    return `${link}/`;
  }

  return link;
}

function buildLink(city, org, state, stateParams) {
  let base = `/go-volunteer/${addTrailingSlashIfNecessary(city)}`;
  if (state.next.name === 'go-volunteer.city.organizations') {
    return `${base}organizations/`;
  }

  if (org) {
    return base + addTrailingSlashIfNecessary(org);
  }

  if (state.next.name === 'go-volunteer.cms') {
    return base + addTrailingSlashIfNecessary(stateParams.cmsPage);
  }

  if (state.next.name === 'go-volunteer.page' || state.next.name === 'go-volunteer.crossroadspage') {
    let organization = stateParams.organization || 'crossroads';
    organization = (organization === 'other') ? 'crossroads' : organization;
    base = `${base}organizations/${organization}/${addTrailingSlashIfNecessary(stateParams.page)}`;
  }

  return base;
}

export function CmsInfo(SignUpForm, $state, $stateParams, GoVolunteerService, $q, $window) {
  const goService = GoVolunteerService;
  const city = $stateParams.city || 'cincinnati';
  $window.sessionStorage.setItem('go-volunteer.city', city);
  const organization = $stateParams.organizations || undefined;
  const link = buildLink(city, organization, $state, $stateParams);
  const deferred = $q.defer();
  const page = SignUpForm.get({ url: link });
  page.then((data) => {
    goService.cmsInfo = data;
    deferred.resolve();
  }, () => {
    deferred.reject();
  });

  return deferred.promise;
}

export function Meta($state, $stateParams) {
  const state = $state;
  const city = $stateParams.city || 'cincinnati';
  state.next.data.meta.title = `GO ${city}`;
}

export function GetCities($log, $state, GoVolunteerDataService, GoVolunteerService, $q) {
  const gService = GoVolunteerService;
  const initiativeId = $state.toParams.initiativeId;
  const deferred = $q.defer();
  GoVolunteerDataService.getInitiativeCities(initiativeId).then((data) => {
    gService.cities = data;
    deferred.resolve();
  }, (err) => {
    // we still want to go to the page even if theres an error
    $log.error(err);
    deferred.resolve();
  });
  return deferred.promise;
}

export function GetProject($state, GoVolunteerDataService, GoVolunteerService, $q) {
  const gService = GoVolunteerService;
  const projectId = $state.toParams.projectId;
  const deferred = $q.defer();
  GoVolunteerDataService.getProject(projectId).then((data) => {
    gService.project = data;
    deferred.resolve();
  }, () => {
    deferred.reject();
  });
  return deferred.promise;
}

export function GetDashboard($state, GoVolunteerDataService, GoVolunteerService, $q, $log) {
  const deferred = $q.defer();
  const projectId = $state.toParams.projectId;
  const gService = GoVolunteerService;
  GoVolunteerDataService.getDashboard(projectId).then((data) => {
    gService.dashboard = data;
    deferred.resolve();
  }, (err) => {
    $log.error('Unable to get the dashboard data', err);
    deferred.reject();
  });
  return deferred.promise;
}

export function GetProfile(Profile, $cookies, $q, GoVolunteerService) {
  const deferred = $q.defer();
  const gService = GoVolunteerService;
  const cid = $cookies.get('userId');

  if (GoVolunteerService.person.nickName === '') {
    const promise = Profile.Person.get({ contactId: cid }).$promise;
    promise.then((person) => {
      gService.person = person;
      deferred.resolve(person);
    }).catch(deferred.reject);
  } else {
    deferred.resolve();
  }

  return deferred.promise;
}

export function GetOrganizations(Organizations, GoVolunteerService, $q, $log) {
  const gService = GoVolunteerService;
  const deferred = $q.defer();
  Organizations.getCurrentOrgs().then((data) => {
    gService.organizations = data;
    deferred.resolve();
  }, (err) => {
    $log.error('Unable to get organizations', err);
    deferred.reject();
  });

  return deferred.promise;
}

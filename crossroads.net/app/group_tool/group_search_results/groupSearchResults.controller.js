
export default class GroupSearchResultsController {
  /*@ngInject*/
  constructor(NgTableParams, GroupService, $state) {
    this.groupService = GroupService;

    this.search = null;
    this.processing = false;
    this.state = $state;
    this.ready = false;
    this.results = [];

    this.showLocationInput = false;
    this.searchedWithLocation = false;

    this.tableParams = new NgTableParams({}, { dataset: this.results });
  }

  $onInit() {
    this.search = {
      query: this.state.params.query,
      location: this.state.params.location
    };
    this.doSearch(this.state.params.query, this.state.params.location);
  }

  doSearch(query, location) {
    this.showLocationInput = false;
    this.searchedWithLocation = location && location.length > 0;
    this.ready = false;
    this.results = [];
    this.groupService.search(query, location).then(
      (data) => {
        this.results = data;
      },
      (err) => {
        this.results = [];
      }
    ).finally(
      () => {
        this.ready = true;
      }
    );
  }

  submit() {
    this.doSearch(this.search.query, this.search.location);
  }

  searchWithLocation() {
    this.doSearch(this.search.query, this.search.location);
  }

  openMap(group) {
    console.log('Open Map');
  }
}
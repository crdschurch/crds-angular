
import GroupResource from './groupResource';

export default class GroupResourceCategory {
  constructor(jsonObject) {
    if (jsonObject) {
      this._assignProperties(jsonObject);
    } else {
      this._assignProperties({});
    }
  }

  _assignProperties(source) {
      this.title = source.title;
      this.description = source.description;
      this.footerContent = source.communitygroupcontent;
      this.sortOrder = source.sortOrder;
      this.active = source.active;
      if(source.resources) {
        this.resources = source.resources.map((r) => {
          return new GroupResource(r);
        });
      } else {
        this.resources = [];
      }
  }

  getTitle() {
    return this.title;
  }

  getDescription() {
    return this.description;
  }

  getFooterContent() {
    return this.footerContent;
  }

  hasFooterContent() {
    return this.footerContent !== undefined && this.footerContent !== null && this.footerContent.length > 0;
  }
  
  getSortOrder() {
    return this.sortOrder;
  }

  isActive() {
    return this.active === true;
  }

  setActive(a) {
    this.active = a;
  }

  getResources() {
    return this.resources;
  }

  compareTo(other) {
    if(this.getSortOrder() === undefined && other.getSortOrder() === undefined) {
      return 0;
    }

    if(this.getSortOrder() === undefined) {
      return -1;
    }

    if(other.getSortOrder() === undefined) {
      return 1;
    }

    let compare = this.getSortOrder() - other.getSortOrder();
    return compare > 0 ? 1 : compare < 0 ? -1 : 0; 
  }
}
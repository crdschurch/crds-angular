//using System.Web.Mvc;
//using AuthorizeAttribute = System.Web.Http.AuthorizeAttribute;
//using System.Web.Mvc;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace crds_angular.Security
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        //Entities context = new Entities(); // my entity  
        //private readonly string[] allowedroles;

        //public CustomAuthorize(params string[] roles)
        //{
        //    this.allowedroles = roles;
        //}

        //public override void OnAuthorization(AuthorizationContext filterContext)
        //{
        //    base.OnAuthorization(filterContext);
        //}

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            return base.IsAuthorized(actionContext);
        }

        //protected override bool AuthorizeCore(HttpContextBase httpContext)
        //{
        //    bool disableAuthentication = false;

        //    #if DEBUG
        //    disableAuthentication = true;
        //    #endif

        //    if (disableAuthentication)
        //        return true;

        //    return base.
        //    return base.AuthorizeCore(httpContext);
        //}

        //protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        //{
        //    filterContext.Result = new HttpUnauthorizedResult();
        //}
    }
}
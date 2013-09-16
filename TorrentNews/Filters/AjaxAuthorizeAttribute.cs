namespace TorrentNews.Filters
{
    using System.Web.Mvc;

    public class AjaxAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext context)
        {
            if (context.HttpContext.Request.IsAjaxRequest())
            {
                var urlHelper = new UrlHelper(context.RequestContext);
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new JsonResult
                {
                    Data = new
                    {
                        Error = "NotAuthorized",
                        LogOnUrl = urlHelper.Action("Login", "Account"),
                        RedirectUrl = context.HttpContext.Request.RawUrl
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                base.HandleUnauthorizedRequest(context);
            }
        }
    }
}
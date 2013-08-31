using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TorrentNews.Filters
{
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Filters;

    public class SecretRequiredAttribute : ActionFilterAttribute
    {
        private readonly string storedSecret;

        public SecretRequiredAttribute()
        {
            this.storedSecret = ConfigurationManager.AppSettings["secret"];
        }

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (!actionContext.ActionArguments.ContainsKey("secret") ||
                !this.storedSecret.Equals(actionContext.ActionArguments["secret"]))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, "Unauthorized access");
            }

            base.OnActionExecuting(actionContext);
        }
    }
}
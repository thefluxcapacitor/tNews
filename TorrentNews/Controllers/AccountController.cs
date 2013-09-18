namespace TorrentNews.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using DotNetOpenAuth.AspNet;

    using Microsoft.Web.WebPages.OAuth;

    using TorrentNews.Dal;
    using TorrentNews.Models;

    using WebMatrix.WebData;
    using TorrentNews.Domain;

    [Authorize]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            this.ViewBag.HideNavigationLinks = true;
            this.ViewBag.HideLoginSection = true;

            this.ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return this.RedirectToAction("Index", "Torrents");
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, this.Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            this.ViewBag.HideNavigationLinks = true;
            this.ViewBag.HideLoginSection = true;

            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(this.Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return this.RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return this.RedirectToLocal(returnUrl);
            }

            if (this.User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, this.User.Identity.Name);
                return this.RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                this.ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                this.ViewBag.ReturnUrl = returnUrl;
                var model = new RegisterExternalLoginModel
                                {
                                    UserName = result.UserName,
                                    ExternalLoginData = loginData,
                                    FB_name = result.ExtraData.ContainsKey("name") ? result.ExtraData["name"] : string.Empty,
                                    FB_link = result.ExtraData.ContainsKey("link") ? result.ExtraData["link"] : string.Empty,
                                    GL_email = result.ExtraData.ContainsKey("email") ? result.ExtraData["email"] : string.Empty
                                };
                return View("ExternalLoginConfirmation", model);
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (this.User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                throw new Exception("Error registering user");
            }

            if (this.ModelState.IsValid)
            {
                var repo = new UsersRepository();
                var user = repo.FindByUsername(model.UserName);

                // Check if user already exists
                if (user == null)
                {
                    user = new User();
                    user.Username = model.UserName;
                    user.Provider = provider;
                    user.ProviderUserId = providerUserId;
                    user.Id = Domain.User.GetFabricatedId(provider, providerUserId);
                    user.FB_link = model.FB_link;
                    user.FB_name = model.FB_name;
                    user.GL_email = model.GL_email;
                    repo.Save(user);

                    OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                    OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);
                    return this.RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("Username", "User name already exists. Please enter a different user name.");
                }
            }

            this.ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            this.ViewBag.ReturnUrl = returnUrl;
            return this.View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            this.ViewBag.HideNavigationLinks = true;
            this.ViewBag.HideLoginSection = true;

            return this.View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;
            return this.PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else
            {
                return this.RedirectToAction("Index", "Torrents");
            }
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                this.Provider = provider;
                this.ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }

            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(this.Provider, this.ReturnUrl);
            }
        }
        #endregion
    }
}

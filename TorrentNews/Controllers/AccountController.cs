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

    [Authorize]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        ////
        //// POST: /Account/Login

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult Login(LoginModel model, string returnUrl)
        //{
        //    if (this.ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
        //    {
        //        return this.RedirectToLocal(returnUrl);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    this.ModelState.AddModelError("", "The user name or password provided is incorrect.");
        //    return View(model);
        //}

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return this.RedirectToAction("Index", "Torrents");
        }

        ////
        //// GET: /Account/Register

        //[AllowAnonymous]
        //public ActionResult Register()
        //{
        //    return this.View();
        //}

        ////
        //// POST: /Account/Register

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult Register(RegisterModel model)
        //{
        //    if (this.ModelState.IsValid)
        //    {
        //        // Attempt to register the user
        //        try
        //        {
        //            WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
        //            WebSecurity.Login(model.UserName, model.Password);
        //            return this.RedirectToAction("Index", "Torrents");
        //        }
        //        catch (MembershipCreateUserException e)
        //        {
        //            this.ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        ////
        //// POST: /Account/Disassociate

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Disassociate(string provider, string providerUserId)
        //{
        //    string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
        //    ManageMessageId? message = null;

        //    // Only disassociate the account if the currently logged in user is the owner
        //    if (ownerAccount == this.User.Identity.Name)
        //    {
        //        // Use a transaction to prevent the user from deleting their last login credential
        //        using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
        //        {
        //            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(this.User.Identity.Name));
        //            if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(this.User.Identity.Name).Count > 1)
        //            {
        //                OAuthWebSecurity.DeleteAccount(provider, providerUserId);
        //                scope.Complete();
        //                message = ManageMessageId.RemoveLoginSuccess;
        //            }
        //        }
        //    }

        //    return this.RedirectToAction("Manage", new { Message = message });
        //}

        ////
        //// GET: /Account/Manage

        //public ActionResult Manage(ManageMessageId? message)
        //{
        //    this.ViewBag.StatusMessage =
        //        message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
        //        : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
        //        : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
        //        : "";
        //    this.ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(this.User.Identity.Name));
        //    this.ViewBag.ReturnUrl = this.Url.Action("Manage");
        //    return this.View();
        //}

        ////
        //// POST: /Account/Manage

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Manage(LocalPasswordModel model)
        //{
        //    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(this.User.Identity.Name));
        //    this.ViewBag.HasLocalPassword = hasLocalAccount;
        //    this.ViewBag.ReturnUrl = this.Url.Action("Manage");
        //    if (hasLocalAccount)
        //    {
        //        if (this.ModelState.IsValid)
        //        {
        //            // ChangePassword will throw an exception rather than return false in certain failure scenarios.
        //            bool changePasswordSucceeded;
        //            try
        //            {
        //                changePasswordSucceeded = WebSecurity.ChangePassword(this.User.Identity.Name, model.OldPassword, model.NewPassword);
        //            }
        //            catch (Exception)
        //            {
        //                changePasswordSucceeded = false;
        //            }

        //            if (changePasswordSucceeded)
        //            {
        //                return this.RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
        //            }
        //            else
        //            {
        //                this.ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // User does not have a local password so remove any validation errors caused by a missing
        //        // OldPassword field
        //        ModelState state = this.ModelState["OldPassword"];
        //        if (state != null)
        //        {
        //            state.Errors.Clear();
        //        }

        //        if (this.ModelState.IsValid)
        //        {
        //            try
        //            {
        //                WebSecurity.CreateAccount(this.User.Identity.Name, model.NewPassword);
        //                return this.RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
        //            }
        //            catch (Exception)
        //            {
        //                this.ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", this.User.Identity.Name));
        //            }
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

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
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
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
            return this.View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;
            return this.PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(this.User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            this.ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(this.User.Identity.Name));
            return this.PartialView("_RemoveExternalLoginsPartial", externalLogins);
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

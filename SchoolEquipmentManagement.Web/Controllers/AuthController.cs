using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.ViewModels.Auth;

namespace SchoolEquipmentManagement.Web.Controllers
{
    public class AuthController : AppController
    {
        private const string DisplayNameClaimType = "display_name";
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly ISecurityAuditService _securityAuditService;

        public AuthController(
            IUserAuthenticationService userAuthenticationService,
            ISecurityAuditService securityAuditService)
        {
            _userAuthenticationService = userAuthenticationService;
            _securityAuditService = securityAuditService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel, CancellationToken cancellationToken)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(viewModel.ReturnUrl);
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var result = await _userAuthenticationService.BeginSignInAsync(viewModel.UserName, viewModel.Password, cancellationToken);
            var nextStep = HandleSignInPreparationResult(viewModel, result);
            if (nextStep is not null)
            {
                return nextStep;
            }

            await SignInUserAsync(result.User!, viewModel.RememberMe);
            return RedirectToLocal(viewModel.ReturnUrl);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult VerifyTwoFactor(string challenge, string? returnUrl = null, bool rememberMe = false, string? destination = null)
        {
            if (string.IsNullOrWhiteSpace(challenge))
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new VerifyTwoFactorViewModel
            {
                ChallengeToken = challenge,
                ReturnUrl = returnUrl,
                RememberMe = rememberMe,
                DestinationHint = destination
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var result = await _userAuthenticationService.VerifyTwoFactorAsync(viewModel.ChallengeToken, viewModel.Code, cancellationToken);
            if (!result.Succeeded || result.User is null)
            {
                return ReturnViewWithError(
                    viewModel,
                    result.ErrorMessage ?? "Неверный код подтверждения.");
            }

            await SignInUserAsync(result.User, viewModel.RememberMe);
            return RedirectToLocal(viewModel.ReturnUrl);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var resetUrlTemplate = Url.Action(nameof(ResetPassword), "Auth", new { challenge = "{token}" }, Request.Scheme)
                ?? $"{Request.Scheme}://{Request.Host}/Auth/ResetPassword?challenge={{token}}";

            await _userAuthenticationService.RequestPasswordResetAsync(viewModel.LoginOrEmail, resetUrlTemplate, cancellationToken);
            return View("ForgotPasswordConfirmation");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string challenge)
        {
            if (string.IsNullOrWhiteSpace(challenge))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            return View(new ResetPasswordViewModel
            {
                ChallengeToken = challenge
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var result = await _userAuthenticationService.ResetPasswordAsync(viewModel.ChallengeToken, viewModel.Code, viewModel.NewPassword, cancellationToken);
            if (!result.Succeeded)
            {
                return ReturnViewWithError(
                    viewModel,
                    result.ErrorMessage ?? "Не удалось сбросить пароль.");
            }

            return View("ResetPasswordConfirmation");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _securityAuditService.WriteAsync(new SecurityAuditWriteDto
            {
                EventType = SecurityAuditEventType.Logout,
                IsSuccessful = true,
                UserName = User.Identity?.Name,
                Summary = "Пользователь завершил сеанс."
            });

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> AccessDenied(string? returnUrl = null)
        {
            await _securityAuditService.WriteAsync(new SecurityAuditWriteDto
            {
                EventType = SecurityAuditEventType.AccessDenied,
                IsSuccessful = false,
                UserName = User.Identity?.Name,
                Summary = string.IsNullOrWhiteSpace(returnUrl)
                    ? "Попытка доступа к запрещенному разделу."
                    : $"Попытка доступа к запрещенному разделу: {returnUrl}"
            });

            return View(model: returnUrl);
        }

        private async Task SignInUserAsync(AuthenticatedUser user, bool rememberMe)
        {
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(CreateClaims(user), CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    AllowRefresh = true
                });
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private IActionResult? HandleSignInPreparationResult(LoginViewModel viewModel, SignInPreparationResult result)
        {
            return result.Status switch
            {
                SignInPreparationStatus.InvalidCredentials => ReturnViewWithError(viewModel, "Неверный логин или пароль."),
                SignInPreparationStatus.LockedOut => ReturnViewWithError(viewModel, result.ErrorMessage ?? "Учетная запись временно заблокирована."),
                SignInPreparationStatus.TwoFactorUnavailable => ReturnViewWithError(viewModel, result.ErrorMessage ?? "Невозможно завершить вход."),
                SignInPreparationStatus.RequiresTwoFactor => RedirectToVerifyTwoFactor(viewModel, result),
                _ => null
            };
        }

        private IActionResult RedirectToVerifyTwoFactor(LoginViewModel viewModel, SignInPreparationResult result)
        {
            return RedirectToAction(nameof(VerifyTwoFactor), new
            {
                challenge = result.ChallengeToken,
                returnUrl = viewModel.ReturnUrl,
                rememberMe = viewModel.RememberMe,
                destination = result.DestinationHint
            })!;
        }

        private IActionResult ReturnViewWithError<TViewModel>(TViewModel viewModel, string message)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(viewModel);
        }

        private static IEnumerable<Claim> CreateClaims(AuthenticatedUser user)
        {
            return
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(DisplayNameClaimType, user.DisplayName)
            ];
        }
    }
}

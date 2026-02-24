using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using ShopNet.Models;
using ShopNet.Models.ViewModels;

namespace ShopNet.Controllers
{
    public class AccountController : Controller
    {

        // UserManager    — CRUD operations on users (create, find, add to role etc.)
        // SignInManager  — login/logout, cookie management, lockout handling

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // ── REGISTER ──────────────────────────────────────────

        [HttpGet]
        [AllowAnonymous] //Explicitly allow - even if you add global auth later.
        public IActionResult Register(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.Email,       // Identity uses UserName for login
                Email = vm.Email,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                PhoneNumber = vm.PhoneNumber
            };

            // CreateAsync hashes the password — NEVER store plain text
            var result = await _userManager.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // Assign default Customer Role
                await _userManager.AddToRoleAsync(user, "Customer");

                _logger.LogInformation("New user registerd: {Email}", vm.Email);

                //Sign in immediately after registeration
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["Success"] = $"Welcome to ShopNet, {user.FirstName}";

                return RedirectToLocal(returnUrl);
            }

            // Identity returns detailed errors - show them all
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        // ── LOGIN ─────────────────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(vm);

            /* 
                PasswordSignInAsync handles:
                - Password Verification (comparing hash)
                - Account lockout tracking
                - Creating the auth cookie
            */

            var result = await _signInManager.PasswordSignInAsync(
                vm.Email,
                vm.Password,
                vm.RememberMe,              // true = presistent cookie (stays after browser close)
                lockoutOnFailure: true);    // Enables brute-fore protection

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in: {Email}", vm.Email);
                TempData["Success"] = "Wecome Back";
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                // Best practice: Dont revel WHY login failed (enumeration attack)
                _logger.LogInformation("Account locked out: {Email}", vm.Email);
                return View("Lockout");
            }

            // Same generic message for wrong email OR worng password
            // Dont say 'email not found' - that reveals which emails are registered
            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");

            return View(vm);
        }

        // ── LOGOUT ────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        // ── PROFILE ───────────────────────────────────────────
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(user);
        }

        // ── ACCESS DENIED ─────────────────────────────────────
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ── HELPER ────────────────────────────────────────────
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            // Security : only redirect to local URLs to prevent open redirect attach
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
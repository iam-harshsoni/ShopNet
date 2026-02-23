using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShopNet.Models;
using ShopNet.Services.Interfaces;

namespace ShopNet.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly ILogger<CartController> _logger;

        public CartController(IProductService productService, ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _logger = logger;
        }

        // Get: Cart
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            ViewData["CartTotal"] = _cartService.GetCartTotal();
            return View(cart);
        }

        // POST :/Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int qty, string? returnUrl = null)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Products");
            }

            if (!product.IsInStock)
            {
                TempData["Error"] = $"'{product.Name}' is out of stock.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            if (qty < 1) qty = 1;
            if (qty > product.Stock) qty = product.Stock;

            _cartService.AddToCart(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                Quantity = qty
            });

            TempData["Success"] = $"'{product.Name}' added to cart";

            // If a returnUrl was passed, go back there (e.g. products listing)
            // LocalRedirect prevents open redirect attacks — only allows local URLs
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Details", "Product", new { id = productId });
        }

        // POST: /Cart/Remove
        [HttpPost]
        public IActionResult Remove(int productId)
        {
            _cartService.RemoveFromCart(productId);
            TempData["Success"] = "Item removed from cart.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int qty)
        {
            _cartService.UpdateQuantity(productId, qty);
            return RedirectToAction(nameof(Index));
        }

        //POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cartService.ClearCart();
            TempData["Success"] = "Cart cleared.";
            return RedirectToAction(nameof(Index));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
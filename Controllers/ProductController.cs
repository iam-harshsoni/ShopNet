using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using ShopNet.Models;
using ShopNet.Repository.IRepository;
using ShopNet.Services.Interfaces;

namespace ShopNet.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ProductController> _logger;

        public ProductController(
            ILogger<ProductController> logger,
            IProductService productService,
            ICartService cartService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _productService = productService;
            _cartService = cartService;
            _unitOfWork = unitOfWork;
        }

        //GET : /Products
        //GET : /Products?search=keyword
        //Get : /Products?categoryId=1
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var products = await _productService.SearchProductsAsync(search, categoryId);
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var categoriesList = categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();

            ViewData["CurrentSearch"] = search;
            ViewData["CurrentCategoryId"] = categoryId;
            ViewData["Categories"] = categoriesList;
            ViewData["CurrentSelectedCategory"] = categoriesList.FirstOrDefault(c => c.Id == categoryId)?.Name;

            return View(products);
        }

        // Get: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                _logger.LogInformation("Product not found: ID {id}", id);
                return NotFound();
            }

            return View(product);
        }

        // POST : /Products/AddToCart
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, string productName, string imageUrl, decimal price, int qty)
        {
            _cartService.AddToCart(new Models.CartItem
            {
                ProductId = productId,
                ProductName = productName,
                ImageUrl = imageUrl,
                Price = price,
                Quantity = qty
            });

            TempData["Success"] = $"'{productName}' added to cart!";
            return RedirectToAction("Details", new { id = productId });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
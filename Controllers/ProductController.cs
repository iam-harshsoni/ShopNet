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
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ProductController> _logger;

        public ProductController(
            ILogger<ProductController> logger,
            IProductService productService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _productService = productService;
            _unitOfWork = unitOfWork;
        }

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
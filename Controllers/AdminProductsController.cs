using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using ShopNet.Models;
using ShopNet.Models.ViewModels;
using ShopNet.Repository.IRepository;
using ShopNet.Services.Interfaces;

namespace ShopNet.Controllers
{
    // [Authorize(Roles = "Admin,StoreManager")] 
    [Authorize(Policy = "AdminOrStoreManager")]  //Using Policy instead of Role string (preferred in production):
    public class AdminProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminProductsController> _logger;

        public AdminProductsController(
            ILogger<AdminProductsController> logger,
            IProductService productService,
            IUnitOfWork unitOfWork)
        {
            _productService = productService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: /AdminProducts
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        // Get: /AdminProducts/Create
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductCreateViewModel()
            {
                Categories = (await _unitOfWork.Categories.GetAllAsync()).ToList()
            };
            return View(vm);
        }

        // Post: /AdminProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> Create(ProductCreateViewModel vm)
        {
            // ModelState.IsValid checks ALL Data Annotations on the ViewModel
            if (!ModelState.IsValid)
            {
                // Repopulate categories — they're not submitted with the form
                vm.Categories = (await _unitOfWork.Categories.GetAllAsync()).ToList();
                return View(vm); // Return form with validation errors shown
            }

            // Map ViewModel → Domain Model
            // Never trust the whole model from user input
            var product = new Product()
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                Stock = vm.Stock,
                ImageUrl = vm.ImageUrl ?? "https://placehold.co/600x400?text=No+Image",
                CategoryId = vm.CategoryId
            };

            await _productService.CreateProductAsync(product);

            TempData["Success"] = $"Product '{product.Name}' create successfully";
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminProducts/Edit/5
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var vm = new ProductEditViewModel()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                Categories = (await _unitOfWork.Categories.GetAllAsync()).ToList()
            };

            return View(vm);
        }

        // POST: /AdminProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                vm.Categories = (await _unitOfWork.Categories.GetAllAsync()).ToList();
                return View(vm);
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            // Only update the fields we want to allow changing
            product.Name = vm.Name;
            product.Description = vm.Description;
            product.Price = vm.Price;
            product.Stock = vm.Stock;
            product.ImageUrl = vm.ImageUrl ?? product.ImageUrl;
            product.CategoryId = vm.CategoryId;

            await _productService.UpdateProductAsync(product);

            TempData["Success"] = $"Product '{product.Name}' updated successfully";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminProducts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteProductAsync(id);

            TempData[success ? "Success" : "Error"] = success
                ? "Product deleted successfully."
                : "Product not found.";

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
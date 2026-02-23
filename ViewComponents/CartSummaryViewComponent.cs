using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShopNet.Services.Interfaces;

namespace ShopNet.ViewComponents
{
    /// <summary>
    /// Self-contained component that fetches cart data independently.
    /// Used in _Layout.cshtml navbar — no parent controller involvement.
    /// </summary>
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartSummaryViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        // InvokeAsync is the entry point — like an Action method in a Controller
        public IViewComponentResult Invoke()
        {
            var count = _cartService.GetCartCount();
            var total = _cartService.GetCartTotal();

            // ViewComponent looks for its view at:
            // Views/Shared/Components/CartSummary/Default.cshtml
            return View(new CartSummaryViewModel
            {
                ItemCount = count,
                Total = total
            });
        }

    }

    // Small ViewModel just for this component
    public class CartSummaryViewModel
    {
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
    }
}
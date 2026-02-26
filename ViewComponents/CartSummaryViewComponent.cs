using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShopNet.Services.Interfaces;

namespace ShopNet.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartSummaryViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            var count = _cartService.GetCartCount();
            var total = _cartService.GetCartTotal();

            return View(new CartSummaryViewModel
            {
                ItemCount = count,
                Total = total
            });
        }

    }

    public class CartSummaryViewModel
    {
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
    }
}
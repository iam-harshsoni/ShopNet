using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text.Json;
using System.Threading.Tasks;
using ShopNet.Models;
using ShopNet.Services.Interfaces;

namespace ShopNet.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "ShopNet_Cart";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<CartItem> GetCart()
        {
            var json = Session.GetString(CartSessionKey);

            return string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
            => Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));

        public void AddToCart(CartItem item)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == item.ProductId);

            if (existing != null)
                existing.Quantity += item.Quantity; // Already in cart -> increase qty.
            else
                cart.Add(item);                     // New item -> Add to cart.

            SaveCart(cart);
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.ProductId == productId);
            SaveCart(cart);
        }

        public void UpdateQuantity(int productId, int qty)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item == null) return;

            if (qty <= 0)
                cart.RemoveAll(x => x.ProductId == productId);
            else
                item.Quantity = qty;

            SaveCart(cart);
        }

        public void ClearCart()
            => Session.Remove(CartSessionKey);

        public int GetCartCount()
            => GetCart().Sum(x => x.Quantity);

        public decimal GetCartTotal()
            => GetCart().Sum(x => x.Total);
    }
}
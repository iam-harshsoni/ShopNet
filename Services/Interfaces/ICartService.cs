using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopNet.Models;

namespace ShopNet.Services.Interfaces
{
    public interface ICartService
    {
        List<CartItem> GetCart();
        void AddToCart(CartItem item);
        void RemoveFromCart(int productId);
        void UpdateQuantity(int productId, int qty);
        void ClearCart();
        int GetCartCount();
        decimal GetCartTotal();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopNet.Models.ViewModels
{
    public class ProductEditViewModel : ProductCreateViewModel
    {
        // Edit needs the Id to know which product to update
        // Everything else inherited from CreateViewModel
        public int Id { get; set; }
    }
}
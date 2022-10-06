using FinalVersion.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalVersion.ViewModels
{
    public class ListBoxItem
    {
        public string Title { get; set; }
        public string? Type { get; set; } = null;
        public string ArticleNumber { get; set; }
        public string? Materials { get; set; } = null;
        //public string? Image { get; set; } = null;
        public decimal MinCostForAgent { get; set; }

        public ListBoxItem(string title, string? type, string article, decimal cost) // string ?image,
        {
            Title = title;
            Type = type;
            ArticleNumber = article;
            //Image = image;
            MinCostForAgent = cost;
        }
    }

    public class ProductsLayoutViewModel : ViewModel
    {
        public List<ListBoxItem> ListBoxItems { get; set; }

        public ProductsLayoutViewModel()
        {
            ListBoxItems = new List<ListBoxItem>();
            InitializeListBoxItems();
        }

        private void InitializeListBoxItems()
        {
            List<Product> products = new DemEkz3Context().Products.Include(p => p.ProductType).ToList();

            foreach (Product product in products)
            {
                ListBoxItem listBoxItem = new ListBoxItem(product.Title, product.ProductType?.Title, product.ArticleNumber, product.MinCostForAgent);   // product.Image

                List<Material> materials = GetProductIdMaterials(product.Id);

                listBoxItem.Materials = MaterialsToString(materials);

                ListBoxItems.Add(listBoxItem);
            }
        }

        private List<Material> GetProductIdMaterials(int product_id)
        {
            List<Material> materials = new List<Material>();
            List<ProductMaterial> productMaterials = new DemEkz3Context().ProductMaterials.Include(m => m.Material).Where(p => p.ProductId == product_id).ToList();

            foreach (ProductMaterial productMaterial in productMaterials)
                materials.Add(productMaterial.Material);

            return materials;
        }

        private string? MaterialsToString(List<Material> materials)
        {
            if (materials.Count == 0)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.Append("Материалы: ");

            foreach (Material material in materials)
                sb.Append($"{material.Title}, ");

            sb.Remove(sb.Length - 2, 2);

            return sb.ToString();
        }
    }
}

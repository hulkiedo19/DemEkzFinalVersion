using FinalVersion.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FinalVersion.Views
{
    /// <summary>
    /// Interaction logic for ProductsLayout.xaml
    /// </summary>
    public class ListBoxItem
    {
        public string Title { get; set; }
        public string? Type { get; set; } = null;
        public string ArticleNumber { get; set; }
        public string? Materials { get; set; } = null;
        public decimal MinCostForAgent { get; set; }

        public ListBoxItem(string title, string? type, string article, decimal cost)
        {
            Title = title;
            Type = type;
            ArticleNumber = article;
            MinCostForAgent = cost;
        }
    }
    public partial class ProductsLayout : Window
    {
        private List<ListBoxItem> ?AllItems = null;
        private List<ListBoxItem> ?Page = null;
        private int PageSize = 4;
        private int PageNumber = 0;
        private int PageMax = 1;

        public ProductsLayout()
        {
            InitializeComponent();

            SetComboBoxes();

            AllItems = GetDatabaseItems();
            PageMax = GetPageMax();
            SetPage();
            SetButtons();
        }

        // listboxitems
        private List<ListBoxItem> GetDatabaseItems()
        {
            List<ListBoxItem> ListBoxItems = new List<ListBoxItem>();
            List<Product> products = new DemEkz3Context().Products.Include(p => p.ProductType).ToList();

            foreach (Product product in products)
            {
                ListBoxItem listBoxItem = new ListBoxItem(product.Title, product.ProductType?.Title, product.ArticleNumber, product.MinCostForAgent);

                List<Material> materials = GetProductIdMaterials(product.Id);

                listBoxItem.Materials = MaterialsToString(materials);

                ListBoxItems.Add(listBoxItem);
            }

            return ListBoxItems;
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

        // pages
        private int GetPageMax()
        {
            if (AllItems == null)
                return 1;

            return (AllItems.Count / PageSize) + ((AllItems.Count % PageSize) > 0 ? 1 : 0);
        }

        private void SetPage()
        {
            if (AllItems == null)
            {
                ListView1.ItemsSource = new List<ListBoxItem>();
                return;
            }

            Page = AllItems
                .Skip(PageNumber * PageSize)
                .Take(PageSize)
                .ToList();

            ListView1.ItemsSource = Page;
        }

        private void SetButtons()
        {
            StackPanelPages.Children.Clear();

            // sets page left button
            Button left_page = new Button();
            left_page.Content = "<";
            left_page.Margin = new Thickness(0, 0, 2, 0);
            left_page.BorderBrush = new SolidColorBrush(Colors.White);
            left_page.BorderThickness = new Thickness(0, 0, 0, 0);
            left_page.Background = new SolidColorBrush(Colors.White);
            left_page.Click += PageLeft_Click;

            StackPanelPages.Children.Add(left_page);

            // sets main numbered buttons
            for (int i = 0; i < PageMax; i++)
            {
                Button number_page = new Button();
                number_page.Content = $"{i + 1}";
                number_page.Margin = new Thickness(2, 0, 2, 0);
                number_page.BorderBrush = new SolidColorBrush(Colors.White);
                number_page.BorderThickness = new Thickness(0, 0, 0, 0);
                number_page.Background = new SolidColorBrush(Colors.White);
                number_page.Click += SpecifiedPageButtonClick;

                StackPanelPages.Children.Add(number_page);
            }

            // sets page left button
            Button right_page = new Button();
            right_page.Content = ">";
            right_page.Margin = new Thickness(2, 0, 0, 0);
            right_page.BorderBrush = new SolidColorBrush(Colors.White);
            right_page.BorderThickness = new Thickness(0, 0, 0, 0);
            right_page.Background = new SolidColorBrush(Colors.White);
            right_page.Click += PageRight_Click;

            StackPanelPages.Children.Add(right_page);
        }

        public void SpecifiedPageButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            PageNumber = Convert.ToInt32(button.Content) - 1;
            SetPage();
        }

        public void PageLeft_Click(object sender, RoutedEventArgs e)
        {
            if (PageNumber <= 1)
                return;

            PageNumber--;
            SetPage();
        }

        public void PageRight_Click(object sender, RoutedEventArgs e)
        {
            if (PageNumber >= PageMax - 1)
                return;

            PageNumber++;
            SetPage();
        }

        // search text
        private List<ListBoxItem>? GetDatabaseItemsText(string text)
        {
            List<ListBoxItem> Items = GetDatabaseItems();

            Items = Items
                .Where(p => p.Title.ToLower().Contains(text.ToLower()))
                .ToList();

            if (Items.Count == 0)
                return null;

            return Items;
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InputTextBox.Text == "")
                AllItems = GetDatabaseItems();
            else
                AllItems = GetDatabaseItemsText(InputTextBox.Text);

            PageNumber = 1;
            PageMax = GetPageMax();
            SetPage();
            SetButtons();
        }

        // combo boxes
        private void SetComboBoxes()
        {
            ComboBoxSort.ItemsSource = new List<string>()
            {
                "По умолчанию",
                "Меньше цена",
                "Больше цена",
                "Больше материалов",
                "Меньше материалов"
            };
            ComboBoxFilter.ItemsSource = new List<string>()
            {
                "По умолчанию",
                "С материалами",
                "Без материалов"
            };
        }

        private List<ListBoxItem> GetSortItems(int sort_index)
        {
            List<ListBoxItem> Items = GetDatabaseItems();

            if (sort_index == 0)
                return Items;

            switch (sort_index)
            {
                case 1:
                    Items = Items.OrderBy(p => p.MinCostForAgent).ToList();
                    break;
                case 2:
                    Items = Items.OrderByDescending(p => p.MinCostForAgent).ToList();
                    break;
                case 3:
                    Items = Items.OrderByDescending(p => p.Materials).ToList();
                    break;
                case 4:
                    Items = Items.OrderBy(p => p.Materials).ToList();
                    break;
            }

            return Items;
        }

        private List<ListBoxItem> GetFilterItems(int filter_index)
        {
            List<ListBoxItem> Items = GetDatabaseItems();

            if (filter_index == 0)
                return Items;

            switch (filter_index)
            {
                case 1:
                    Items = Items.Where(p => p.Materials != null).ToList();
                    break;
                case 2:
                    Items = Items.Where(p => p.Materials == null).ToList();
                    break;
            }

            return Items;
        }

        private void ComboBoxSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sort_type = ComboBoxSort.SelectedIndex;

            AllItems = GetSortItems(sort_type);

            PageNumber = 1;
            PageMax = GetPageMax();
            SetPage();
            SetButtons();
        }

        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int filter_type = ComboBoxFilter.SelectedIndex;

            AllItems = GetFilterItems(filter_type);

            PageNumber = 1;
            PageMax = GetPageMax();
            SetPage();
            SetButtons();
        }
    }
}

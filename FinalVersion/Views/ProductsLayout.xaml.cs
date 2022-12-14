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
    public class Item
    {
        public string Title { get; set; }
        public string? Type { get; set; } = null;
        public string ArticleNumber { get; set; }
        public string? Materials { get; set; } = null;
        public decimal MinCostForAgent { get; set; }
        public ImageSource Image { get; set; }

        public Item(string title, string? type, string article, string? materials, decimal cost, ImageSource image)
        {
            Title = title;
            Type = type;
            ArticleNumber = article;
            Materials = materials;
            MinCostForAgent = cost;
            Image = image;
        }
    }
    public partial class ProductsLayout : Window
    {
        private List<Item> ?AllItems = null;
        private List<Item> ?Page = null;
        private List<string> PageNumbers = new List<string>();
        private int PageSize = 4;
        private int PageNumber = 0;
        private int PageMax = 1;
        private int PagesPrint = 4;
        private int PageNumberPrintMax;

        public ProductsLayout()
        {
            InitializeComponent();

            SetComboBoxes();

            AllItems = GetDatabaseItems();
            PageMax = GetPageMax();
            SetPageNumbers();
            SetButtons();
            SetPage();
        }

        // listboxitems
        private List<Item> GetDatabaseItems()
        {
            List<Item> Items = new List<Item>();
            List<Product> products = new DemEkz3Context().Products
                .Include(p => p.ProductType)
                .ToList();

            foreach (Product product in products)
            {
                string? Materials = MaterialsToString(GetProductIdMaterials(product.Id));
                ImageSource imageSource = GetImageSource(product.Image);

                Item Item = new Item(
                    product.Title, 
                    product.ProductType?.Title, 
                    product.ArticleNumber,
                    Materials,
                    product.MinCostForAgent,
                    imageSource);

                Items.Add(Item);
            }

            return Items;
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

        private ImageSource GetImageSource(string ?ImagePath)
        {
            if (ImagePath == null)
                return new BitmapImage(new Uri("/FinalVersion;component/Resources/picture.png", UriKind.Relative));

            return new BitmapImage(new Uri("/FinalVersion;component/" + ImagePath, UriKind.Relative));
        }

        // pages
        private void SetPageNumbers()
        {
            if (PageMax > PagesPrint)
                PageNumberPrintMax = PageMax - PagesPrint;
            else
                PageNumberPrintMax = PageMax;

            PageNumbers.Clear();

            for (int i = 0; i < PageMax; i++)
                PageNumbers.Add(Convert.ToString(i + 1));
        }

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

            {
                // sets page left button
                Button left_page = new Button();
                left_page.Content = "<";
                left_page.Margin = new Thickness(0, 0, 2, 0);
                left_page.BorderBrush = new SolidColorBrush(Colors.White);
                left_page.BorderThickness = new Thickness(0, 0, 0, 0);
                left_page.Background = new SolidColorBrush(Colors.White);
                left_page.Width = 15.0;
                left_page.Click += PageLeft_Click;

                StackPanelPages.Children.Add(left_page);
            }

            int StartPagePrintNumber = (PageNumber > PageNumberPrintMax) ? PageNumberPrintMax : PageNumber;
            int PagesPrintCount = (PagesPrint > PageNumbers.Count) ? PageNumbers.Count : PagesPrint;

            // sets main numbered buttons
            for (int i = 0; i < PagesPrintCount; i++)
            {
                Button number_page = new Button();
                number_page.Content = PageNumbers[StartPagePrintNumber + i];
                number_page.Margin = new Thickness(2, 0, 2, 0);
                number_page.BorderBrush = new SolidColorBrush(Colors.White);
                number_page.BorderThickness = new Thickness(0, 0, 0, 0);
                number_page.Background = new SolidColorBrush(Colors.White);
                number_page.Width = 15.0;
                number_page.Click += SpecifiedPageButtonClick;

                StackPanelPages.Children.Add(number_page);
            }

            {
                // sets page left button
                Button right_page = new Button();
                right_page.Content = ">";
                right_page.Margin = new Thickness(2, 0, 0, 0);
                right_page.BorderBrush = new SolidColorBrush(Colors.White);
                right_page.BorderThickness = new Thickness(0, 0, 0, 0);
                right_page.Background = new SolidColorBrush(Colors.White);
                right_page.Width = 15.0;
                right_page.Click += PageRight_Click;

                StackPanelPages.Children.Add(right_page);
            }
            }

        public void SpecifiedPageButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            PageNumber = Convert.ToInt32(button.Content) - 1;
            SetPage();
        }

        public void PageLeft_Click(object sender, RoutedEventArgs e)
        {
            if (PageNumber == 0)
                return;

            PageNumber--;
            SetPageNumbers();
            SetButtons();
            SetPage();
        }

        public void PageRight_Click(object sender, RoutedEventArgs e)
        {
            if (PageNumber >= PageMax - 1)
                return;

            PageNumber++;
            SetPageNumbers();
            SetButtons();
            SetPage();
        }

        // search text
        private List<Item>? GetDatabaseItemsText(string text)
        {
            List<Item> Items = GetDatabaseItems();

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

            PageNumber = 0;
            PageMax = GetPageMax();
            SetPageNumbers();
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

        private List<Item> GetSortItems(int sort_index)
        {
            List<Item> Items = GetDatabaseItems();

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

        private List<Item> GetFilterItems(int filter_index)
        {
            List<Item> Items = GetDatabaseItems();

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

            PageNumber = 0;
            PageMax = GetPageMax();
            SetPageNumbers();
            SetPage();
            SetButtons();
        }

        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int filter_type = ComboBoxFilter.SelectedIndex;

            AllItems = GetFilterItems(filter_type);

            PageNumber = 0;
            PageMax = GetPageMax();
            SetPageNumbers();
            SetPage();
            SetButtons();
        }
    }
}

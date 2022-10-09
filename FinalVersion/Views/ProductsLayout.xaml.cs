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
        private List<List<ListBoxItem>> ?AllPages = null;
        private int CurrentPage = 0;

        public ProductsLayout()
        {
            InitializeComponent();

            SetComboBoxes();
            List<ListBoxItem> ListBoxItems = GetDatabaseItems();
            InitializePages(ListBoxItems);
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
        private void InitializePages(List<ListBoxItem> ListBoxItems)
        {
            GetPages(ListBoxItems);
            SetPage();
            SetButtons();
        }

        private void GetPages(List<ListBoxItem> ?ListBoxItems)
        {
            CurrentPage = 0;

            if(ListBoxItems == null)
            {
                if(AllPages != null)
                    AllPages.Clear();
                return;
            }

            if (AllPages != null)
                AllPages.Clear();
            else
                AllPages = new List<List<ListBoxItem>>();

            int element_index = 0;
            int pages = ListBoxItems.Count / 4;
            int last_page_elements = ListBoxItems.Count % 4;

            // sets all 4-element pages
            for(int i = 0; i < pages; i++)
            {
                List<ListBoxItem> page = new List<ListBoxItem>();

                for (int j = 0; j < 4; j++)
                    page.Add(ListBoxItems[element_index++]);

                AllPages.Add(page);
            }

            // if there are elements left, the last page will be added
            if(last_page_elements > 0)
            {
                List<ListBoxItem> page = new List<ListBoxItem>();

                for (int i = 0; i < last_page_elements; i++)
                    page.Add(ListBoxItems[element_index++]);

                AllPages.Add(page);
            }
        }

        private void SetPage()
        {
            if (AllPages == null || AllPages.Count == 0)
                ListView1.ItemsSource = new List<List<ListBoxItem>>();
            else
                ListView1.ItemsSource = AllPages[CurrentPage];
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
            for (int i = 0; i < AllPages.Count; i++)
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

            int page_number = Convert.ToInt32(button.Content);

            CurrentPage = page_number - 1;
            SetPage();
        }

        public void PageLeft_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage == 0)
                return;

            CurrentPage--;
            SetPage();
        }

        public void PageRight_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage == (AllPages.Count - 1))
                return;

            CurrentPage++;
            SetPage();
        }

        // search text
        private List<ListBoxItem>? GetDatabaseItemsText(string text)
        {
            List<ListBoxItem> ListBoxItems = GetDatabaseItems();

            ListBoxItems = ListBoxItems
                .Where(p => p.Title.ToLower().Contains(text.ToLower()))
                .ToList();

            if (ListBoxItems.Count == 0)
                return null;

            return ListBoxItems;
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
       {
            List<ListBoxItem> ?ListBoxItems = null;

            if (InputTextBox.Text == "")
                ListBoxItems = GetDatabaseItems();
            else
                ListBoxItems = GetDatabaseItemsText(InputTextBox.Text);

            GetPages(ListBoxItems);
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
            List<ListBoxItem> ListBoxItems = GetDatabaseItems();

            if (sort_index == 0)
                return ListBoxItems;

            switch (sort_index)
            {
                case 1:
                    ListBoxItems = ListBoxItems.OrderBy(p => p.MinCostForAgent).ToList();
                    break;
                case 2:
                    ListBoxItems = ListBoxItems.OrderByDescending(p => p.MinCostForAgent).ToList();
                    break;
                case 3:
                    ListBoxItems = ListBoxItems.OrderByDescending(p => p.Materials).ToList();
                    break;
                case 4:
                    ListBoxItems = ListBoxItems.OrderBy(p => p.Materials).ToList();
                    break;
            }

            return ListBoxItems;
        }

        private List<ListBoxItem> GetFilterItems(int filter_index)
        {
            List<ListBoxItem> ListBoxItems = GetDatabaseItems();

            if (filter_index == 0)
                return ListBoxItems;

            switch (filter_index)
            {
                case 1:
                    ListBoxItems = ListBoxItems.Where(p => p.Materials != null).ToList();
                    break;
                case 2:
                    ListBoxItems = ListBoxItems.Where(p => p.Materials == null).ToList();
                    break;
            }

            return ListBoxItems;
        }

        private void ComboBoxSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sort_type = ComboBoxSort.SelectedIndex;

            List<ListBoxItem> ListBoxItems = GetSortItems(sort_type);
            GetPages(ListBoxItems);
            SetPage();
            SetButtons();
        }

        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int filter_type = ComboBoxFilter.SelectedIndex;

            List<ListBoxItem> ListBoxItems = GetFilterItems(filter_type);
            GetPages(ListBoxItems);
            SetPage();
            SetButtons();
        }
    }
}

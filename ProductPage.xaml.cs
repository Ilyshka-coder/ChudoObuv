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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChudoObuv
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();
            var currentServices = AlalykinChydoobuvEntities.GetContext().Products.ToList();
            ProductListView.ItemsSource = currentServices;
            ComboType.SelectedIndex = 0;
            UpdateProducts();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                AlalykinChydoobuvEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                ProductListView.ItemsSource = AlalykinChydoobuvEntities.GetContext().Products.ToList();
                UpdateProducts();
            }
        }
        private void UpdateProducts()
        {
            var currentServices = AlalykinChydoobuvEntities.GetContext().Products.ToList();
            if (ComboType.SelectedIndex == 1)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.CategoryID) == 3)).ToList();
            }
            if (ComboType.SelectedIndex == 2)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.CategoryID) == 2)).ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.CategoryID) == 1)).ToList();
            }
            if (ComboType.SelectedIndex == 0)
            {
                currentServices = currentServices.ToList();
            }
            currentServices = currentServices.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
            ProductListView.ItemsSource = currentServices.ToList();
            if (RButtonDown.IsChecked.Value)
            {
                currentServices = currentServices.OrderByDescending(p => p.Price).ToList();
            }
            if (RButtonUp.IsChecked.Value)
            {
                currentServices = currentServices.OrderBy(p => p.Price).ToList();
            }
        }
    }
}

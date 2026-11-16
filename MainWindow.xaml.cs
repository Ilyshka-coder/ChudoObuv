
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ЧудоОбувь
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Manager.MainFrame = MainFrame;

            MainFrame.Navigated += MainFrame_Navigated;

            MainFrame.Navigate(new LoginPage());

            UpdateUserInfo();
        }

        // ОБРАБОТКА ПЕРЕХОДОВ МЕЖДУ СТРАНИЦАМИ
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            BtnBack.Visibility = MainFrame.CanGoBack
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (e.Content is LoginPage)
            {
                UserInfoTB.Visibility = Visibility.Collapsed;

                // Очищаем историю навигации
                while (MainFrame.CanGoBack)
                {
                    MainFrame.RemoveBackEntry();
                }

                BtnBack.Visibility = Visibility.Collapsed;

                // Скрываем все кнопки
                BtnAnalytics.Visibility = Visibility.Collapsed;
                BtnOrders.Visibility = Visibility.Collapsed;
                BtnCart.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserInfoTB.Visibility = Visibility.Visible;

                // Обновляем информацию и кнопки
                UpdateUserInfo();
            }
        }

        // ОБНОВЛЕНИЕ ИНФОРМАЦИИ О ПОЛЬЗОВАТЕЛЕ
        public void UpdateUserInfo()
        {
            if (Manager.CurrentUser == null || Manager.IsGuest)
            {
                UserInfoTB.Text = "Гость";

                // Гость не видит ни одну из этих кнопок
                BtnAnalytics.Visibility = Visibility.Collapsed;
                BtnOrders.Visibility = Visibility.Collapsed;
                BtnCart.Visibility = Visibility.Collapsed;

                return;
            }

            // Получаем роль пользователя
            string role = Manager.CurrentUser.Roles?.RoleName?.Trim();

            if (string.IsNullOrEmpty(role))
            {
                if (Manager.CurrentUser.RoleID == 1)
                    role = "Администратор";
                else if (Manager.CurrentUser.RoleID == 2)
                    role = "Менеджер";
                else if (Manager.CurrentUser.RoleID == 3)
                    role = "Авторизованный пользователь";
            }

            UserInfoTB.Text =
                $"{Manager.CurrentUser.UserSurname} " +
                $"{Manager.CurrentUser.UserName} ({role})";

            // АДМИНИСТРАТОР
            if (Manager.IsAdmin)
            {
                BtnAnalytics.Visibility = Visibility.Visible;
                BtnOrders.Visibility = Visibility.Visible;
                BtnCart.Visibility = Visibility.Visible;
            }
            // МЕНЕДЖЕР
            else if (Manager.IsManager)
            {
                BtnAnalytics.Visibility = Visibility.Collapsed;
                BtnOrders.Visibility = Visibility.Visible;
                BtnCart.Visibility = Visibility.Visible;
            }
            // ОБЫЧНЫЙ ПОЛЬЗОВАТЕЛЬ
            else
            {
                BtnAnalytics.Visibility = Visibility.Collapsed;
                BtnOrders.Visibility = Visibility.Collapsed;
                BtnCart.Visibility = Visibility.Visible;
            }
        }

        // АНАЛИТИКА
        private void BtnAnalytics_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.IsAdmin)
            {
                MainFrame.Navigate(new AnalyticsPage());
            }
        }

        // КОРЗИНА
        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            if (!Manager.IsGuest)
            {
                MainFrame.Navigate(new CartPage());
            }
        }

        // ЗАКАЗЫ
        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            if (!Manager.IsGuest)
            {
                MainFrame.Navigate(new OrdersPage());
            }
        }

        // НАЗАД
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }
    }
}
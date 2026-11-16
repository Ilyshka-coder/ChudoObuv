using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ЧудоОбувь;

namespace ЧудоОбувь
{
    public partial class OrdersPage : Page
    {
        private bool isUpdatingDate = false;

        public OrdersPage()
        {
            InitializeComponent();
            UpdateOrders();
        }

        

        // ЗАГРУЗКА ЗАКАЗОВ
        private void UpdateOrders()
        {
            try
            {
                var context = ChudoShoesEntities1.GetContext();

                var ordersQuery = context.Orders.AsQueryable();

                // Обычный пользователь видит только свои заказы
                if (Manager.IsUser)
                {
                    ordersQuery = ordersQuery.Where(
                        o => o.UserID == Manager.CurrentUser.UserID);
                }

                var ordersList = ordersQuery.ToList()
                    .Select(o => new
                    {
                        OrderID = o.OrderID,
                        OrderDate = o.OrderDate.Date,
                        ClientName = o.Users.UserSurname + " " +
                                     o.Users.UserName + " " +
                                     o.Users.UserLastName,
                        Cost = o.Cost,
                        OriginalOrder = o
                    })
                    .ToList();

                OrdersGrid.ItemsSource = ordersList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message);
            }
        }

        

        // ВЫБОР ЗАКАЗА
        private void OrdersGrid_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (OrdersGrid.SelectedItem == null)
            {
                OrderDetailsPanel.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                dynamic selected = OrdersGrid.SelectedItem;
                Orders order = selected.OriginalOrder;

                var context = ChudoShoesEntities1.GetContext();

                var items = context.OrderItems
                    .Where(oi => oi.OrderID == order.OrderID)
                    .ToList();

                OrderItemsGrid.ItemsSource = items;

                OrderDetailsPanel.Visibility = Visibility.Visible;

                bool isAdmin = Manager.IsAdmin;
                bool isManager = Manager.IsManager;

                if (isAdmin)
                {
                    // Админ редактирует состав
                    OrderItemsGrid.Columns[3].Visibility = Visibility.Visible;
                    EditOrderPanel.Visibility = Visibility.Visible;
                    LoadStock();

                    // И дату
                    OrderDateTB.Visibility = Visibility.Visible;
                    OrderDatePicker.Visibility = Visibility.Visible;

                    isUpdatingDate = true;
                    OrderDatePicker.SelectedDate = order.OrderDate;
                    isUpdatingDate = false;

                }
                else
                {
                    // Менеджер и пользователь не редактируют
                    OrderItemsGrid.Columns[3].Visibility = Visibility.Collapsed;
                    EditOrderPanel.Visibility = Visibility.Collapsed;
                    OrderDateTB.Visibility = Visibility.Collapsed;
                    OrderDatePicker.Visibility = Visibility.Collapsed;
                }

                // Удалять может менеджер и админ
                BtnDeleteOrder.Visibility = (isAdmin || isManager)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                OrderDetailsPanel.Visibility = Visibility.Collapsed;
                MessageBox.Show("Ошибка загрузки состава заказа: " + ex.Message);
            }
        }

        // ЗАГРУЗКА ТОВАРОВ СО СКЛАДА
        private void LoadStock()
        {
            try
            {
                var context = ChudoShoesEntities1.GetContext();

                var stocks = context.Stock
                    .Where(s => s.Available > 0)
                    .ToList()
                    .Select(s => new
                    {
                        StockID = s.StockID,

                        Display = s.Products.ProductName +
                                  " (Размер: " + s.Sizes.SizeValue +
                                  ", В наличии: " + s.Available +
                                  ", Цена: " + s.Products.Price + ")"
                    })
                    .ToList();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки склада: " + ex.Message);
            }
        }

        // ОБНОВЛЕНИЕ ИНТЕРФЕЙСА ПОСЛЕ ИЗМЕНЕНИЙ
        private void RefreshOrderData()
        {
            if (OrdersGrid.SelectedItem == null)
                return;

            dynamic selected = OrdersGrid.SelectedItem;
            int orderId = selected.OrderID;

            UpdateOrders();

            for (int i = 0; i < OrdersGrid.Items.Count; i++)
            {
                dynamic row = OrdersGrid.Items[i];

                if (row.OrderID == orderId)
                {
                    OrdersGrid.SelectedIndex = i;
                    OrdersGrid.ScrollIntoView(OrdersGrid.Items[i]);
                    break;
                }
            }
        }

        // УВЕЛИЧИТЬ КОЛИЧЕСТВО (только админ; заказ -> черновик)
        private void BtnPlusItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Manager.IsAdmin)
                return;

            var item = (sender as Button).Tag as OrderItems;

            if (item == null)
                return;

            try
            {
                var context = ChudoShoesEntities1.GetContext();

                var dbItem = context.OrderItems.FirstOrDefault(
                    oi => oi.OrderID == item.OrderID &&
                          oi.StockID == item.StockID);

                var stock = context.Stock.Find(item.StockID);
                var order = context.Orders.Find(item.OrderID);

                if (dbItem != null && stock != null && order != null)
                {
                    if (stock.Available > 0)
                    {
                        dbItem.Quantity++;
                        stock.Available--;

                        order.Cost += stock.Products.Price;

                        context.SaveChanges();

                        RefreshOrderData();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Достигнуто максимальное доступное количество на складе.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения количества: " + ex.Message);
            }
        }

        // УМЕНЬШИТЬ КОЛИЧЕСТВО (только админ; заказ -> черновик)
        private void BtnMinusItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Manager.IsAdmin)
                return;

            var item = (sender as Button).Tag as OrderItems;

            if (item == null)
                return;

            try
            {
                var context = ChudoShoesEntities1.GetContext();

                var dbItem = context.OrderItems.FirstOrDefault(
                    oi => oi.OrderID == item.OrderID &&
                          oi.StockID == item.StockID);

                var stock = context.Stock.Find(item.StockID);
                var order = context.Orders.Find(item.OrderID);

                if (dbItem != null && stock != null && order != null)
                {
                    if (dbItem.Quantity > 1)
                    {
                        dbItem.Quantity--;

                        stock.Available++;

                        order.Cost -= stock.Products.Price;
                    }
                    else
                    {
                        stock.Available += dbItem.Quantity;

                        order.Cost -=
                            stock.Products.Price * dbItem.Quantity;

                        context.OrderItems.Remove(dbItem);
                    }


                    context.SaveChanges();

                    RefreshOrderData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения количества: " + ex.Message);
            }
        }

        // УДАЛИТЬ ТОВАРНУЮ ПОЗИЦИЮ (только админ; заказ -> черновик)
        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Manager.IsAdmin)
                return;

            var item = (sender as Button).Tag as OrderItems;

            if (item == null)
                return;

            try
            {
                var context = ChudoShoesEntities1.GetContext();

                var dbItem = context.OrderItems.FirstOrDefault(
                    oi => oi.OrderID == item.OrderID &&
                          oi.StockID == item.StockID);

                var stock = context.Stock.Find(item.StockID);
                var order = context.Orders.Find(item.OrderID);

                if (dbItem != null && stock != null && order != null)
                {
                    stock.Available += dbItem.Quantity;

                    order.Cost -=
                        stock.Products.Price * dbItem.Quantity;

                    context.OrderItems.Remove(dbItem);


                    context.SaveChanges();

                    RefreshOrderData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления товара: " + ex.Message);
            }
        }

        

        

        // УДАЛИТЬ ЗАКАЗ ЦЕЛИКОМ (менеджер и админ)
        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!Manager.IsManager && !Manager.IsAdmin)
                return;

            if (OrdersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите заказ для удаления.");
                return;
            }

            if (MessageBox.Show(
                "Вы точно хотите удалить этот заказ целиком?",
                "Внимание",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                dynamic selected = OrdersGrid.SelectedItem;

                Orders order = selected.OriginalOrder;

                var context = ChudoShoesEntities1.GetContext();

                var orderToDelete = context.Orders.Find(order.OrderID);

                if (orderToDelete != null)
                {
                    var items = context.OrderItems
                        .Where(oi => oi.OrderID == orderToDelete.OrderID)
                        .ToList();

                    // Возвращаем товары на склад
                    foreach (var item in items)
                    {
                        var stock = context.Stock.Find(item.StockID);

                        if (stock != null)
                        {
                            stock.Available += item.Quantity;
                        }
                    }

                    context.OrderItems.RemoveRange(items);

                    context.Orders.Remove(orderToDelete);

                    context.SaveChanges();

                    OrderDetailsPanel.Visibility = Visibility.Collapsed;

                    UpdateOrders();

                    MessageBox.Show("Заказ успешно удален.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении заказа: " + ex.Message);
            }
        }

        // ИЗМЕНЕНИЕ ДАТЫ ЗАКАЗА (ТОЛЬКО АДМИНИСТРАТОР; -> черновик)
        private void OrderDatePicker_SelectedDateChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (isUpdatingDate)
                return;

            if (!Manager.IsAdmin)
                return;

            if (OrdersGrid.SelectedItem == null)
                return;

            if (OrderDatePicker.SelectedDate == null)
                return;

            try
            {
                dynamic selected = OrdersGrid.SelectedItem;

                Orders order = selected.OriginalOrder;

                DateTime newDate =
                    OrderDatePicker.SelectedDate.Value.Date;

                if (newDate > DateTime.Today)
                {
                    MessageBox.Show(
                        "Дата заказа не может быть в будущем.",
                        "Недопустимая дата",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    isUpdatingDate = true;
                    OrderDatePicker.SelectedDate = order.OrderDate.Date;
                    isUpdatingDate = false;

                    return;
                }

                if (order.OrderDate.Date == newDate)
                    return;

                var context = ChudoShoesEntities1.GetContext();

                var dbOrder = context.Orders.Find(order.OrderID);

                if (dbOrder == null)
                    return;

                TimeSpan existingTime = dbOrder.OrderDate.TimeOfDay;

                dbOrder.OrderDate = newDate + existingTime;

                context.SaveChanges();

                RefreshOrderData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка сохранения даты заказа: " + ex.Message);
            }
        }
    }
}
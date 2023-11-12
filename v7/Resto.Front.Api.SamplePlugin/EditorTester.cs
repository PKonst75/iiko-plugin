using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Extensions;
using System.Windows.Controls;
using System.Threading;
using Resto.Front.Api.SamplePlugin.WpfHelpers;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;
using Button = System.Windows.Controls.Button;
using Resto.Front.Api.Data.Brd;
using System.Collections.Generic;
using Resto.Front.Api.Data.Organization.Sections;
using System.ComponentModel;

namespace Resto.Front.Api.SamplePlugin
{
    internal sealed class EditorTester : IDisposable
    {
        private Window window;
        private ItemsControl buttons;
        private readonly Random rnd = new Random();

        public EditorTester()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        private void EntryPoint()
        {
            buttons = new ItemsControl();
            window = new Window
            {
                Title = "Sample plugin",
                Content = new ScrollViewer
                {
                    Content = buttons
                },
                Width = 200,
                Height = 500,
                Topmost = true,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                ResizeMode = ResizeMode.CanMinimize,
                ShowInTaskbar = true,
                VerticalContentAlignment = VerticalAlignment.Center,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.SingleBorderWindow,
                SizeToContent = SizeToContent.Width
            };

            AddButton("Create order", CreateOrder);
            AddButton("Create order to another waiter", CreateOrderToWaiter);
            AddButton("Add guest", AddGuest);
            AddButton("Add product", AddProduct);
            AddButton("Add reserve", AddReserve);
            window.ShowDialog();
        }

        private void AddButton(string text, Action action)
        {
            var button = new Button
            {
                Content = text,
                Margin = new Thickness(2),
                Padding = new Thickness(4),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };
            button.Click += (s, e) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    var message = string.Format("{4} \n Cannot {0} :-({1}Message: {2}{1}{3}", text, Environment.NewLine, ex.Message, ex.StackTrace, ex.GetType());
                    MessageBox.Show(message, "SamplePlugin", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            buttons.Items.Add(button);
        }

        private void AddButton(string text, Action<IOrder, IOperationService> action)
        {
            AddButton(text, () =>
            {
                action(PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New), PluginContext.Operations);
            });
        }

        private void AddButton(string text, Action<IOperationService> action)
        {
            AddButton(text, () =>
            {
                action(PluginContext.Operations);
            });
        }

        /// <summary>
        /// Создание брони на рендомный стол 
        /// </summary> 
        private static void AddReserve([NotNull] IOperationService os)
        {
            var credentials = os.GetCredentials();  // Получение данных о правах доступа текущего сеанса
            PhoneDto phone = new PhoneDto
            {
                PhoneValue = "+79130000000",
                IsMain = true
            };
            List<PhoneDto> phones = new List<PhoneDto> { phone };
            IClient client = os.CreateClient(Guid.NewGuid(), "Test Reserve", phones, null, DateTime.Now, credentials);
            DateTime reserve_time = DateTime.Now.AddDays(1);
            IReadOnlyList<Data.Organization.Sections.ITable> tables = os.GetTables();
            IReadOnlyList<IReserve> reserves = os.GetReserves();
            // Список столов не пуст
            List<Data.Organization.Sections.ITable> reserve_tables = new List<ITable>();
            foreach (ITable table in tables)
            {
                if (table.IsActive)
                {
                    bool reserved = false;
                    foreach (IReserve reserve in reserves)
                    {
                        foreach (ITable rtable in reserve.Tables)
                        {
                            if (rtable.Id == table.Id)
                            {
                                // Стол уже зарезервирован
                                if (reserve.Status != ReserveStatus.Closed && reserve_time >= reserve.EstimatedStartTime && reserve_time <= reserve.EstimatedStartTime + reserve.Duration)
                                {
                                    reserved = true;
                                    break;
                                }
                            }
                        }
                        if (reserved)
                        {
                            break;
                        }
                    }
                    if (reserved)
                    {
                        continue;
                    }
                    reserve_tables.Add(table);
                    break;
                }
            }
            if (reserve_tables.Count() > 0)
            {
                IReserve res = os.CreateReserve(reserve_time, client, reserve_tables, credentials);
                if (res == null)
                {
                    // Резерв не создан
                    os.AddErrorMessage("Reserve don't created", "My plugin", new TimeSpan(0, 0, 20));
                    return;
                }
                // Резерв создан
                os.AddNotificationMessage("Reserve created", "My plugin", new TimeSpan(0, 0, 20));
            }
        }

        /// <summary>
        /// Создание заказа с добавлением гостя Alex и продукта номенклатуры.
        /// </summary>   
        private void CreateOrder()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var product1 = GetProduct();
            var product2 = GetProduct();

            // создаём заказ, добавляем в него пару гостей, одному из гостей добавляем блюдо, и всё это атомарно в одной сессии
            var editSession = PluginContext.Operations.CreateEditSession();
            var newOrder = editSession.CreateOrder(null); // заказ будет создан на столе по умолчанию
            editSession.ChangeOrderOriginName("Sample Plugin", newOrder);
            var guest1 = editSession.AddOrderGuest(null, newOrder); // настоящего гостя ещё нет (он будет после SubmitChanges), ссылаемся на будущего гостя через INewOrderGuestItemStub
            var guest2 = editSession.AddOrderGuest("Alex", newOrder);
            editSession.AddOrderProductItem(2m, product1, newOrder, guest1, null);
            var result = PluginContext.Operations.SubmitChanges(editSession);

            var previouslyCreatedOrder = result.Get(newOrder);
            var previouslyAddedGuest = result.Get(guest2); // настоящие гости уже есть, можно ссылаться напрямую на нужного гостя через IOrderGuestItem

            PluginContext.Operations.AddOrderProductItem(17.3m, product2, previouslyCreatedOrder, previouslyAddedGuest, null, credentials);
        }

        /// <summary>
        /// Создание заказа на конкретного официанта с добавлением гостя Alex и продукта номенклатуры.
        /// </summary>   
        private void CreateOrderToWaiter()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            var product = GetProduct();

            var editSession = PluginContext.Operations.CreateEditSession();
            if (!ChooseItemDialogHelper.ShowDialog(PluginContext.Operations.GetUsers(), user => user.Name, out var selectedUser, "Select waiter", window))
                return;
            var newOrder = editSession.CreateOrder(null, selectedUser); // заказ будет создан на столе по умолчанию
            editSession.ChangeOrderOriginName("Sample Plugin", newOrder);
            editSession.AddOrderGuest(null, newOrder);
            var guest2 = editSession.AddOrderGuest("Alex", newOrder);
            var result = PluginContext.Operations.SubmitChanges(editSession);

            var previouslyCreatedOrder = result.Get(newOrder);
            var previouslyAddedGuest = result.Get(guest2);

            PluginContext.Operations.AddOrderProductItem(17.3m, product, previouslyCreatedOrder, previouslyAddedGuest, null, credentials);
        }

        /// <summary>
        /// Добавление гостя John Doe. 
        /// </summary>
        private static void AddGuest([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetCredentials();
            os.AddOrderGuest("John Doe", order, credentials);
        }

        /// <summary>
        /// Добавление продукта из номенклатуры.
        /// </summary>
        private void AddProduct([NotNull] IOrder order, [NotNull] IOperationService os)
        {
            var credentials = os.GetCredentials();
            var product = GetProduct();
            var guest = order.Guests.Last();
            var addedItem = os.AddOrderProductItem(42m, product, order, guest, null, credentials);

            if (product.ImmediateCookingStart)
                os.PrintOrderItems(credentials, os.GetOrderById(order.Id), new[] { addedItem });
        }

        private IProduct GetProduct(bool isCompound = false)
        {
            var activeProducts = PluginContext.Operations.GetActiveProducts()
                .Where(product =>
                    isCompound
                        ? product.Template != null
                        : product.Template == null
                        && product.Type == ProductType.Dish)
                .ToList();

            var index = rnd.Next(activeProducts.Count);
            return activeProducts[index];
        }


        public void Dispose()
        {
            window.Dispatcher.InvokeShutdown();
            window.Dispatcher.Thread.Join();
        }
    }
}
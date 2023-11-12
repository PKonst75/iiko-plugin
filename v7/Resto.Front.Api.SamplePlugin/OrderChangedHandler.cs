using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Windows;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Organization.Sections;


namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    public class OrderChangedHandler : IDisposable
    {
        private readonly IDisposable subscription;

        public event EventHandler<Table> TableOrderChange; // Событие - изменение стола!

        public OrderChangedHandler()
        {
            subscription = PluginContext.Notifications.OrderChanged.Subscribe(s => OnOrderChanged(s.Entity));
        }

        private void OnOrderChanged([NotNull] IOrder order)
        {
            PluginContext.Operations.AddNotificationMessage("Order changed", "My plugin", new TimeSpan(0, 0, 20));
            List<Table> tables = Table.MakeList((List<ITable>)order.Tables, order);
            //List<ITable> tables = (List<ITable>)order.Tables;
            //if (OrderStatus.New == status)
            //{
            //    // Добавили новый заказ - значит стол в заказе!
            //}
            //else
            //{
            //    return;
            //}
            //string message = "Tables ordered ";
            //foreach (ITable table in tables)
            //{
            //    message += ("[" +table.Id.ToString() + "] ");
            //}
            //MessageBox.Show(message, "", MessageBoxButton.OK);
            foreach (Table table in tables) {
                TableOrderChange.Invoke(this, table);
            }
        }

        public void Dispose()
        {
            try
            {
                subscription.Dispose();
            }
            catch (RemotingException)
            {
                // nothing to do with the lost connection
            }
        }
    }
}

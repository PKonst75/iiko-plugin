using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.SamplePlugin.NotificationHandlers;

namespace Resto.Front.Api.SamplePlugin
{
    /// <summary>
    /// Тестовый плагин для демонстрации возможностей Api.
    /// Автоматически не публикуется, для использования скопировать Resto.Front.Api.SamplePlugin.dll в Resto.Front.Main\bin\Debug\Plugins\Resto.Front.Api.SamplePlugin\
    /// </summary>
    [UsedImplicitly]
    [PluginLicenseModuleId(21005108)]
    public sealed class SamplePlugin : IFrontPlugin
    {
        private readonly Stack<IDisposable> subscriptions = new Stack<IDisposable>();

        public SamplePlugin()
        {
            PluginContext.Log.Info("Initializing SamplePlugin");
            OrderChangedHandler riser;
            ExchangeServerNamePipe sender;

            subscriptions.Push(new ButtonsTester());
            subscriptions.Push(new EditorTester());
            subscriptions.Push(riser = new OrderChangedHandler()); // Cоздаем класс, который подписывается на событие изменения стола
            subscriptions.Push(sender = new ExchangeServerNamePipe()); // Создаем сервер прослущивания сообщений

            // Пересекаем события
            riser.TableOrderChange += sender.OnSendTable;
            // добавляйте сюда другие подписчики

            PluginContext.Log.Info("SamplePlugin started");


        }

        public void Dispose()
        {
            while (subscriptions.Any())
            {
                var subscription = subscriptions.Pop();
                try
                {
                    subscription.Dispose();
                }
                catch (RemotingException)
                {
                    // nothing to do with the lost connection
                }
            }

            PluginContext.Log.Info("SamplePlugin stopped");
        }
    }
}
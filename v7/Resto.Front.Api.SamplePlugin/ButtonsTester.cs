using System;
using System.Reactive.Disposables;

namespace Resto.Front.Api.SamplePlugin
{
    using static PluginContext;

    internal sealed class ButtonsTester : IDisposable
    {
        private readonly CompositeDisposable subscriptions;

        public ButtonsTester()
        {
            subscriptions = new CompositeDisposable
            {
                 Resto.Front.Api.PluginContext.Operations.AddButtonToPluginsMenu("SamplePlugin: Message button", x => x.vm.ShowOkPopup("Sample", "Message shown from Sample plugin.")),
            };
        }

        public void Dispose()
        {
            subscriptions.Dispose();
        }

    }
}

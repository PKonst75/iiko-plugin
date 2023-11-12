using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Resto.Front.Api.SamplePlugin
{
    public class ExchangeServerNamePipe : IDisposable
    {
        private NamedPipeServerStream server_;
        StreamWriter writer_;
        public ExchangeServerNamePipe()
        {
            var windowThread = new Thread(StartServer);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        public void OnSendTable(object sender, Table e)
        {
            PluginContext.Operations.AddNotificationMessage("Sending data .....", "My plugin", new TimeSpan(0, 0, 20));
            SendTable(e);
        }

        void StartServer()
        {
            // Каждый раз после обрыва соединения
            bool first = true;
            while (true)
            {
                server_ = new NamedPipeServerStream("TestPipe");
                PluginContext.Operations.AddNotificationMessage("Waiting Connection .....", "My plugin", new TimeSpan(0, 0, 20));
                server_.WaitForConnection();
                PluginContext.Operations.AddNotificationMessage("Connection sucess", "My plugin", new TimeSpan(0, 0, 20));
                StreamReader reader = new StreamReader(server_);
                writer_ = new StreamWriter(server_);
                while (server_.IsConnected)
                {
                    // При соединении отправить список столов
                    //var line = reader.ReadLine();
                    //writer.Flush();
                    if (first)
                    {
                        // При первичном соединении отправляем список столов
                        List<Table> list = Table.MakeList();
                        foreach (var table in list)
                        {
                            string msg = PipeMessage.PipeMessageText(table);
                            writer_.WriteLine(msg);
                            writer_.Flush();
                        }
                        writer_.Flush();
                        first = false;
                    }

                    //var id = reader.ReadLine();
                    //if(id != null && id != "")
                    //{
                    //    PluginContext.Operations.AddNotificationMessage("Addin outer reserv .....", "My plugin", new TimeSpan(0, 0, 20));
                    //    Reserve.AddReserve(id);
                    //}
                    //writer_.Flush();

                }
                reader.Dispose();
                writer_.Dispose();
                first = true;
            }
        }

        public void SendTable(Table table)
        {
            
            string msg = PipeMessage.PipeMessageText(table);
            PluginContext.Operations.AddNotificationMessage(msg, "My plugin", new TimeSpan(0, 0, 20));
            writer_.WriteLine(msg);
            writer_.Flush();
        }

        public void Dispose()
        {
            server_.Disconnect();
            server_.Close();
        }
    }
}

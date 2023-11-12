using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.Windows.Controls;

namespace WpfAppIiko
{
    internal class NamePipeClient
    {
        StreamWriter? writer_;
        StreamReader? reader_;
        Queue<string> queue_ = new Queue<string> { };   // Очередь для отправки сообщений серверу
        class PipeMessage
        {
            public string? type { get; set; }
            public string? message { get; set; }
        }

        public void OnItemClick(object sender, RoutedEventArgs e)
        {
            if (sender == null || writer_ == null)
            {
                return;
            }
            string id = ((ListViewItem)sender).Tag.ToString();
            queue_.Enqueue(id);
        //    writer_.WriteLineAsync(id);
        //    writer_.FlushAsync();
        }

        public void Connect(MainWindow? wnd)
        {
            //RTable table = new() { id = Guid.NewGuid(), name = "Table 1"};

            //PipeMessage msg = new() { type = "TABLE", message = JsonSerializer.Serialize(table) };
            //msg.type = "TABLE";
            //msg.message = JsonSerializer.Serialize(table);
            //MessageBox.Show(msg.message);
            //string smsg = JsonSerializer.Serialize(msg);
            //MessageBox.Show(smsg);

            //string testmsg = @"{""type"":""TABLE"",""message"":""{\u0022id\u0022:\u0022" + table.id + @"\u0022,\u0022name\u0022:\u0022test name\u0022}""}";

            //PipeMessage msg1 = JsonSerializer.Deserialize<PipeMessage>(testmsg);
            //MessageBox.Show(msg1.type +  " = " + msg1.message );
            //RTable t1 = JsonSerializer.Deserialize<RTable>(msg1.message);
            //MessageBox.Show(t1.id + " = " + t1.name);

            //Client
            //var client = new NamedPipeClientStream("TestPipe");
            var client = new NamedPipeClientStream(".", "TestPipe", PipeDirection.InOut);
            client.Connect();
            reader_ = new StreamReader(client);
            writer_ = new StreamWriter(client);
            while (client.IsConnected)
            {
               
                //MessageBox.Show("Message Income");
                string? s = reader_.ReadLine();
                //if (queue_.Count > 0)
                //{
                //    MessageBox.Show("Try send");
                //    writer_.WriteLine(queue_.Dequeue().ToString());
                //}
               // writer_.Flush();
                if (s == null || s == "")
                {
                    continue;
                }
                // Пробуем осознать тип сообщения
                PipeMessage? msg = JsonSerializer.Deserialize<PipeMessage>(s);
                if(msg != null && msg.message != null)
                {
                    RTable? table = JsonSerializer.Deserialize<RTable>(msg.message);
                    wnd?.AddTable(table);
                }

               // MessageBox.Show(msg.type +  " = " + msg.message );
             
            }
        }
    }
}

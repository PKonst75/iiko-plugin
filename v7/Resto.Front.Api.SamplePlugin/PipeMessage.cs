using Resto.Front.Api.Data.Organization.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;


namespace Resto.Front.Api.SamplePlugin
{
    enum MessageType { UNKNOWN, TABLE};
    internal class PipeMessage
    {
        private MessageType type_;
        object data_;

        public static string PipeMessageText(Table table)
        {
            string msg = @"{""type"":""TABLE"",""message"":""{\u0022id\u0022:\u0022" + table.Id.ToString();
            msg += @"\u0022,\u0022name\u0022:\u0022" + table.Name;
            msg += @"\u0022,\u0022status\u0022:\u0022" + table.TableStatusText();
            msg += @"\u0022}""}";
            return msg;
        }

        Table GetTable()
        {
            return type_ == MessageType.TABLE?(Table)data_: null;
        }

    }

  
}

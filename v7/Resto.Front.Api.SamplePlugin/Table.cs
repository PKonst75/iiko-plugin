using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Organization.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Resto.Front.Api.SamplePlugin
{
    public enum Status { OK, DISABLED, RESERVERD, INORDER };
    public class Table
    {
        public Table(ITable table)
        {
            id_ = table.Id;
            name_ = table.Name;
            status_ = Status.OK;

            if (!table.IsActive)
            {
                status_ = Status.DISABLED;
            }
        }

        public Table(ITable table, Status status)
        {
            id_ = table.Id;
            name_ = table.Name;
            status_ = status;
        }
        private Guid id_;
        public string Id { get { return id_.ToString(); } }
        private string name_;
        private Status status_;

        public string Name
        {
            get
            {
                return name_;
            }
        }

        public Status TableStatus
        {
            get
            {
                return status_;
            }
            set
            {
                status_ = value;
            }
        }
        public string TableStatusText()
        {
           switch(status_){
                case Status.RESERVERD:
                    return "RESERVERD";
                case Status.OK:
                    return "OK";
                case Status.INORDER:
                    return "INORDER";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>
        /// Проверяем есть ли резервы на стол с данным Id и на назначенное время
        /// </summary>
        public static bool IsReserve(Guid id, DateTime time)
        {
            IReadOnlyList<IReserve> reserves = PluginContext.Operations.GetReserves();
            DateTime reserve_time = DateTime.Now;
            foreach (IReserve reserve in reserves)
            {
                foreach (ITable table in reserve.Tables)
                {
                    if (table.Id == id)
                    {
                        // Стол уже зарезервирован - пока без учеьа времени
                        // if (reserve.Status != ReserveStatus.Closed && reserve_time >= reserve.EstimatedStartTime && reserve_time <= reserve.EstimatedStartTime + reserve.Duration)
                        if (reserve.Status != ReserveStatus.Closed)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Проверяем есть ли резервы на стол с данным Id и на назначенное время
        /// </summary>
        public static bool IsOrdered(Guid id)
        {
            IReadOnlyList<IOrder> orders = PluginContext.Operations.GetOrders();
            foreach (IOrder order in orders)
            {
                foreach (ITable table in order.Tables)
                {
                    if (table.Id == id)
                    {
                        // Стол уже зарезервирован - пока без учеьа времени
                        // if (reserve.Status != ReserveStatus.Closed && reserve_time >= reserve.EstimatedStartTime && reserve_time <= reserve.EstimatedStartTime + reserve.Duration)
                        if (order.Status != OrderStatus.Deleted || order.Status != OrderStatus.Deleted)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Создаем список столов со статусами
        /// </summary>
        public static List<Table> MakeList()
        {
            var list = new List<Table>();

            IReadOnlyList<Data.Organization.Sections.ITable> tables = PluginContext.Operations.GetTables();
            foreach (ITable table in tables)
            {
                Table our_table = new Table(table); // Новый элемент - стол (Активность уже помечена)
                if(our_table.TableStatus != Status.DISABLED)
                {
                    // Ищем резервы
                    if(IsReserve(our_table.id_, DateTime.Now))
                    {
                        our_table.TableStatus = Status.RESERVERD;
                    }
                    // Ищем Заказы
                    if (IsOrdered(our_table.id_))
                    {
                        our_table.TableStatus = Status.INORDER;
                    }
                }
                list.Add(our_table);
            }
            return list;
        }

        public static List<Table> MakeList(List<ITable> tables, IOrder order)
        {
            var list = new List<Table>();
            Status status = Status.INORDER;
            if(order.Status == OrderStatus.Deleted || order.Status == OrderStatus.Closed)
            {
                status = Status.OK;
            }
            foreach (ITable table in tables)
            {
                Table our_table = new Table(table, status); // Новый элемент - с нужным статусом
                list.Add(our_table);
            }
            return list;
        }

    }
}

using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Data.Organization.Sections;
using Resto.Front.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resto.Front.Api.SamplePlugin
{
    internal class Reserve
    {
        public static void AddReserve(string id)
        {
            IOperationService os = PluginContext.Operations;
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
                if(!(table.Id.ToString() == id))
                {
                    continue; // Не тот стол
                }
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
                            return; // не можем зарезервировать
                        }
                    }
                    if (reserved)
                    {
                        return;
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
    }
}

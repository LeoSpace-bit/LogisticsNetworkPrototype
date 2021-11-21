using LogisticService.model;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace LogisticService
{
    // ПРИМЕЧАНИЕ. Можно использовать команду "Переименовать" в меню "Рефакторинг", чтобы изменить имя интерфейса "ILogisticService" в коде и файле конфигурации.
    [ServiceContract(CallbackContract = typeof(ILogisticServiceCallback))]
    public interface ILogisticService
    {
        [OperationContract] int Connect();
        [OperationContract] void Disconnect(int id);
        [OperationContract] void Query(int id, string query);
        [OperationContract] List<CargoСategory> GetCargoСategories();
        [OperationContract] City GetCity();
        [OperationContract] List<Way> GetWays();
        [OperationContract] void AddOrder(Order order, out int orderID);
        [OperationContract] void AddFullOrder(Order order);
        [OperationContract] List<RoadStatus> GetRoadStatuses(string trakingCode);
        [OperationContract] bool IsRightAnswer(string trakingCode, string phoneNumber);
        [OperationContract] void UpdateOrderStatus(string trakingCode, string status);
    }

    public interface ILogisticServiceCallback
    {
        [OperationContract(IsOneWay = true)] void QueryCallback(string answer);
    }

}

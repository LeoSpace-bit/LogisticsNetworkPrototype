using System.ServiceModel;

namespace LogisticService
{
    internal class ServerUser
    {
        public int ID { get; set; }
        public OperationContext OperationContext { get; set; }
    }
}
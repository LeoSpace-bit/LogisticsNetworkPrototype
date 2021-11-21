using System.Runtime.Serialization;

namespace LogisticService
{
    [DataContract]
    public class Order
    {
        [DataMember] public Customer Customer { get; set; }
        [DataMember] public Customer Recipient { get; set; }
        [DataMember] public Road Path { get; set; }
        [DataMember] public string CurrentState { get; set; }
        [DataMember] public int Cost { get; set; }
        [DataMember] public Department ReceivingPoint { get; set; } // FROM
        [DataMember] public Department DeliveryPoint { get; set; }  // TO
        [DataMember] public string TrackingCode { get; set; }

    }

}

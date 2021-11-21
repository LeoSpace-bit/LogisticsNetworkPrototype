using System.Runtime.Serialization;

namespace LogisticService
{
    [DataContract]
    public class Way
    {
        [DataMember] public City InitialCity { get; set; }
        [DataMember] public City DestinationCity { get; set; }
        [DataMember] public int Cost { get; set; }
        [DataMember] public string DeliveryType { get; set; }
    }

}

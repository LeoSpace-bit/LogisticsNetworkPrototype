using System.Runtime.Serialization;

namespace LogisticService
{
    [DataContract]
    public class Node
    {
        [DataMember] public int No { get; set; }
        [DataMember] public string Type { get; set; }
        [DataMember] public int Cost { get; set; }
    }

}

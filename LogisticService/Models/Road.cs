using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LogisticService
{
    [DataContract]
    public class Road
    {
        [DataMember] public List<Node> Path { get; set; } = new List<Node>();
        [DataMember] public int Cost { get; set; }

    }

}

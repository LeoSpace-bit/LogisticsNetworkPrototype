using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LogisticService
{
    [DataContract]
    public class CargoСategory
    {
        [DataMember] public string Name { get; set; }
        [DataMember] public int AddedСost { get; set; }
        [DataMember] public List<string> BlockedTypes { get; set; }
    }

}

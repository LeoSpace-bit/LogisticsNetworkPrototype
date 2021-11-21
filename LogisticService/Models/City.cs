using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LogisticService
{
    [DataContract]
    public class City
    {
        [DataMember] public int ID { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public List<Department> Departments { get; set; }
    }

}

using System.Runtime.Serialization;

namespace LogisticService
{
    [DataContract]
    public class Department
    {
        [DataMember] public string Name { get; set; }
        [DataMember] public string Address { get; set; }
    }

}

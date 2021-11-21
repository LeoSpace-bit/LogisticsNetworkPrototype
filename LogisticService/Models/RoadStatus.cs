using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LogisticService
{
    [DataContract]
    public class RoadStatus
    {
        [DataMember] public int SerialNumber { get; set; }
        [DataMember] public string InitialCityName { get; set; }
        [DataMember] public List<Department> InitialCityDepartments { get; set; }
        [DataMember] public string DestinationCity { get; set; }
        [DataMember] public List<Department> DestinationCityDepartments { get; set; }
        [DataMember] public bool VisitStatus { get; set; }
        [DataMember] public DateTime DateTimeBegin { get; set; }
        [DataMember] public DateTime DateTimeEnd { get; set; }
        [DataMember] public Department OrderBeginDepartments { get; set; }
        [DataMember] public Department OrderFinishDepartments { get; set; }
        [DataMember] public string CurrentState { get; set; }
    }

}

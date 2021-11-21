using System.Runtime.Serialization;

namespace LogisticService
{
    [DataContract]
    public class Customer
    {
        [DataMember] public string FirstName { get; set; }
        [DataMember] public string LastName { get; set; }
        [DataMember] public string Patronymic { get; set; }
        [DataMember] public Passport Passport { get; set; }
        [DataMember] public string PhoneNumber { get; set; }

    }

}

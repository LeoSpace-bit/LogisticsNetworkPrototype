using System.Runtime.Serialization;

namespace LogisticService
{
    [DataContract]
    public class Passport
    {
        [DataMember] public string Series { get; set; }
        [DataMember]  public string Number { get; set; }
    }

}

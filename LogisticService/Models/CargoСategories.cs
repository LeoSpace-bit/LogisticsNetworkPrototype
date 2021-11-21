using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LogisticService
{
    [DataContract]
    public class CargoСategories
    {
        [DataMember] private List<CargoСategory> сategories = new List<CargoСategory>();
        [DataMember] public List<CargoСategory> Сategories { get => сategories; }

        //public void AddCategory(string name, int addedСost)
        //{
        //    сategories.Add(new CargoСategory(name, addedСost));
        //}

        //public void AddCategory(string name, int addedСost, Type[] blockedTypes)
        //{
        //    сategories.Add(new CargoСategory(name, addedСost, blockedTypes));
        //}

    }
}

using System.Collections.Generic;

namespace Simulations
{
    internal class City
    {
        internal int ID { get; set; }
        internal string Name { get; set; }
        internal List<Department> Departments { get; set; }
    }
}

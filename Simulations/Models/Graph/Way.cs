using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Graph
{
    internal class Way
    {
        internal List<Node> Path { get; private set; }
        internal int Cost { get; private set; }

        internal Way(List<Node> path, int cost)
        {
            Path = path;
            Cost = cost;
        }

    }
}

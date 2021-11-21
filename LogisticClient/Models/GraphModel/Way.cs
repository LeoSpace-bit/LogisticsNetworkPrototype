using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticClient.Models.GraphModel
{
    public class Way
    {
        public List<Node> Path { get; private set; }
        public int Cost { get; private set; }

        public Way(List<Node> path, int cost)
        {
            Path = path;
            Cost = cost;
        }

    }
}

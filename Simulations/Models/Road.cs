using System.Collections.Generic;

namespace Simulations
{
    internal class Road
    {
        internal List<Node> Path { get; set; } = new List<Node>();
        internal int Cost { get; set; }

    }
}

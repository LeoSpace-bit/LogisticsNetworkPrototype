using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Type = Simulations.Type;

namespace Models.Graph
{
    internal class Node : ICloneable
    {
        internal int No { get; private set; }
        internal Type Type { get; private set; }
        internal int Cost { get; private set; }

        public Node(int number)
        {
            No = number;
        }

        public Node(int no, Type type)
        {
            No = no;
            Type = type;
        }

        public Node(int no, Type type, int cost)
        {
            No = no;
            Type = type;
            Cost = cost;
        }

        public object Clone() => new Node(No, Type, Cost);
        public override string ToString() => No.ToString();

    }
}

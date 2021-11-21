using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticClient.Models.GraphModel
{
    public class Node : ICloneable
    {
        public int No { get; private set; }
        public Type Type { get; private set; }
        public int Cost { get; private set; }

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

        public object Clone()
        {
            return new Node(No, Type, Cost);
        }

        public override string ToString()
        {
            return No.ToString();
        }

    }
}

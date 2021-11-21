using Simulations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Type = Simulations.Type;

namespace Models.Graph
{
    internal class Graph
    {
        internal List<Way> Ways { get; private set; }

        private int _verticesNumber;
        private Dictionary<int, City> _vertices;
        private List<Node>[] adjacencyList;
        private List<List<Node>> _paths { get; set; }

        internal Graph()
        {
            _paths = new List<List<Node>>();
            _vertices = new Dictionary<int, City>();
            Ways = new List<Way>();
        }

        private void InitAdjList() // utility method to initialise // adjacency list
        {
            adjacencyList = new List<Node>[_verticesNumber];

            for (int i = 0; i < _verticesNumber; i++)
            {
                adjacencyList[i] = new List<Node>();
            }
        }

        internal void AddVertex(City city)
        {
            _vertices.Add(city.ID, city);
            _verticesNumber = _vertices.Count;
            InitAdjList();
        }



        internal void AddVertex(int number, City city)
        {
            _vertices.Add(number, city);
            _verticesNumber = _vertices.Count;
            InitAdjList();
        }

        internal void SetCountVertices(int count)
        {
            _verticesNumber = count;
            InitAdjList();
        }

        internal void AddEdge(City starting, City destination, Type type, int cost)
        {
            adjacencyList[starting.ID].Add(new Node(destination.ID, type, cost));
        }

        internal void AddEdge(City starting, City destination, string type, int cost)
        {
            Type value = Type.Air;

            switch (type)
            {
                case "Land":
                    value = Type.Land;
                    break;

                case "Water":
                    value = Type.Water;
                    break;
            }

            adjacencyList[starting.ID].Add(new Node(destination.ID, value, cost));
        }

        internal void FindAllPaths(int starting, int destination, params Type[] types)
        {
            _paths.Clear();
            Ways.Clear();

            bool[] isVisited = new bool[_verticesNumber];
            List<Node> pathList = new List<Node>();

            pathList.Add(new Node(starting)); // add source to path[]

            FindAllUtilPaths(starting, destination, isVisited, pathList, types); // Call recursive utility

            PerformPreparation();
        }

        

        private void FindAllUtilPaths(int starting, int destination, bool[] isVisited, List<Node> localPathList, params Type[] types)
        {
            if (starting.Equals(destination))
            {
                _paths.Add((from Node item in localPathList select (Node)item.Clone()).ToList());
                return;
            }

            isVisited[starting] = true;
            foreach (Node node in adjacencyList[starting]) // Recur for all the vertices // adjacent to current vertex
            {
                foreach (var item in types)
                {
                    if (node.Type == item && !isVisited[node.No])
                    {
                        localPathList.Add(node); // store current node in path[]
                        FindAllUtilPaths(node.No, destination, isVisited, localPathList, types);
                        localPathList.Remove(node); // remove current node in path[]
                    }
                }

            }
            isVisited[starting] = false;
        }

        internal void PrintPathsAndCosts()
        {
            foreach (var list in _paths)
            {
                int finalCost = 0;
                string path = string.Empty;
                foreach (var item in list)
                {
                    path += GetName(item.No);
                    finalCost += item.Cost;
                }

                Debug.WriteLine(path + " | Cost: " + finalCost);

            }
        }

        internal void PerformPreparation()
        {
            for (int i = 0; i < _paths.Count; i++)
            {
                int finalCost = 0;
                string path = string.Empty;
                foreach (var item in _paths[i])
                {
                    path += GetName(item.No);
                    finalCost += item.Cost;
                }

                Ways.Add(new Way(_paths[i], finalCost));
            }
        }

        internal City GetName(int key)
        {
            return _vertices[key];
        }

        internal Way GetCheapestWay()
        {
            if (Ways.Count == 0)
            {
                return null;
            }

            int index = 0;
            int cost = Ways[0].Cost;
            for (int i = 1; i < Ways.Count; i++)
            {
                if (cost > Ways[i].Cost)
                {
                    cost = Ways[i].Cost;
                    index = i;
                }
            }

            return Ways[index];
        }

        internal Way GetShortestWay()
        {
            if (Ways.Count == 0)
            {
                return null;
            }

            int index = 0;
            int count = Ways[0].Path.Count;
            for (int i = 1; i < Ways.Count; i++)
            {
                if (count > Ways[i].Path.Count)
                {
                    count = Ways[i].Path.Count;
                    index = i;
                }
            }

            return Ways[index];
        }

    }
}

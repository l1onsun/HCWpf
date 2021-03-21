using System;
using System.Collections.Generic;
using System.Text;

namespace HCWpf
{
    class HCPoint
    {
        public readonly string Name;
        public readonly double X, Y;
        public HCPoint(string name, double x, double y)
        {
            Name = name;
            X = x;
            Y = y;
        }
        override public string ToString()
        {
            if (Name != null)
            {
                return Name;
            }
            else
            {
                string x = X.ToString();
                string y = Y.ToString();
                return $"({x},{y})";
            }
        }
    }

    class HCCluster
    {
        private static int nextClusterOrder = 0;

        public readonly List<HCPoint> Points;
        public readonly int Order;
        public HCCluster(List<HCPoint> points)
        {
            Order = nextClusterOrder;
            nextClusterOrder++;
            Points = points;
        }
        public static HCCluster FromSinglePoint(HCPoint point)
        {
            List<HCPoint> pointList = new();
            pointList.Add(point);
            return new(pointList);
        }
        public static HCCluster Join(HCCluster a, HCCluster b)
        {
            List<HCPoint> pointList = new(a.Points);
            pointList.AddRange(b.Points);
            return new(pointList);
        }
        public override string ToString()
        {
            // StringBuilder sb = new($"*{Order}{{");
            StringBuilder sb = new("{");
            foreach (HCPoint p in this.Points)
            {
                sb.Append(p.ToString());
            }
            sb.Append('}');
            return sb.ToString();
        }
        public static List<HCCluster> ClusterPerPoint(List<HCPoint> points)
        {
            List<HCCluster> clusters = new();
            foreach (HCPoint p in points)
            {
                clusters.Add(HCCluster.FromSinglePoint(p));
            }
            return clusters;
        }
        public static List<(HCCluster I, HCCluster J)> AllPairs(List<HCCluster> clusters)
        {
            List<(HCCluster I, HCCluster J)> pairs = new();
            for (var i = 0; i < clusters.Count; i++)
            {
                for (var j = i + 1; j < clusters.Count; j++)
                {
                    pairs.Add((clusters[i], clusters[j]));
                }
            }
            return pairs;
        }
        public List<(HCCluster I, HCCluster J)> PairsWith(List<HCCluster> clusters)
        {
            List<(HCCluster I, HCCluster J)> pairs = new();
            foreach (HCCluster cluster in clusters)
            {
                pairs.Add((this, cluster));
            }
            return pairs;
        }
    }

    class HCClusterPair : IComparable<HCClusterPair>
    {
        public HCCluster I;
        public HCCluster J;
        public double Distance;

        public HCClusterPair(HCCluster clusterI, HCCluster clusterJ, double distance)
        {
            I = clusterI;
            J = clusterJ;
            Distance = distance;
        }

        public int CompareTo(HCClusterPair other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }

    class HCIteration
    {
        public readonly List<HCCluster> Clusters;
        public HCClusterPair ClosestPair;

        public HCIteration(List<HCCluster> clusters, HCClusterPair closestPair)
        {
            this.Clusters = clusters;
            this.ClosestPair = closestPair;
        }
        public override string ToString()
        {
            StringBuilder sb = new($"< {ClosestPair.Distance:0.####}: ");
            sb.Append(new String(' ', Math.Max(0, 10 - sb.Length)));

            foreach (HCCluster cluster in this.Clusters)
            {
                sb.Append($"{cluster} ");
            }
            sb.Append('>');
            return sb.ToString();
        }
    }

    class HCState
    {
        public readonly List<HCIteration> Iterations;

        public HCState()
        {
            Iterations = new List<HCIteration>();
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (HCIteration i in this.Iterations)
            {
                sb.AppendLine(i.ToString());
            }
            return sb.ToString();
        }
    }

    interface IDistanceMatrix
    {
        static public (int, int) Ordered(HCCluster a, HCCluster b)
        {
            int i = Math.Min(a.Order, b.Order);
            int j = Math.Max(a.Order, b.Order);
            return (i, j);
        }
        public double GetDistance(HCCluster a, HCCluster b);
        public double FindDistance(HCCluster a, HCCluster b, Func<HCCluster, HCCluster, double> metric);
        public HCClusterPair FindClosestPair(
            IEnumerable<(HCCluster I, HCCluster J)> clusterPairs,
            Func<HCCluster, HCCluster, double> metric
        );
    }
}

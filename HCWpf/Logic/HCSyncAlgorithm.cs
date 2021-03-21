using System;
using System.Collections.Generic;

namespace HCWpf
{
    class SyncDistanceMatrix : IDistanceMatrix
    {
        private readonly Dictionary<(int, int), double> matrix;
        public SyncDistanceMatrix()
        {
            this.matrix = new();
        }
        public double GetDistance(HCCluster a, HCCluster b)
        {
            return matrix[IDistanceMatrix.Ordered(a, b)];
        }
        public double FindDistance(HCCluster a, HCCluster b, Func<HCCluster, HCCluster, double> metric)
        {
            double distance = metric(a, b);
            matrix[IDistanceMatrix.Ordered(a, b)] = distance;
            return distance;
        }
        public HCClusterPair FindClosestPair(
            IEnumerable<(HCCluster I, HCCluster J)> clusterPairs,
            Func<HCCluster, HCCluster, double> metric
        )
        {
            HCClusterPair closest = new(
                clusterI: null,
                clusterJ: null,
                distance: double.PositiveInfinity
            );

            foreach ((HCCluster I, HCCluster J) clusterPair in clusterPairs)
            {
                double distance = FindDistance(clusterPair.I, clusterPair.J, metric);

                if (distance < closest.Distance)
                {
                    (closest.I, closest.J) = clusterPair;
                    closest.Distance = distance;
                }
            }

            return closest;
        }
    }

    class HCSyncAlgorithm : HCBaseAlgorithm
    {
        public HCSyncAlgorithm() : base(new SyncDistanceMatrix()) {}
    }
}

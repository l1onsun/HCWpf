using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace HCWpf
{
    class ConcurrentDistanceMatrix : IDistanceMatrix
    {
        private readonly ConcurrentDictionary<(int, int), double> matrix;
        public ConcurrentDistanceMatrix()
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
            Func<HCCluster, HCCluster, double> distanceFunc
        )
        {
            HCClusterPair closest = clusterPairs.AsParallel().Select(clusterPair => new HCClusterPair(
                clusterI: clusterPair.I,
                clusterJ: clusterPair.J,
                distance: FindDistance(clusterPair.I, clusterPair.J, distanceFunc)
            )).Min();

            if (closest == null)
            {
                closest = new(null, null, double.PositiveInfinity);
            }
            return closest;
        }
    }

    class HCConcurrentAlgorithm : HCBaseAlgorithm
    {
        public HCConcurrentAlgorithm() : base(new ConcurrentDistanceMatrix()) {}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace HCWpf
{
    class HCBaseAlgorithm
    {

        public HCState State;
        private readonly IDistanceMatrix distanceMatrix;

        public int MinClusters = -1;
        public double DistanceLimit = -1;

        public HCBaseAlgorithm(IDistanceMatrix distanceMatrix)
        {
            this.distanceMatrix = distanceMatrix;
        }

        public void InitState(List<HCPoint> points)
        {
            State = new();

            List<HCCluster> clusters = HCCluster.ClusterPerPoint(points);
            HCClusterPair closest = distanceMatrix.FindClosestPair(
                clusterPairs: HCCluster.AllPairs(clusters), 
                metric: Metric.SingleLinkage
            );

            State.Iterations.Add(new HCIteration(
                clusters: clusters,
                closestPair: closest
            ));
        }

        public bool Step()
        {
            var prevIteration = State.Iterations.Last();
            if (prevIteration.Clusters.Count <= 1)
            {
                return false;
            }
            if (MinClusters > 0 && prevIteration.Clusters.Count <= MinClusters)
            {
                return false;
            }
            if (DistanceLimit > 0 && prevIteration.ClosestPair.Distance >= DistanceLimit)
            {
                return false;
            }
            HCCluster clusterI = prevIteration.ClosestPair.I;
            HCCluster clusterJ = prevIteration.ClosestPair.J;
            HCCluster joinedCluster = HCCluster.Join(clusterI, clusterJ);

            List<HCCluster> newClusters = new();
            foreach (HCCluster oldCluster in prevIteration.Clusters)
            {
                if (oldCluster != clusterI && oldCluster != clusterJ)
                {
                    newClusters.Add(oldCluster);
                }
            }
            var closest = distanceMatrix.FindClosestPair(
                clusterPairs: joinedCluster.PairsWith(newClusters),
                metric: (joinedCluster, other) => distanceMatrix.LanceWillamsSingleLinkage(clusterI, clusterJ, other)
            );
            newClusters.Add(joinedCluster);

            if (closest.Distance > prevIteration.ClosestPair.Distance)
            {
                closest = distanceMatrix.FindClosestPair(
                    clusterPairs: HCCluster.AllPairs(newClusters),
                    metric: distanceMatrix.GetDistance
                );
            }

            State.Iterations.Add(new HCIteration(
                clusters: newClusters,
                closestPair: closest
            ));
            return true;
        }

        public string LastIterationInfo()
        {
            return State.Iterations.Last().ToString();
        }

        public int MaxIterations()
        {
            if (State.Iterations.Count == 0)
                return 0;
            if (MinClusters <= 0)
                return State.Iterations[0].Clusters.Count;
            else
                return State.Iterations[0].Clusters.Count - MinClusters + 1;
        }


    }
}

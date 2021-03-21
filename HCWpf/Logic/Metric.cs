using System;

namespace HCWpf
{
    static class Metric
    {
        public static double Euclid(HCPoint a, HCPoint b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }
        
        public static double SingleLinkage(HCCluster a, HCCluster b)
        {
            double minDistance = double.PositiveInfinity;
            for (var i = 0; i < a.Points.Count; i++)
            {
                for (var j = 0; j < b.Points.Count; j++)
                {
                    minDistance = Math.Min(minDistance, Metric.Euclid(a.Points[i], b.Points[j]));
                }
            }
            return minDistance;
        }

        public static double LanceWillamsSingleLinkage(this IDistanceMatrix dm, HCCluster joinedA, HCCluster joinedB, HCCluster other)
        {
            return 0.5 * dm.GetDistance(joinedA, other)
                   + 0.5 * dm.GetDistance(joinedB, other)
                   - 0.5 * Math.Abs(dm.GetDistance(joinedA, other) - dm.GetDistance(joinedB, other));
        }
    }
}

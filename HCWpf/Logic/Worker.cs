using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace HCWpf
{
    class Worker
    {
        private HCBaseAlgorithm algorithm;
        private readonly BackgroundWorker backgroundWorker;
        private readonly Action<int> progressCallback;
        private Action completeCallback;
        private int maxIterations;
        private double maxDistance;

        public Worker(Action<int> progressCallback)
        {
            this.progressCallback = progressCallback;

            backgroundWorker = new()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };
            backgroundWorker.DoWork += DoWork;
            backgroundWorker.ProgressChanged += ProgressChanged;
            backgroundWorker.RunWorkerCompleted += RunWorkerCompleted;

        }

        public bool IsBusy { get => this.backgroundWorker.IsBusy; }

        public void Cancel()
        {
            this.backgroundWorker.CancelAsync();
        }

        public void Run(HCBaseAlgorithm algorithm, Action completeCallback)
        {
            this.algorithm = algorithm;
            this.completeCallback = completeCallback;
            maxIterations = algorithm.MaxIterations();
            maxDistance = algorithm.DistanceLimit;
            backgroundWorker.RunWorkerAsync();
        }
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            while (algorithm.Step())
            {

                if (backgroundWorker.CancellationPending)
                    return;
                backgroundWorker.ReportProgress(PredictProgress());

                Thread.Sleep(2000);  // emulate long computation 
            }
        }
        private int PredictProgress()
        {
            

            double predictionByIteration = (double) algorithm.State.Iterations.Count / (double) maxIterations;

            double predictionByDistance = 0;
            if (maxDistance > 0)
            {
                predictionByIteration = algorithm.State.Iterations.Last().ClosestPair.Distance / maxDistance;
            }

            Trace.WriteLine($"predictionByIteration: {predictionByIteration}");
            Trace.WriteLine($"predictionByDistance: {predictionByDistance}");

            return Convert.ToInt32(Math.Max(predictionByIteration, predictionByDistance) * 100);
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressCallback(e.ProgressPercentage);
            //Trace.WriteLine("from worker ProgressChanged:");
            //Trace.WriteLine($"ProgressPercentage: {e.ProgressPercentage}");
            //Trace.WriteLine(algorithm.LastIterationInfo());
        }

        void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            completeCallback();
        }
    }
}

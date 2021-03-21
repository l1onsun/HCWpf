using System;
using System.Collections.Generic;
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
        private List<HCPoint> data;

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

        public void Run(HCBaseAlgorithm algorithm, List<HCPoint> data, Action completeCallback)
        {
            this.algorithm = algorithm;
            this.completeCallback = completeCallback;
            this.data = data;

            maxDistance = algorithm.DistanceLimit;
            backgroundWorker.RunWorkerAsync();
        }
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorker.ReportProgress(0);
            algorithm.InitState(data);
            maxIterations = algorithm.MaxIterations();
            backgroundWorker.ReportProgress(PredictProgress());

            while (algorithm.Step())
            {

                if (backgroundWorker.CancellationPending)
                    return;
                backgroundWorker.ReportProgress(PredictProgress());

                // Thread.Sleep(2000);  // emulate long computation 
            }
        }
        private int PredictProgress()
        {
            

            double predictionByIteration = (double) algorithm.State.Iterations.Count / (double) maxIterations;

            double predictionByDistance = 0;
            if (maxDistance > 0)
            {
                predictionByDistance = algorithm.State.Iterations.Last().ClosestPair.Distance / maxDistance;
            }

            return Convert.ToInt32(Math.Max(predictionByIteration, predictionByDistance) * 100);
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressCallback(e.ProgressPercentage);
        }

        void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            completeCallback();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace HCWpf
{
    class DatasetKeeper
    {
        public readonly List<HCPoint> Points;
        public List<string> Logs;
        public Action<string> LogCallback;
        public DatasetKeeper(List<HCPoint> points)
        {
            Points = points;
            Logs = new();
        }
        public int Size { get => Points.Count; }
        public void AddLog(string log)
        {
            Logs.Add(log);
            LogCallback?.Invoke(log);
        }
    }

    class AppController
    {
        public readonly Dictionary<string, DatasetKeeper> Datasets;
        private DatasetKeeper activeDataset;
        private readonly Action<string> logCallback;
        private readonly Worker worker;

        public AppController(Action<string> logCallback, Action<int> progressCallback)
        {
            this.logCallback = logCallback;
            Datasets = new();
            worker = new(progressCallback);
        }

        public bool IsWorkerBusy { get => worker.IsBusy; }

        public string LoadDatasetDialog()
        {
            string defaultDirectory = @"..\..\..\..\ExampleDatasets";

            Microsoft.Win32.OpenFileDialog openDialog = new()
            {
                DefaultExt = ".csv",
                Filter = "CSV dataset |*.csv"
            };

            if (Directory.Exists(defaultDirectory))
            {
                Trace.WriteLine("defaultDirectory exists");
                openDialog.InitialDirectory = Path.GetFullPath(defaultDirectory);
            }

            bool? success = openDialog.ShowDialog();

            if (success == true)
            {
                HCPointsReader pointReader = new();
                pointReader.ReadCsv(openDialog.FileName);
                if (pointReader.SkipedRows > 1)
                {
                    MessageBox.Show($"Warrning. A lot of skipped rows: {pointReader.SkipedRows}.");
                }
                string datasetName = Path.GetFileName(openDialog.FileName);
                string uniqueSuffix = "";
                int uniqueNumber = 1;
                while (Datasets.ContainsKey(datasetName + uniqueSuffix))
                {
                    uniqueSuffix = $" ({uniqueNumber})";
                    uniqueNumber++;
                }
                datasetName += uniqueSuffix;
                Datasets.Add(datasetName, new DatasetKeeper(pointReader.Points));
                return datasetName;
            }
            return null;
        }

        public void Start(string algorithmType, int appliedSize, int maxClusters, double distanceLimit)
        {
            activeDataset.AddLog("Start new clustering!");
            activeDataset.AddLog("");
            HCBaseAlgorithm algorithm = new HCSyncAlgorithm();

            activeDataset.AddLog("Clustering parametes:");
            
            if (algorithmType == "Synchronus")
                algorithm = new HCSyncAlgorithm();
            else
                algorithm = new HCConcurrentAlgorithm();
            activeDataset.AddLog($"Algorithm type: {algorithmType}");

            algorithm.InitState(activeDataset.Points.GetRange(0, appliedSize));
            activeDataset.AddLog($"Points count: {appliedSize}/{activeDataset.Size}");

            if (maxClusters > 0)
            {
                algorithm.MaxClusters = maxClusters;
                activeDataset.AddLog($"Maximum clusters: {maxClusters}");
            }
            if (distanceLimit > 0)
            {
                algorithm.DistanceLimit = distanceLimit;
                activeDataset.AddLog($"Distance limit: {distanceLimit}");
            }
            activeDataset.AddLog("");
            activeDataset.AddLog("Start background worker");

            worker.Run(algorithm, (int x) => { });

            activeDataset.AddLog("");
        }

        public void SetActiveDataset(DatasetKeeper newActiveDataset)
        {
            if (activeDataset != null)
            {
                activeDataset.LogCallback = null;
            }
            activeDataset = newActiveDataset;
            activeDataset.LogCallback = logCallback;
        }
    }

    class Worker
    {
        private HCBaseAlgorithm algorithm;
        private readonly BackgroundWorker backgroundWorker;
        private readonly Action<int> progressCallback;
        private Action<int> completeCallback;
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

        public void Run(HCBaseAlgorithm algorithm, Action<int> completeCallback)
        {
            this.algorithm = algorithm;
            this.completeCallback = completeCallback;
            maxIterations = algorithm.MaxIterations();
            if (algorithm.DistanceLimit > 0)
            {
                maxDistance = algorithm.DistanceLimit;
            }
            else
            {
                maxDistance = double.PositiveInfinity;
            }
            backgroundWorker.RunWorkerAsync();
        }
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            while (algorithm.Step())
            {
                if (backgroundWorker.CancellationPending)
                    return;
                backgroundWorker.ReportProgress(PredictProgress());
            }
        }
        private int PredictProgress()
        {
            double predictionByIteration = algorithm.State.Iterations.Count / maxIterations;
            double predictionByDistance = algorithm.State.Iterations.Last().ClosestPair.Distance / maxDistance;

            return (int)Math.Max(predictionByIteration, predictionByDistance) * 100;
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressCallback(e.ProgressPercentage);
            Trace.WriteLine("from worker ProgressChanged:");
            Trace.WriteLine(algorithm.LastIterationInfo());
        }

        void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show(algorithm.LastIterationInfo());
        }
    }
}

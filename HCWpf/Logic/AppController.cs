using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly Action<int> progressCallback;
        private readonly Worker worker;

        public AppController(Action<string> logCallback, Action<int> progressCallback)
        {
            this.logCallback = logCallback;
            this.progressCallback = progressCallback;
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

        public void StopWorker()
        {
            if (IsWorkerBusy)
            {
                worker.Cancel();
            }
        }

        public void StartWorker(string algorithmType, int appliedSize, int minClusters, double distanceLimit)
        {
            activeDataset.AddLog("Start new clustering!");
            activeDataset.AddLog("");

            HCBaseAlgorithm algorithm;

            activeDataset.AddLog("Clustering parametes:");
            
            if (algorithmType == "Synchronus")
                algorithm = new HCSyncAlgorithm();
            else
                algorithm = new HCConcurrentAlgorithm();
            activeDataset.AddLog($"Algorithm type: {algorithmType}");

            algorithm.InitState(activeDataset.Points.GetRange(0, appliedSize));
            activeDataset.AddLog($"Points count: {appliedSize}/{activeDataset.Size}");

            if (minClusters > 0)
            {
                algorithm.MinClusters = minClusters;
                activeDataset.AddLog($"Minimum clusters: {minClusters}");
            }
            if (distanceLimit > 0)
            {
                algorithm.DistanceLimit = distanceLimit;
                activeDataset.AddLog($"Distance limit: {distanceLimit}");
            }
            activeDataset.AddLog("");
            activeDataset.AddLog("Start background worker");
            activeDataset.AddLog("");

            worker.Run(algorithm, 
                completeCallback: () => {
                    foreach (HCIteration i in algorithm.State.Iterations)
                    {
                        activeDataset.AddLog(i.ToString());
                    }
                    activeDataset.AddLog("");
                    activeDataset.AddLog("");
                    progressCallback(-1);
                }
            );

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
}

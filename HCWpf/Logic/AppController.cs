using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public AppController(Action<string> logCallback)
        {
            this.logCallback = logCallback;
            Datasets = new();
        }

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

        public void Start()
        {
            activeDataset.AddLog($"points number: {activeDataset.Size}");
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

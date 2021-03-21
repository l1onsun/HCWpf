using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HCWpf
{

    public partial class MainWindow : Window
    {
        private readonly AppController appController;
        private readonly FontFamily logFontFamily = new("Courier New");

        public MainWindow()
        {
            appController = new(logCallback: AddLog, progressCallback: UpdateProggres);
            InitializeComponent();
        }

        private DatasetKeeper SelectedDataset { 
            get => appController.Datasets[chooseDataset.SelectedItem.ToString()]; 
        }
        public int DatasetSize
        {
            get {
                if (chooseDataset.SelectedItem != null)
                    return SelectedDataset.Size;
                else
                    return 0;
            }
        }
        public int AppliedSize
        {
            get => (int) sliderAppliedSize.Value;
            set
            {
                sliderAppliedSize.Value = value;
            }
        }
        public int MaxClustets { get; set; }
        public double DistanceLimit { get; set; }


        private void ButtonLoadDataset_Click(object sender, RoutedEventArgs e)
        {
            string datasetName = appController.LoadDatasetDialog();
            if (datasetName != null)
            {
                chooseDataset.Items.Add(datasetName);
                chooseDataset.SelectedIndex = chooseDataset.Items.IndexOf(datasetName);
                UpdateStartStopButtons();
            }
        }

        private void ChooseDataset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            textDatasetSize.Text = DatasetSize.ToString();

            SliderRescale();

            ReplaceLogs(SelectedDataset.Logs);
            appController.SetActiveDataset(SelectedDataset);

            // buttonStart.IsEnabled = // ToDo: deside how to 
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.IsEnabled = false;
            appController.Start(
                algorithmType: ((ComboBoxItem) chooseAlgorithm.SelectedItem).Content.ToString() ,
                appliedSize: AppliedSize,
                maxClusters: MaxClustets,
                distanceLimit: DistanceLimit
            );
        }

        private void AddLog(string log)
        {
            logStackPanel.Children.Add(
                new TextBlock { 
                    Text = log,
                    FontFamily = logFontFamily,
                }
            );
        }

        private void ReplaceLogs(List<string> newLogs)
        {
            logStackPanel.Children.Clear();
            foreach (string log in newLogs)
            {
                AddLog(log);
            }
        }

        private void SliderRescale()
        {
            bool shouldScaleSlider = false;
            if (sliderAppliedSize.Value == sliderAppliedSize.Maximum)
            {
                shouldScaleSlider = true;
            }
            sliderAppliedSize.Maximum = DatasetSize;
            if (shouldScaleSlider)
            {
                sliderAppliedSize.Value = DatasetSize;
            }
        }

        private void UpdateProggres(int value)
        {
            Trace.WriteLine($"...{value}");
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = value;
            UpdateStartStopButtons();
        }

        private void UpdateStartStopButtons()
        {
            buttonStart.IsEnabled = !appController.IsWorkerBusy;
            buttonStop.IsEnabled = appController.IsWorkerBusy;
        }
    }
}

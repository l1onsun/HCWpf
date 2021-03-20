using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace HCWpf
{

    public partial class MainWindow : Window
    {
        private readonly AppController appController;

        public MainWindow()
        {
            appController = new(logCallback: AddLog);
            InitializeComponent();
            buttonStart.IsEnabled = true; // delete this line
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
            get => (int)sliderAppliedSize.Value;
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
                buttonStart.IsEnabled = true;
            }
        }

        private void ChooseDataset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            textDatasetSize.Text = DatasetSize.ToString();

            SliderRescale();

            ReplaceLogs(SelectedDataset.Logs);
            appController.SetActiveDataset(SelectedDataset);
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            appController.Start();
        }

        private void AddLog(string log)
        {
            logStackPanel.Children.Add(new TextBlock { Text = log });
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
    }
}

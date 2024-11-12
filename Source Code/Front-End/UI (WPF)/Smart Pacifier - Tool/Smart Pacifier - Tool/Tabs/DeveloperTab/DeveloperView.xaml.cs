﻿using SmartPacifier.Interface.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SmartPacifier.BackEnd.DatabaseLayer.InfluxDB.DataManipulation;

namespace Smart_Pacifier___Tool.Tabs.DeveloperTab
{
    public partial class DeveloperView : UserControl
    {
        private readonly IDatabaseService _databaseService;
        private readonly IManagerPacifiers _managerPacifiers;
        private readonly IManagerCampaign _managerCampaign;
        private readonly IDataManipulationHandler _dataManipulationHandler; // Added field

        private DataTable allData = new DataTable();
        private int currentPage = 1;
        private int pageSize = 10;

        public DeveloperView(IDatabaseService databaseService, IManagerCampaign managerCampaign, IManagerPacifiers managerPacifiers)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _managerPacifiers = managerPacifiers;
            _managerCampaign = managerCampaign;

            _dataManipulationHandler = new DataManipulationHandler(_databaseService); // Initialize handler

            _ = LoadDataAsync();  // Call the async method but don't await, suppressing CS4014
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Load all sensor data from the database (without filtering by campaign)
                allData.Clear();
                allData = await _databaseService.GetSensorDataAsync();

                // Extract unique values for Campaign, Pacifier, and Sensor Type for combo boxes
                var uniqueCampaigns = allData.AsEnumerable()
                    .Select(row => row["Campaign Name"].ToString())
                    .Distinct()
                    .ToList();

                var uniquePacifiers = allData.AsEnumerable()
                    .Select(row => row["Pacifier Name"].ToString())
                    .Distinct()
                    .ToList();

                var uniqueSensors = allData.AsEnumerable()
                    .Select(row => row["Sensor Type"].ToString())
                    .Distinct()
                    .ToList();

                // Populate the combo boxes with unique values
                Campaign.ItemsSource = uniqueCampaigns;
                Pacifier.ItemsSource = uniquePacifiers;
                Sensor.ItemsSource = uniqueSensors;

                // Display all data initially
                DisplayData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }


        private void DisplayData()
        {
            if (allData.Rows.Count == 0)
            {
                MessageBox.Show("No data to display.");
                return;
            }

            // Display all data in the DataListView without pagination
            DataListView.ItemsSource = allData.DefaultView;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedCampaign = Campaign.SelectedItem?.ToString();
            string selectedPacifier = Pacifier.SelectedItem?.ToString();
            string selectedSensorType = Sensor.SelectedItem?.ToString();

            var filteredData = allData.AsEnumerable().Where(row =>
                (string.IsNullOrEmpty(selectedCampaign) || row["Campaign Name"].ToString() == selectedCampaign) &&
                (string.IsNullOrEmpty(selectedPacifier) || row["Pacifier Name"].ToString() == selectedPacifier) &&
                (string.IsNullOrEmpty(selectedSensorType) || row["Sensor Type"].ToString() == selectedSensorType));

            if (filteredData.Any())
            {
                DataTable filteredTable = filteredData.CopyToDataTable();
                DataListView.ItemsSource = filteredTable.DefaultView;
            }
            else
            {
                DataListView.ItemsSource = null;
                MessageBox.Show("No data matches the selected filters.");
            }
        }



        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                DisplayData();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * pageSize < allData.Rows.Count)
            {
                currentPage++;
                DisplayData();
            }
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == Campaign || sender == Pacifier || sender == Sensor)
            {
                ApplyButton_Click(sender, e);
            }
        }



        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement your add functionality here
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataListView.SelectedItem is DataRowView selectedRow)
            {
                // Extract the selected row data into a dictionary
                var originalData = selectedRow.Row
                    .Table.Columns.Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => selectedRow.Row[col] ?? string.Empty); // Ensure no null values

                // Create an instance of the EditDataWindow and pass the data
                var editWindow = new EditDataWindow(_dataManipulationHandler, originalData);

                // Show the EditDataWindow as a dialog and refresh data if changes were saved
                if (editWindow.ShowDialog() == true)
                {
                    await LoadDataAsync();
                }
            }
            else
            {
                MessageBox.Show("Please select a row to edit.");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure there are selected rows in the DataListView
            var selectedRows = DataListView.SelectedItems.OfType<DataRowView>().ToList();

            if (selectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.");
                return;
            }

            try
            {
                foreach (var row in selectedRows)
                {
                    // Retrieve the unique Entry ID for the selected row
                    if (row["Entry ID"] is int entryId)
                    {
                        // Remove the entry from the real database based on the Entry ID
                        await DeleteEntryFromDatabaseAsync(entryId);

                        // Remove the entry from the DataTable (UI data)
                        allData.Rows.Remove(row.Row);
                    }
                }

                // Refresh the DataListView after deletion
                DataListView.ItemsSource = allData.DefaultView;
                MessageBox.Show("Selected entries deleted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting entries: {ex.Message}");
            }
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Delete functionality not implemented for direct database rows.");
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in DataListView.Items)
            {
                var listViewItem = DataListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (listViewItem != null)
                {
                    listViewItem.IsSelected = true;
                }
            }
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in DataListView.Items)
            {
                var listViewItem = DataListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (listViewItem != null)
                {
                    listViewItem.IsSelected = false;
                }
            }
        }
    }
}

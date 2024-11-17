using SmartPacifier.BackEnd.DatabaseLayer.InfluxDB.DataManipulation;
using SmartPacifier.Interface.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Smart_Pacifier___Tool.Tabs.DeveloperTab
{
    public partial class DeveloperView : UserControl
    {
        private readonly IDatabaseService _databaseService;
        private readonly IManagerCampaign _managerCampaign;
        private readonly IManagerPacifiers _managerPacifiers;
        private readonly IDataManipulationHandler _dataManipulationHandler;
        private DataTable allData = new DataTable();

        public DeveloperView(IDatabaseService databaseService, IManagerCampaign managerCampaign, IManagerPacifiers managerPacifiers)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _managerCampaign = managerCampaign;
            _managerPacifiers = managerPacifiers;

            _dataManipulationHandler = new DataManipulationHandler(_databaseService);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Fetch data from the database
                allData = await _databaseService.GetSensorDataAsync();

                if (allData == null || allData.Rows.Count == 0)
                {
                    MessageBox.Show("No data found to display.");
                    return;
                }

                // Generate columns dynamically
                GenerateColumns(allData);

                // Bind data to the ListView
                DataListView.ItemsSource = allData.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private void GenerateColumns(DataTable dataTable)
        {
            // Clear existing columns
            DynamicGridView.Columns.Clear();

            // Dynamically add columns based on the DataTable
            foreach (DataColumn column in dataTable.Columns)
            {
                var gridViewColumn = new GridViewColumn
                {
                    Header = column.ColumnName,
                    DisplayMemberBinding = new Binding($"[{column.ColumnName}]")
                };
                DynamicGridView.Columns.Add(gridViewColumn);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyButton_Click(sender, e);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string? selectedCampaign = Campaign.SelectedItem?.ToString();
            string? selectedPacifier = Pacifier.SelectedItem?.ToString();
            string? selectedSensor = Sensor.SelectedItem?.ToString();

            // Filter data based on selections
            var filteredData = allData.AsEnumerable().Where(row =>
                (string.IsNullOrEmpty(selectedCampaign) || row["campaign_name"].ToString() == selectedCampaign) &&
                (string.IsNullOrEmpty(selectedPacifier) || row["pacifier_name"].ToString() == selectedPacifier) &&
                (string.IsNullOrEmpty(selectedSensor) || row["sensor_type"].ToString() == selectedSensor));

            DataListView.ItemsSource = filteredData.Any()
                ? filteredData.CopyToDataTable().DefaultView
                : null;

            if (!filteredData.Any())
            {
                MessageBox.Show("No data matches the selected filters.");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataListView.SelectedItem is DataRowView selectedRow)
            {
                try
                {
                    int entryId = Convert.ToInt32(selectedRow["entry_id"]);
                    string measurement = selectedRow["Measurement"].ToString();

                    // Delete entry
                    await _databaseService.DeleteEntryFromDatabaseAsync(entryId, measurement);
                    MessageBox.Show("Entry deleted successfully.");
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting entry: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select an entry to delete.");
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataListView.SelectedItem is DataRowView selectedRow)
            {
                // Extract original data from the selected row
                var originalData = selectedRow.Row.Table.Columns
                    .Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => selectedRow.Row[col]);

                // Open the Edit Data Window
                var editWindow = new EditDataWindow(_dataManipulationHandler, originalData);
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for Add functionality
            MessageBox.Show("Add functionality not yet implemented.");
        }
    }
}

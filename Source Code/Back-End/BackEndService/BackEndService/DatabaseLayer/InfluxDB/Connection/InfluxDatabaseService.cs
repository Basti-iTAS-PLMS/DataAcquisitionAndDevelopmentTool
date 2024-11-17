﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Protos;
using SmartPacifier.Interface.Services;
using System.Data;
using System.Globalization;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Configurations;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

namespace SmartPacifier.BackEnd.Database.InfluxDB.Connection
{
    public class InfluxDatabaseService : IDatabaseService
    {
        private readonly InfluxDBClient _client;
        private readonly string _bucket = "SmartPacifier-Bucket1";
        private readonly string _org = "thu-de";
        private readonly string _token;
        private readonly string _baseUrl;
        public InfluxDatabaseService(InfluxDBClient client, string token, string baseUrl, string org)
        {
            _client = client;
            _token = token;
            _baseUrl = baseUrl;
            _org = org;

        }

        public InfluxDBClient GetClient() => _client;

        public string Bucket => _bucket;
        public string Org => _org;
        public string Token => _token; // Implement Token
        public string BaseUrl => _baseUrl; // Implement BaseUrl

        public async Task WriteDataAsync(string measurement, Dictionary<string, object> fields, Dictionary<string, string> tags)
        {
            try
            {
                var point = PointData.Measurement(measurement)
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                foreach (var tag in tags)
                {
                    point = point.Tag(tag.Key, tag.Value);
                }

                foreach (var field in fields)
                {
                    if (field.Value is float)
                        point = point.Field(field.Key, (float)field.Value);
                    else if (field.Value is double)
                        point = point.Field(field.Key, (double)field.Value);
                    else if (field.Value is int)
                        point = point.Field(field.Key, (int)field.Value);
                    else if (field.Value is string)
                        point = point.Field(field.Key, (string)field.Value);
                }

                var writeApi = _client.GetWriteApiAsync();
                await writeApi.WritePointAsync(point, _bucket, _org);
            }
            catch (UnauthorizedException)
            {
                MessageBox.Show("Unauthorized access to InfluxDB. Please check the API key and permissions.", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<List<string>> ReadData(string query)
        {
            var queryApi = _client.GetQueryApi();
            var tables = await queryApi.QueryAsync(query, _org);
            var records = new List<string>();

            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    var recordData = new Dictionary<string, object>();

                    foreach (var key in record.Values.Keys)
                    {
                        var value = record.GetValueByKey(key);
                        if (key == "_time" && value is NodaTime.Instant instant)
                        {
                            // Convert NodaTime.Instant to a string format
                            recordData[key] = instant.ToDateTimeOffset().ToString("o");
                        }
                        else
                        {
                            recordData[key] = value;
                        }
                    }

                    // Convert the record dictionary to JSON
                    var json = System.Text.Json.JsonSerializer.Serialize(recordData);
                    records.Add(json);
                }
            }

            // Optional: Show message box to verify the JSON records
            //System.Windows.MessageBox.Show(string.Join("\n\n", records), "Raw JSON Data", MessageBoxButton.OK, MessageBoxImage.Information);

            return records;
        }


        public async Task<List<string>> GetCampaignsAsync()
        {
            var campaigns = new List<string>();
            var fluxQuery = $"from(bucket: \"{_bucket}\") |> range(start: -30d) |> keep(columns: [\"campaign_name\"]) |> distinct(column: \"campaign_name\")";

            var queryApi = _client.GetQueryApi();
            var tables = await queryApi.QueryAsync(fluxQuery, _org);

            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    var campaignName = record.GetValueByKey("campaign_name")?.ToString();
                    if (!string.IsNullOrEmpty(campaignName))
                    {
                        campaigns.Add(campaignName);
                    }
                }
            }

            return campaigns;
        }



        public async Task<DataTable> GetSensorDataAsync()
        {
            string fluxQuery = @"
        from(bucket: ""SmartPacifier-Bucket1"")
        |> range(start: 0)
        |> filter(fn: (r) => r._measurement == ""campaigns"" or r._measurement == ""campaign_metadata"")
        |> pivot(rowKey: [""_time""], columnKey: [""_field""], valueColumn: ""_value"")
    ";

            DataTable dataTable = new DataTable();

            try
            {
                var queryApi = _client.GetQueryApi();
                var tables = await queryApi.QueryAsync(fluxQuery, _org);

                // Dynamically build DataTable columns based on keys in records
                foreach (var table in tables)
                {
                    foreach (var record in table.Records)
                    {
                        // Add columns dynamically from record keys
                        foreach (var key in record.Values.Keys)
                        {
                            if (!dataTable.Columns.Contains(key))
                            {
                                dataTable.Columns.Add(key);
                            }
                        }
                    }
                }

                // Populate rows with record values
                foreach (var table in tables)
                {
                    foreach (var record in table.Records)
                    {
                        DataRow row = dataTable.NewRow();

                        foreach (var key in record.Values.Keys)
                        {
                            var value = record.GetValueByKey(key);

                            // Assign value to the appropriate column, handle nulls
                            row[key] = value ?? DBNull.Value;
                        }

                        dataTable.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching sensor data: {ex.Message}");
                throw;
            }

            return dataTable;
        }




        // Corrected deletion function in C#
        public async Task DeleteEntryFromDatabaseAsync(int entryId, string measurement)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Set up the headers
                    client.DefaultRequestHeaders.Add("Authorization", $"Token {_token}");
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    // Define the start and stop times for deletion
                    string start = "1970-01-01T00:00:00Z";
                    string stop = DateTime.UtcNow.ToString("o");

                    // Create a dynamic predicate based on measurement type
                    string predicate;
                    if (measurement == "campaign_metadata")
                    {
                        // Only use entry_id for campaign_metadata
                        predicate = $"_measurement=\"campaign_metadata\" AND entry_id=\"{entryId}\"";
                    }
                    else if (measurement == "campaigns")
                    {
                        // Use both measurement and entry_id for campaigns
                        predicate = $"_measurement=\"campaigns\" AND entry_id=\"{entryId}\"";
                    }
                    else
                    {
                        MessageBox.Show("Unknown measurement type specified.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Create the JSON payload for the delete request
                    var deleteRequest = new
                    {
                        start = start,
                        stop = stop,
                        predicate = predicate
                    };

                    var jsonContent = JsonConvert.SerializeObject(deleteRequest);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Send the DELETE request to the InfluxDB API
                    var response = await client.PostAsync($"{_baseUrl}/api/v2/delete?org={_org}&bucket={_bucket}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        //MessageBox.Show("Entry deleted successfully from the database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error deleting entry: {response.ReasonPhrase} - {errorContent}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting entry from database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public async Task<Dictionary<string, object>> GetCampaignDataAlgorithmLayerAsync(string campaignName)
        {
            var campaignData = new Dictionary<string, object>
    {
        { "campaignName", campaignName },
        { "pacifiers", new List<Dictionary<string, object>>() }
    };

            var fluxQuery = $@"
        from(bucket: ""{_bucket}"")
        |> range(start: -30d)
        |> filter(fn: (r) => r._measurement == ""campaigns"" and r.campaign_name == ""{campaignName}"")
        |> pivot(rowKey: [""_time""], columnKey: [""_field""], valueColumn: ""_value"")
    ";

            var queryApi = _client.GetQueryApi();
            var tables = await queryApi.QueryAsync(fluxQuery, _org);

            var pacifiers = (List<Dictionary<string, object>>)campaignData["pacifiers"];

            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    // Extract pacifier and sensor data from the record
                    string pacifierName = record.GetValueByKey("pacifier_name")?.ToString() ?? "Unknown";
                    string sensorType = record.GetValueByKey("sensor_type")?.ToString() ?? "Unknown";

                    // Find existing pacifier data or create a new entry
                    var pacifierData = pacifiers.FirstOrDefault(p => (string)p["name"] == pacifierName);
                    if (pacifierData == null)
                    {
                        pacifierData = new Dictionary<string, object>
                {
                    { "name", pacifierName },
                    { "sensors", new List<Dictionary<string, object>>() }
                };
                        pacifiers.Add(pacifierData);
                    }

                    // Prepare sensor data
                    var sensorData = new Dictionary<string, object>
            {
                { "type", sensorType },
                { "data", new Dictionary<string, object>() }
            };

                    var sensorValues = (Dictionary<string, object>)sensorData["data"];
                    foreach (var key in record.Values.Keys)
                    {
                        // Avoid InfluxDB metadata fields and known tags
                        if (!key.StartsWith("_") && key != "campaign_name" && key != "pacifier_name" && key != "sensor_type")
                        {
                            sensorValues[key] = record.GetValueByKey(key);
                        }
                    }

                    // Add sensor data to the pacifier
                    ((List<Dictionary<string, object>>)pacifierData["sensors"]).Add(sensorData);
                }
            }

            // Debug: Output the retrieved data for inspection
            var debugOutput = new System.Text.StringBuilder();
            debugOutput.AppendLine($"Campaign Name: {campaignData["campaignName"]}");
            debugOutput.AppendLine("Pacifiers:");

            foreach (var pacifier in pacifiers)
            {
                debugOutput.AppendLine($"  Pacifier Name: {pacifier["name"]}");
                var sensors = (List<Dictionary<string, object>>)pacifier["sensors"];
                foreach (var sensor in sensors)
                {
                    debugOutput.AppendLine($"    Sensor Type: {sensor["type"]}");
                    var data = (Dictionary<string, object>)sensor["data"];
                    foreach (var key in data.Keys)
                    {
                        debugOutput.AppendLine($"      {key}: {data[key]}");
                    }
                }
            }

            MessageBox.Show(debugOutput.ToString(), "Campaign Data Debug Info");

            return campaignData;
        }



    }
}



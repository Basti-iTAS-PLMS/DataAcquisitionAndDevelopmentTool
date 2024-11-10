using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using InfluxDB.Client;
using SmartPacifier.Interface.Services;

namespace SmartPacifier.BackEnd.DatabaseLayer.InfluxDB.Connection
{
    public class DatabaseServiceSwitcher : IDatabaseServiceSwitcher
    {
        private readonly IDatabaseService _localDatabaseService;
        private readonly IDatabaseService _serverDatabaseService;
        private IDatabaseService _currentDatabaseService;

        public DatabaseServiceSwitcher(IDatabaseService localDatabaseService, IDatabaseService serverDatabaseService, bool useLocal)
        {
            _localDatabaseService = localDatabaseService;
            _serverDatabaseService = serverDatabaseService;
            _currentDatabaseService = useLocal ? _localDatabaseService : _serverDatabaseService;
        }

        public void SwitchToLocal()
        {
            _currentDatabaseService = _localDatabaseService;
        }

        public void SwitchToServer()
        {
            _currentDatabaseService = _serverDatabaseService;
        }

        // Proxy methods to the current database service
        public InfluxDBClient GetClient() => _currentDatabaseService.GetClient();
        public string Bucket => _currentDatabaseService.Bucket;
        public string Org => _currentDatabaseService.Org;
        public string Token => _currentDatabaseService.Token;
        public string BaseUrl => _currentDatabaseService.BaseUrl;

        public Task WriteDataAsync(string measurement, Dictionary<string, object> fields, Dictionary<string, string> tags) =>
            _currentDatabaseService.WriteDataAsync(measurement, fields, tags);

        public Task<List<string>> ReadData(string query) =>
            _currentDatabaseService.ReadData(query);

        public Task<List<string>> GetCampaignsAsync() =>
            _currentDatabaseService.GetCampaignsAsync();

        public Task<DataTable> GetSensorDataAsync() =>
            _currentDatabaseService.GetSensorDataAsync();


        public IDatabaseService GetCurrentDatabaseService()
        {
            return _currentDatabaseService;
        }
    }
}

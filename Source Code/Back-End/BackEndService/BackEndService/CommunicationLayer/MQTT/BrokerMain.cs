﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows; // For MessageBox
using SmartPacifier.Interface.Services;
using System.IO;
namespace SmartPacifier.BackEnd.CommunicationLayer.MQTT
{
    public class BrokerMain : IBrokerMain
    {
        private static bool isBrokerRunning = false; // Static flag to prevent duplicate starts

        public async Task StartAsync(string[] args)
        {
            StringBuilder debugLog = new StringBuilder();
            debugLog.AppendLine("Starting Broker in a new console window...");

            if (!isBrokerRunning)
            {
                bool brokerStarted = await Task.Run(() => StartBrokerConsoleApp(debugLog));
                isBrokerRunning = brokerStarted; // Set the flag if broker started successfully

                if (brokerStarted)
                {
                    debugLog.AppendLine("Broker started successfully.");
                }
                else
                {
                    debugLog.AppendLine("Failed to start Broker.");
                }
            }
            else
            {
                debugLog.AppendLine("Broker is already running. Skipping duplicate start.");
            }

            MessageBox.Show(debugLog.ToString(), "Broker Debug Log", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private bool StartBrokerConsoleApp(StringBuilder debugLog)
        {
            try
            {
                // Path to the DLL file of BrokerConsoleApp
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var dllName = "BrokerConsoleApp.dll";
                var pathToDll = Path.Combine(baseDirectory, dllName);

                // Check if the file exists before attempting to run
                if (!File.Exists(pathToDll))
                {
                    debugLog.AppendLine($"Error: DLL file not found at path: {pathToDll}");
                    return false;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{pathToDll}\"", // Run the DLL directly
                    CreateNoWindow = false, // Show the console window
                    UseShellExecute = true // Allows the console window to remain open
                };

                var process = Process.Start(startInfo);
                debugLog.AppendLine("Console window for Broker attempted to start.");

                // Check if process started successfully
                if (process == null || process.HasExited)
                {
                    debugLog.AppendLine("Error: Process failed to start or exited immediately.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                debugLog.AppendLine($"Error starting Broker: {ex.Message}");
                return false;
            }
        }


    }
}

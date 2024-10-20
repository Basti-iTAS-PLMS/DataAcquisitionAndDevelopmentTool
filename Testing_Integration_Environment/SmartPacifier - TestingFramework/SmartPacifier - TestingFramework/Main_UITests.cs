﻿using System;
using System.Diagnostics;
using FlaUI.Core;
using FlaUI.UIA3;
using Xunit;
using SmartPacifier___TestingFramework.Tests_FrontEnd.Tests_Tabs.Tests_SettingTab;
using SmartPacifier_UITests;

namespace SmartPacifier___TestingFramework
{
    public class Main_UITests
    {
        private Application? app;

        /// <summary>
        /// This function is used to launch the application for all UI Tests.
        /// </summary>
        /// <returns></returns>
        private Application LaunchApplication()
        {
            string applicationPath = @"C:\programming\TeamOrientedProject---Smart-Pacifier\Source Code\Front-End\UI (WPF)\Smart Pacifier - Tool\Smart Pacifier - Tool\bin\Debug\net8.0-windows\SmartPacifier.UI (WPF).exe";
            var processStartInfo = new ProcessStartInfo(applicationPath)
            {
                WorkingDirectory = @"C:\programming\TeamOrientedProject---Smart-Pacifier\Source Code\Front-End\UI (WPF)\Smart Pacifier - Tool\Smart Pacifier - Tool\bin\Debug\net8.0-windows\"
            };

            app = Application.Launch(processStartInfo);
            return app;
        }

        /// <summary>
        /// Manually run the CheckButtonsExistenceInSettingsTab test from the UITests_Settings class.
        /// </summary>
        [Fact]
        public void RunSettingsTabTests()
        {
            app = LaunchApplication();
            try
            {
                var settingsTests = new UITests_Settings(app);

                // Assert each function returns true
                Assert.True(settingsTests.CheckButtonsExistenceInSettingsTab(), "Buttons existence test failed.");
                Assert.True(settingsTests.CheckTextBlocksExistenceAndBehaviorInSettingsTab(), "Text blocks existence and behavior test failed.");
                Assert.True(settingsTests.CheckCheckBoxesExistenceInSettingsTab(), "Check boxes existence test failed.");
            }
            finally
            {
                app.Close();
            }
        }



        /// <summary>
        /// Runs the PIN validation tests for the Developer tab.
        /// </summary>
        [Fact]
        public void RunPinValidationTests()
        {
            app = LaunchApplication();
            try
            {
                // Pass the launched application to the PinValidationTests class
                var pinValidationTests = new PINValidationTests(app);
                pinValidationTests.ValidateDeveloperTabActivation_WithCorrectPin();  // Call the function
            }
            finally
            {
                app.Close();
            }
        }


    }
}

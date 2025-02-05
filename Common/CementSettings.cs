using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Common
{
    public class CementSettings
    {
        // ReSharper disable UnassignedField.Global
        public string UserName;
        public string Domain;
        public string Password;
        public string EncryptedPassword;
        public string DefaultMsBuildVersion;
        public string CementServer;
        public bool KillMsBuild = true;
        public string SelfUpdateTreeish;
        public List<Package> Packages;
        public int? MaxDegreeOfParallelism;
        public bool? IsEnabledSelfUpdate;

        public Dictionary<string, string> UserCommands;
        // ReSharper restore UnassignedField.Global

        public void Save()
        {
            var text = JsonConvert.SerializeObject(this, Formatting.Indented);
            var path = Path.Combine(Helper.GetGlobalCementDirectory(), "settings");
            Helper.CreateFileAndDirectory(path, text);
        }

        private static CementSettings GetDefaultSettings()
        {
            var path = Path.Combine(Helper.GetCementInstallDirectory(), "dotnet", "defaultSettings.json");
            if (!File.Exists(path))
                ConsoleWriter.WriteError($"{path} not found");
            return ReadSettings(path);
        }

        public static CementSettings Get()
        {
            var path = Path.Combine(Helper.GetGlobalCementDirectory(), "settings");
            var defaultSettings = GetDefaultSettings();

            var settings = ReadSettings(path);
            settings = settings ?? defaultSettings;

            settings.Packages = settings.Packages ?? defaultSettings.Packages;
            foreach (var package in defaultSettings.Packages)
            {
                if (settings.Packages.All(p => p.Name != package.Name))
                    settings.Packages.Add(package);
            }

            settings.SelfUpdateTreeish = settings.SelfUpdateTreeish ?? defaultSettings.SelfUpdateTreeish;
            settings.UserCommands = settings.UserCommands ?? defaultSettings.UserCommands;
            settings.CementServer = settings.CementServer ?? defaultSettings.CementServer;

            if (settings.Password != null)
            {
                settings.EncryptedPassword = Helper.Encrypt(settings.Password);
                settings.Password = null;
            }

            return settings;
        }

        private static CementSettings ReadSettings(string path)
        {
            if (!File.Exists(path))
                return new CementSettings();

            try
            {
                var data = File.ReadAllText(path);
                var settings = JsonConvert.DeserializeObject<CementSettings>(data);
                return settings ?? new CementSettings();
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteWarning("Can't read cement settings from " + path + ": " + exception.Message);
                return new CementSettings();
            }
        }
    }
}
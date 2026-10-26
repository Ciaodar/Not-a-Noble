using System;
using System.IO;
using Newtonsoft.Json;
using TaleWorlds.Library;

namespace NaN.Config
{
    public static class NanSettingsManager
    {
        private static NanJsonModel _settings;
        private static Func<NanJsonModel> _externalProvider;
        private static readonly object SyncRoot = new object();

        public static bool IsModEnabled => SettingsOrDefault().IsModEnabled;
        public static bool StrictDialogueEnabled => SettingsOrDefault().StrictDialogueEnabled;
        public static int PeasantDialogueRelationThreshold => SettingsOrDefault().PeasantDialogueRelationThreshold;
        public static int FreebooterRequiredRelation => SettingsOrDefault().FreebooterRequiredRelation;
        public static int FreebooterRequiredQuests => SettingsOrDefault().FreebooterRequiredQuests;
        public static int BannerlordPersuasionDifficulty => SettingsOrDefault().BannerlordPersuasionDifficulty;
        public static int ChieftainPersuasionDifficulty => SettingsOrDefault().ChieftainPersuasionDifficulty;
        public static int BannerlordRequiredPersuasions => SettingsOrDefault().BannerlordRequiredPersuasions;
        public static int ChieftainRequiredCaptures => SettingsOrDefault().ChieftainRequiredCaptures;
        public static int ChieftainRequiredReleases => SettingsOrDefault().ChieftainRequiredReleases;
        public static int CountRequiredArmyDefeats => SettingsOrDefault().CountRequiredArmyDefeats;
        public static int CountRequiredTowns => SettingsOrDefault().CountRequiredTowns;
        public static int SovereignRequiredTowns => SettingsOrDefault().SovereignRequiredTowns;
        public static int SovereignRequiredCastles => SettingsOrDefault().SovereignRequiredCastles;

        public static void Initialize()
        {
            LoadJsonSettings();
        }

        public static void RegisterExternalSettingsProvider(Func<NanJsonModel> provider)
        {
            _externalProvider = provider;
        }

        private static NanJsonModel SettingsOrDefault()
        {
            if (_externalProvider != null)
            {
                var externalSettings = _externalProvider();
                if (externalSettings != null)
                    return externalSettings;
            }

            lock (SyncRoot)
            {
                if (_settings == null)
                {
                    LoadJsonSettings();
                }
                return _settings ?? new NanJsonModel();
            }
        }

        public static void Reload()
        {
            lock (SyncRoot)
            {
                LoadJsonSettings();
            }
        }

        private static void LoadJsonSettings()
        {
            try
            {
                string path = Path.Combine(BasePath.Name, "Modules", "Not-a-Noble", "config.json");
                string docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), 
                    "Mount and Blade II Bannerlord", "Configs", "Not-a-Noble", "config.json");

                string fileToLoad = File.Exists(docPath) ? docPath : path;

                if (File.Exists(fileToLoad))
                {
                    string json = File.ReadAllText(fileToLoad);
                    _settings = JsonConvert.DeserializeObject<NanJsonModel>(json);
                }
                else
                {
                    _settings = new NanJsonModel();
                    SaveJsonSettings(docPath);
                }
            }
            catch (Exception ex)
            {
                Debug.Print("[NaN] Error loading settings: " + ex.Message);
                _settings = new NanJsonModel();
            }
        }

        private static void SaveJsonSettings(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Debug.Print("[NaN] Error saving settings: " + ex.Message);
            }
        }
    }
}

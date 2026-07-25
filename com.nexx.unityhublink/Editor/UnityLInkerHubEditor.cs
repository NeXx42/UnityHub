using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Nexx.HubLinker
{
    [InitializeOnLoad]
    public static class UnityLinkerHubEditor
    {
        public static readonly int? projectId;

        private const string CONFIG_FILE_NAME = "linker.json";
        public static Config config { private set; get; }

        public static string getDataRoot => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NexxUnityHub"));
        public static string getProjectRoot => Path.Combine(getDataRoot, "ProjectData", projectId.Value.ToString());


        static UnityLinkerHubEditor()
        {
            string configFile = Path.Combine(getDataRoot, CONFIG_FILE_NAME);

            if (File.Exists(configFile))
            {
                try
                {
                    string json = File.ReadAllText(configFile);
                    config = JsonUtility.FromJson<Config>(json);
                }
                catch (Exception e)
                {
                    config = new Config();
                    Debug.LogWarning($"Failed to parse linker config, reverting to default\n{e.Message}");
                }
            }
            else
            {
                config = new Config();
                SaveConfig(config);
            }

            string handoverFile = Path.Combine(getDataRoot, "LastActiveProject");

            if (!File.Exists(handoverFile))
            {
                Debug.LogError("Failed to find handover file at - " + handoverFile);
                return;
            }

            string active = File.ReadAllText(handoverFile);
            projectId = int.Parse(active);

            if (!Directory.Exists(getProjectRoot))
                Directory.CreateDirectory(getProjectRoot);

            if (config.startUpMessage)
                Debug.Log($"Link established for projectid - {projectId}");

            UnityHubScreenshotTool.Register();
        }

        public static void SaveConfig(Config config)
        {
            UnityLinkerHubEditor.config = config;

            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(Path.Combine(getDataRoot, CONFIG_FILE_NAME), json);
        }

        public static void MarkDirty(params string[] columns)
        {
            if (!projectId.HasValue)
                return;

            string dirtyFile = Path.Combine(getDataRoot, "dirty");

            if (!File.Exists(dirtyFile))
                File.Create(dirtyFile).Dispose();

            string[] lines = File.ReadAllLines(dirtyFile);
            string identifier = $"{projectId.Value}:";

            foreach (string line in lines)
            {
                if (line.StartsWith(identifier))
                {
                    // append columns
                    File.WriteAllLines(identifier, lines);
                    return;
                }
            }

            string[] newLine = new string[1] { identifier };
            File.AppendAllLines(dirtyFile, newLine);
        }

        [System.Serializable]
        public record Config
        {
            public AutoScreenshotTypes autoScreenshotOnPlay;
            public float autoScreenshotOnPlayDelay;

            public bool startUpMessage;
            public bool screenshotMessage;

            public Config()
            {
                startUpMessage = true;
                autoScreenshotOnPlay = AutoScreenshotTypes.OnPlay;
                screenshotMessage = true;
            }

            public enum AutoScreenshotTypes
            {
                None,
                OnPlay,
                OnPlayMissing
            }
        }
    }
}

using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Nexx.HubLinker
{
    [InitializeOnLoad]
    public static class UnityLinkerHubEditor
    {
        public static readonly int? projectId;

        public static string getDataRoot => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NexxUnityHub"));
        public static string getProjectRoot => Path.Combine(getDataRoot, projectId.Value.ToString());

        static UnityLinkerHubEditor()
        {
            string handoverFile = Path.Combine(getDataRoot, "LastActiveProject");

            if (!File.Exists(handoverFile))
            {
                Debug.LogError("Failed to find handover file at - " + handoverFile);
                return;
            }

            string active = File.ReadAllText(handoverFile);
            projectId = int.Parse(active);

            if (!Directory.Exists(Path.Combine(getDataRoot, projectId.ToString())))
                Directory.CreateDirectory(Path.Combine(getDataRoot, projectId.ToString()));

            Debug.Log($"Link established for projectid - {projectId}");
            UnityHubScreenshotTool.Register();
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
    }
}

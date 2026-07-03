using System.IO;
using UnityEditor;
using UnityEngine;

namespace Nexx.HubLinker
{
    public static class UnityHubScreenshotTool
    {
        public static void Register()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView view)
        {
            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(10, 10, 160, 40));

            if (GUILayout.Button("Screenshot Scene"))
            {
                Capture(view);
            }

            GUILayout.EndArea();

            Handles.EndGUI();
        }

        static void Capture(SceneView view)
        {
            var path = Path.Combine(UnityLinkerHubEditor.getDataRoot, UnityLinkerHubEditor.projectId.Value.ToString(), "icon.png");

            Camera cam = view.camera;

            int width = (int)view.position.width;
            int height = (int)view.position.height;

            RenderTexture rt = new RenderTexture(width, height, 24);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;

            File.WriteAllBytes(path, tex.EncodeToPNG());

            Object.DestroyImmediate(tex);
            rt.Release();

            UnityLinkerHubEditor.MarkDirty("icon");
        }
    }
}

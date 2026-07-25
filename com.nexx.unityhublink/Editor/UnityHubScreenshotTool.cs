using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Overlays;



#if UNITY_6000_0_OR_NEWER
using UnityEngine;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;
#endif


namespace Nexx.HubLinker
{
    public static class UnityHubScreenshotTool
    {
        [MenuItem("Edit/Nexx/Screenshot Scene")]
        public static void ScreenshotScene() => CaptureScene();

        [MenuItem("Edit/Nexx/Screenshot Game")]
        public static void ScreenshotGame() => CaptureGame();

        public static void Register()
        {
            EditorApplication.playModeStateChanged += OnPlay;
        }

        private static void OnPlay(PlayModeStateChange to)
        {
            switch (UnityLinkerHubEditor.config.autoScreenshotOnPlay)
            {
                case UnityLinkerHubEditor.Config.AutoScreenshotTypes.None:
                    return;

                case UnityLinkerHubEditor.Config.AutoScreenshotTypes.OnPlayMissing:
                    if (File.Exists(Path.Combine(UnityLinkerHubEditor.getProjectRoot, "icon.png")))
                        return;
                    break;
            }

            if (to != PlayModeStateChange.EnteredPlayMode)
                return;

            _ = Run();

            async Task Run()
            {
                int delay = Math.Max(0, (int)Math.Round(UnityLinkerHubEditor.config.autoScreenshotOnPlayDelay * 1000));
                await Task.Delay(delay);

                if (!Application.isPlaying)
                    return;

                CaptureGame();
            }
        }

        public static void CaptureScene()
        {
            if (SceneView.sceneViews.Count == 0)
                return;

            SceneView view = (SceneView)SceneView.sceneViews[0];
            CaptureScreenShot(view.camera, view.camera.pixelWidth, view.camera.pixelHeight);
        }

        public static void CaptureGame()
        {
            Camera cam = Camera.main;

            if (cam == null)
            {
                Debug.LogWarning("Failed to find main camera to screenshot");
                return;
            }

            CaptureScreenShot(cam, cam.pixelWidth, cam.pixelHeight);
        }

        private static void CaptureScreenShot(Camera cam, int width, int height)
        {
            var path = Path.Combine(UnityLinkerHubEditor.getProjectRoot, "icon.png");

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

            UnityEngine.Object.DestroyImmediate(tex);
            rt.Release();

            UnityLinkerHubEditor.MarkDirty("icon");

            if (UnityLinkerHubEditor.config.screenshotMessage)
                Debug.Log("Saved screenshot");
        }
    }

#if UNITY_6000_0_OR_NEWER
    [Overlay(typeof(SceneView), "Unity Linker", defaultDisplay = true)]
    public class UnityHubScreenshotTool_Scene : ToolbarOverlay
    {
        public UnityHubScreenshotTool_Scene() : base(UnityHubScreenshotTool_Screenshot.ID) { }
    }

    [EditorToolbarElement(
    ID,
    typeof(SceneView)
)]
    public class UnityHubScreenshotTool_Screenshot : EditorToolbarButton
    {
        public const string ID = "Nexx/Screenshot";

        public UnityHubScreenshotTool_Screenshot()
        {
            text = "Screenshot";
            tooltip = "Capture Scene View screenshot";

            clicked += UnityHubScreenshotTool.CaptureScene;
        }
    }
#endif
}

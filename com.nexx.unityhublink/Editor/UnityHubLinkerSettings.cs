#if UNITY_EDITOR
using System.IO;
using Nexx.HubLinker;
using UnityEditor;
using UnityEngine;

public class UnityHubLinkerSettings : EditorWindow
{
    private UnityLinkerHubEditor.Config.AutoScreenshotTypes autoScreenshotOnPlay;
    private float autoScreenshotOnPlayDelay;

    private bool messageOnInit;
    private bool messageOnScreenshot;


    private string imagePath => Path.Combine(UnityLinkerHubEditor.getProjectRoot, "icon.png");

    private Texture2D selectedImage;

    [MenuItem("Edit/Nexx/Hublink Config")]
    public static void ShowWindow()
    {
        GetWindow<UnityHubLinkerSettings>("Unity Hub Linker");
    }

    private void OnEnable()
    {
        autoScreenshotOnPlay = UnityLinkerHubEditor.config.autoScreenshotOnPlay;
        autoScreenshotOnPlayDelay = UnityLinkerHubEditor.config.autoScreenshotOnPlayDelay;

        messageOnInit = UnityLinkerHubEditor.config.startUpMessage;
        messageOnScreenshot = UnityLinkerHubEditor.config.screenshotMessage;

        LoadImage();
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Linked Project", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Project ID:", UnityLinkerHubEditor.projectId.ToString());

        GUILayout.Space(15);
        EditorGUILayout.BeginHorizontal();

        const int iconWidth = 400;
        EditorGUILayout.BeginVertical(GUILayout.Width(iconWidth));

        if (selectedImage != null)
        {
            float aspect = (float)selectedImage.width / selectedImage.height;
            float iconHeight = iconWidth / aspect;

            GUILayout.Label(selectedImage, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
            EditorGUILayout.LabelField(imagePath);

            if (GUILayout.Button("Delete Icon"))
            {
                DeleteImage();
            }
        }
        else
        {
            GUILayout.Box("No Icon", GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
        }

        EditorGUILayout.EndVertical();
        GUILayout.Space(15);
        EditorGUILayout.BeginVertical();

        autoScreenshotOnPlay = (UnityLinkerHubEditor.Config.AutoScreenshotTypes)EditorGUILayout.EnumPopup("Auto Screenshot on Play", autoScreenshotOnPlay);
        autoScreenshotOnPlayDelay = EditorGUILayout.Slider("Auto Screenshot delay (Seconds)", autoScreenshotOnPlayDelay, 0, 10);

        messageOnInit = EditorGUILayout.Toggle("Startup Message", messageOnInit);
        messageOnScreenshot = EditorGUILayout.Toggle("Message on screenshot", messageOnScreenshot);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            UnityLinkerHubEditor.SaveConfig(UnityLinkerHubEditor.config with
            {
                autoScreenshotOnPlay = autoScreenshotOnPlay,
                autoScreenshotOnPlayDelay = autoScreenshotOnPlayDelay,

                screenshotMessage = messageOnScreenshot,
                startUpMessage = messageOnInit
            });
        }
    }

    private void LoadImage()
    {
        selectedImage = null;

        if (string.IsNullOrEmpty(imagePath))
            return;

        if (!File.Exists(imagePath))
            return;

        byte[] imageData = File.ReadAllBytes(imagePath);

        selectedImage = new Texture2D(2, 2);
        selectedImage.LoadImage(imageData);

        Repaint();
    }

    private void DeleteImage()
    {
        selectedImage = null;

        if (File.Exists(imagePath))
        {
            File.Delete(imagePath);
        }

        Repaint();
    }
}
#endif
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
namespace Editor
{
    public static class PlayModeBootper
    {
    const string BootSceneName = "_Boot.unity";
    const string BootSceneKeyPlayerPrefs = "PlayBoot_TargetScene"; // used at runtime by the _Boot scene
    const string EditorSceneKey = "PlayBoot_EditorScene"; // used to restore the editor scene after play

    static PlayModeBootper()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // About to enter Play Mode from the Editor
            var activeScene = SceneManager.GetActiveScene();
            var activePath = activeScene.path;

            // If the scene is unsaved or user has modifications, ask to save first
            if (string.IsNullOrEmpty(activePath))
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // User cancelled the save prompt. Stop entering Play Mode.
                    EditorApplication.isPlaying = false;
                    return;
                }
                activePath = SceneManager.GetActiveScene().path;
            }

            // If already in the boot scene, nothing to do
            if (!string.IsNullOrEmpty(activePath) && (activePath.EndsWith(BootSceneName) || activePath.Contains("/" + BootSceneName)))
            {
                return;
            }

            // Save the editor scene path so we can restore it after Play Mode
            EditorPrefs.SetString(EditorSceneKey, activePath);

            // Save the target scene path into PlayerPrefs so the runtime Boot scene can read it
            PlayerPrefs.SetString(BootSceneKeyPlayerPrefs, activePath);
            PlayerPrefs.Save();

            // Find the boot scene
            string bootPath = FindBootScenePath();
            if (string.IsNullOrEmpty(bootPath))
            {
                EditorUtility.DisplayDialog("Boot scene not found", $"Could not find {BootSceneName} in the project. Play will continue with the current scene.", "OK");
                return;
            }

            // Open the boot scene (single) so Play Mode starts from it
            var bootScene = EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);
            if (!bootScene.IsValid())
            {
                Debug.LogError($"Failed to open boot scene at {bootPath}");
            }
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Returned to Edit Mode after stopping Play Mode — restore the previously open scene
            if (EditorPrefs.HasKey(EditorSceneKey))
            {
                var sceneToOpen = EditorPrefs.GetString(EditorSceneKey);
                if (!string.IsNullOrEmpty(sceneToOpen))
                {
                    // Try to open by path
                    if (System.IO.File.Exists(sceneToOpen))
                    {
                        EditorSceneManager.OpenScene(sceneToOpen, OpenSceneMode.Single);
                    }
                    else
                    {
                        // If the exact path not found, attempt to open by scene name
                        var name = System.IO.Path.GetFileNameWithoutExtension(sceneToOpen);
                        var guids = AssetDatabase.FindAssets(name + " t:Scene");
                        if (guids != null && guids.Length > 0)
                        {
                            EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guids[0]), OpenSceneMode.Single);
                        }
                        else
                        {
                            Debug.LogWarning($"Original scene '{sceneToOpen}' not found. Could not restore after Play Mode.");
                        }
                    }
                }
                EditorPrefs.DeleteKey(EditorSceneKey);
            }

            // Clean up runtime PlayerPrefs key used by Boot
            if (PlayerPrefs.HasKey(BootSceneKeyPlayerPrefs))
            {
                PlayerPrefs.DeleteKey(BootSceneKeyPlayerPrefs);
            }
        }
    }
}

    static string FindBootScenePath()
    {
        // Common default location
        string candidate = "Assets/Scenes/_Boot.unity";
        if (System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "Scenes/_Boot.unity")))
            return candidate;

        // Search the asset database for a scene named _Boot
        string[] guids = AssetDatabase.FindAssets("_Boot t:Scene");
        if (guids != null && guids.Length > 0)
        {
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (path.EndsWith(BootSceneName))
                    return path;
            }
        }

        // Broader search fallback
        guids = AssetDatabase.FindAssets("_Boot");
        if (guids != null)
        {
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (path.EndsWith(".unity") && path.Contains("_Boot"))
                    return path;
            }
        }

        return null;
    }
}


using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
	const string BootSceneKeyPlayerPrefs = "PlayBoot_TargetScene";
	public float delayBeforeLoad = 0.1f; // small delay to allow boot scene initialization

	IEnumerator Start()
	{
		// Try read the target scene path from PlayerPrefs (set by the editor)
		if (!PlayerPrefs.HasKey(BootSceneKeyPlayerPrefs))
		{
			yield break; // nothing to do
		}

		var target = PlayerPrefs.GetString(BootSceneKeyPlayerPrefs);
		if (string.IsNullOrEmpty(target))
			yield break;

		// clear the key so subsequent runs are clean
		PlayerPrefs.DeleteKey(BootSceneKeyPlayerPrefs);

		// Wait a small amount of time so any boot initialization has time to run
		if (delayBeforeLoad > 0f)
			yield return new WaitForSeconds(delayBeforeLoad);

		// If the value looks like an asset path (Assets/.../scene.unity), try to load by scene name
		string sceneToLoad = target;
		if (target.StartsWith("Assets/"))
		{
			sceneToLoad = System.IO.Path.GetFileNameWithoutExtension(target);
		}

		if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
		{
			SceneManager.LoadScene(sceneToLoad);
		}
		else
		{
			// Attempt to load the raw target string as fallback
			try
			{
				SceneManager.LoadScene(target);
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"BootLoader failed to load target scene '{target}': {ex.Message}");
			}
		}
	}
}



using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] GameObject loadingScreen;
    [SerializeField] Slider slider;
    const float minimumLoadingTime = 2f; // Minimum loading time in seconds

    // Assign this method to a button click event
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false; // Cause the size of scene is too small, 
                                                // so we need to wait for a while (fake loading)
        loadingScreen.SetActive(true);

        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / minimumLoadingTime);

            // If the load is finished, allow the scene activation
            if (operation.progress >= 0.9f && elapsedTime >= minimumLoadingTime)
            {
                operation.allowSceneActivation = true;
            }

            // Update the loading progress
            slider.value = Mathf.Min(progress, Mathf.Clamp01(operation.progress / 0.9f));

            yield return null;
        }
    }
}

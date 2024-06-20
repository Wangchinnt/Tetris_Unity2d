using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider slider;
    public float minimumLoadingTime = 2f;  // Thời gian tối thiểu cho màn hình loading

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false; // Ngăn cảnh kích hoạt ngay lập tức

        loadingScreen.SetActive(true);

        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / minimumLoadingTime);

            // Nếu việc load đã hoàn tất và thời gian tối thiểu đã qua, kích hoạt cảnh
            if (operation.progress >= 0.9f && elapsedTime >= minimumLoadingTime)
            {
                operation.allowSceneActivation = true;
            }

            // Cập nhật thanh trượt với giá trị nhỏ hơn giữa tiến trình thực tế và thời gian đã trôi qua
            slider.value = Mathf.Min(progress, Mathf.Clamp01(operation.progress / 0.9f));

            yield return null;
        }
    }
}

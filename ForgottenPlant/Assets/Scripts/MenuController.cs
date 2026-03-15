using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject optionsPanel;

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    public void LoadSandbox()
    {
        StartCoroutine(LoadSceneAsync("SC_Sandbox"));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError("Scene could not be loaded.");
            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }
    }
}
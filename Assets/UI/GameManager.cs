using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    static string currentScene = "MainMenu";
    private void Awake()
    {
        if (!instance){
            instance = this;
            DontDestroyOnLoad(this);
        }
        else{
            Destroy(this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)){
            if (currentScene != "MainMenu") LoadScene("MainMenu");
            else Application.Quit();
        }
    }

    public static void LoadScene(string sceneName){
        SceneManager.LoadScene(sceneName);
        currentScene = sceneName;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
	void Start () {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    public void StartClassicGame() {
        PlayerPrefs.SetInt("3dMode", 0);
        PlayerPrefs.SetInt("AppleCount", 1);
        SceneManager.LoadScene("Main");
    }

    public void Start3DGame() {
        PlayerPrefs.SetInt("3dMode", 1);
        PlayerPrefs.SetInt("AppleCount", 10);
        SceneManager.LoadScene("Main");
    }

    public void ExitGame() {
        Application.Quit();
    }
}

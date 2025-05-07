using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private SaveSlotUI saveSlotUI; // 存档界面UI
    
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitMenu()
    {
        // 创建临时对象以定位DontDestroyOnLoad场景
        GameObject temp = new GameObject("TempDontDestroyScene");
        DontDestroyOnLoad(temp);
        Scene dontDestroyScene = temp.scene;

        // 获取并销毁该场景中的所有根对象
        GameObject[] rootObjects = dontDestroyScene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            Destroy(obj);
        }

        // 加载初始场景
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

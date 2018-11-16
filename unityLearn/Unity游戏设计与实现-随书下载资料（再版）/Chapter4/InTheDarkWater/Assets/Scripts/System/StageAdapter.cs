using UnityEngine;
using System.Collections;

/// <summary>
/// 用于Stage场景的Adapter。各Stage的Field的载入器作用
/// 游戏的开始和结束都一定会调用这个类，切换场景时会通知Root结束事件
/// </summary>
public class StageAdapter : MonoBehaviour {

    public enum Type
    {
        None = -1,
        Stage1,
        Stage2,
        Stage3
    };
    [SerializeField]
    private Type currentType = Type.None;   // 现在的关卡
    [SerializeField]
    private string[] fieldSceneName = new string[] { 
        "Field1", "Field2", "Field3" 
    };  // 各种主场景名称
   
    private GameObject root = null;
    private GameObject ui = null;
    private GameObject field = null;

    [SerializeField]
    private bool playOnAwake = true;
    private bool nextStage = false;
    private bool loaded = false;

    void Awake()
    {
        // 使用LoadLevel对Destory对象进行剔除
        // Destory的判断由SceneSelector执行
        DontDestroyOnLoad(gameObject);
    }

    // 加载结束时
//    void OnLevelWasLoaded(int level)
//    {
//        Debug.Log("OnLevelWasLoaded : level=" + level + " - " + Application.loadedLevelName);
    // 加载结束后，因为无法单体调试所以直接Start
    void Start()
    {
        root = GameObject.Find("/Root");
        ui = GameObject.Find("/UI");
        // Field海没有被加载因此跳过Intermission加载最初的关卡
        if (playOnAwake)
        {
            SetNextStage(Type.Stage1);
            OnIntermissionEnd();
        }
    }

    // 各个Field调用
    void OnLoadedField()
    {
        loaded = true;
        // Field加载完成后开始切换过场
        Debug.Log("Field Loaded");
        field = GameObject.Find("/Field");

        // 对声纳位置进行对齐等。在SlideOut前设定好
        if (ui) ui.BroadcastMessage("OnAwakeStage", (int)currentType);

        if (root) root.BroadcastMessage("OnSlideOut", gameObject);
        else OnIntermissionEnd();
    }

    // 过场结束时的处理
    void OnIntermissionEnd()
    {
        if (loaded) GameStart();
        else Load();
    }

    // 场景结束时被调用
    void OnGameEnd(bool nextStage_)
    {
        nextStage = nextStage_;
        if (nextStage) {
            // 过关后的处理
            Debug.Log("!!! GameClear !!!");
            if (field) field.BroadcastMessage("OnGameClear", SendMessageOptions.DontRequireReceiver);
            if (ui) ui.BroadcastMessage("OnGameClear", SendMessageOptions.DontRequireReceiver);
        }
		else {
	        // 游戏结束后的处理
            Debug.Log("!!! GameOver !!!");
            if (field) field.BroadcastMessage("OnGameOver", SendMessageOptions.DontRequireReceiver);
            if (ui) ui.BroadcastMessage("OnGameOver", SendMessageOptions.DontRequireReceiver);
		}
    }

    // TitleSwitcher调用
    void OnSceneEnd()
    {
        if (nextStage)
        {
            // 切换到下一个Stage
            SetNextStage();
            // 开始切换过场
            if (root) root.BroadcastMessage("OnSlideIn", gameObject);
            else OnIntermissionEnd();
        }
        else
        {
            // 返回Title
            if (root) root.SendMessage("OnStartTitle");
        }
    }
    
    // 主要用于调试
    void OnFieldLoad( Type type )
    {
        SetNextStage(type);
        if (root) root.BroadcastMessage("OnSlideIn", gameObject);
        else OnIntermissionEnd();
    }


    // 这里表示游戏开始
    private void GameStart()
    {
        Debug.Log("!!! GameStart !!!");
        // 开始游戏
        if (field) field.BroadcastMessage("OnGameStart", SendMessageOptions.DontRequireReceiver);
        if (ui) ui.BroadcastMessage("OnGameStart", SendMessageOptions.DontRequireReceiver);
    }
    // 加载
    private bool Load()
    {
        if (currentType == Type.None) return false;

        // 销毁field
        if (field) GameObject.Destroy(field);
        // 重置UI
        if (ui) ui.BroadcastMessage("OnStageReset", SendMessageOptions.DontRequireReceiver);

        string name = fieldSceneName[(int)currentType];
        Debug.Log("Load : " + name);
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);		// Application.LoadLevelAdditive
        return true;
    }

    private void SetNextStage(Type setType = Type.None)
    {
        loaded = false;

        // 设定currentType
        if (setType == Type.None)
        {
            int current = (int)currentType;
            current++;
            // 超过关卡数量时回到Title
            if (current >= fieldSceneName.Length)
            {
                // 回到Title
                if (root) root.SendMessage("OnStartTitle");
                return;
            }
            else currentType = (Type)(current);
        }
        else currentType = setType;
    }

}

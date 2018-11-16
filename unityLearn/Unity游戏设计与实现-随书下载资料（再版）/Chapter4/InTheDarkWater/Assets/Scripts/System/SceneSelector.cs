using UnityEngine;
using System.Collections;

/// <summary>
/// 切换场景
/// </summary>
public class SceneSelector : MonoBehaviour {

    public enum Type {
        None = -1,
        Title = 0,
        Stage
    };
    [SerializeField]
    private string titleSceneName = "Title";
    [SerializeField]
    private string stageSceneName = "Stage";
    [SerializeField]
    private Type type = Type.None;   // 当前关卡

    [SerializeField]
    private bool playOnAwake = true;    // 是否立刻开始（Release时设置为On）

    [SerializeField]    // debug
    private int highScore = 0;        // 最高分记录

	void Awake()
	{
		// 通过LoadLevel将被Destory的对象剥离
        DontDestroyOnLoad(gameObject);
	}
	
    void Start() 
    {
        // 即时加载Title（考虑到加载为了保险起见延迟1.0f）
        if (playOnAwake) StartCoroutine("Wait", 1.0f);
    }
    private IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        LoadScene();
    }

    private bool LoadScene()
    {
        switch (type)
        { 
            default:    return false;
            case Type.Title:
                GameObject ui = GameObject.Find("/UI");
                if(ui) {
                    // 更新得分记录
                    UpdateHiScore( ui );
                    // 强制销毁UI
                    Destroy(ui);
                    ui = null;
                }
                // 保存引用
                GameObject adapter = GameObject.Find("/Adapter");
                if (adapter)
                {
                    // 强制销毁Adapter
                    Destroy(adapter);
                    adapter = null;
                }
                UnityEngine.SceneManagement.SceneManager.LoadScene(titleSceneName);
                break;
            case Type.Stage:
                // Title未被设置为DontDestory因此LoadLevel是OK的
                UnityEngine.SceneManagement.SceneManager.LoadScene(stageSceneName);
                break;
        }
        
        return true;
    }

    private void UpdateHiScore( GameObject ui )
    {
        StageUI stageUI = ui.GetComponent<StageUI>();
        int newScore = 0;
        if (stageUI) newScore = stageUI.Score();
        if (highScore < newScore) highScore = newScore;
    }

    // 载入标题
    void OnStartTitle()
    {
        // 设置标题
        type = Type.Title;
        // 开始过场切换
        BroadcastMessage("OnFadeIn", gameObject);
    }

    // 载入下一步骤
    void OnStartStage( )
    {
        // 设置关卡
        type = Type.Stage;
        // 开始过场切换
        BroadcastMessage("OnFadeIn", gameObject);
    }

    // 过场切换结束后
    void OnIntermissionEnd()
    {
        // 如果未加载则开始加载
        LoadScene(); 
    }

    public int HighScore()
    {
        return highScore;
    }
}

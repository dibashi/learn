using UnityEngine;
using System.Collections;

/// <summary>
/// 生成物的变化。当前，只考虑通过RandomGenerator生成的对象
/// </summary>
public class GenerateSwitcher : MonoBehaviour {

    enum Type
    {
        None = 0,
        OnlyOne,
        Switch,
//        Random,
//        All,
    };
    [SerializeField]
    private Type type = Type.None;
    [SerializeField]
    private string currentTag = "Enemey";

    public class TargetGenerator
    {
        public bool clearCondition = false;
        public RandomGenerator gen = null;
    };

    private TargetGenerator current = null;
    private Hashtable generators = new Hashtable();

	void Start () 
    {
    }

    private void Init()
    {
        RandomGenerator[] genArr = GetComponentsInChildren<RandomGenerator>();
        foreach( RandomGenerator gen in genArr ){
            AddGenerator(gen);
        } 
    }

    private void AddGenerator( RandomGenerator generater )
    {
        Debug.Log("AddGenerator");
        if (generater == null) return;

        GameObject target = generater.Target();
        Debug.Log("Target:" + target.tag);
        if (target == null) return;
        string tag = target.tag;

        TargetGenerator targetGenerator = new TargetGenerator();
        targetGenerator.clearCondition = false;
        targetGenerator.gen = generater;
        generators.Add(tag, targetGenerator);
    }
	

    void OnSwitchCheck( string key )
    {
		Debug.Log("OnSwitchCheck:" + key);
        if (currentTag.Equals(key))
        {
            switch (type)
            {
                case Type.Switch: Switch(); break;
                // 如果希望改变模式则追加
                //case Type.Random: SetRandom(); break;
                default: break;
            }
        }
    }

    private void Run()
    {
        if (!generators.ContainsKey(currentTag))
        {
            Debug.Log(currentTag + ": Not Exist!");
            return;
        }
        current = generators[currentTag] as TargetGenerator;
        current.gen.SendMessage("OnGeneratorStart");
    }

    /// <summary>
    /// 开关
    /// </summary>
    private void Switch()
    {
        if (generators.Count == 0) return;

        Suspend();
        //current.gen.SendMessage("OnGeneratorSuspend");

        foreach( string key in generators.Keys )
        {
            if (!currentTag.Equals(key))
            {
                currentTag = key;
                Run();
                return;
            }
        }
    }

    // 暂时中断
    private void Suspend()
    {
        if (current == null) return;
        current.gen.SendMessage("OnGeneratorSuspend");
    }


    // 游戏开始事件
    void OnGameStart()
    {
        Init();
        Run();
    }
    // 游戏结束事件
    void OnGameOver()
    {
        Suspend();
    }
    // 清空游戏事件
    void OnGameClear()
    {
        Suspend();
    }


    void OnClearCondition(string tag)
    {
        // 清空条件
        bool allClear = true;
        foreach (string key in generators.Keys) 
        {
            // 将用于表示条件满足的标签TargetObject值设置为true
            TargetGenerator target = generators[key] as TargetGenerator;
            if (tag.CompareTo(key) == 0) target.clearCondition = true;
            // 检测是否全部清空
            allClear &= target.clearCondition;
        }

        if (allClear) {
            // 游戏结束，进入下一步骤
            GameObject adapter = GameObject.Find("/Adapter");
            adapter.SendMessage("OnGameEnd", true);
        }
    }
}

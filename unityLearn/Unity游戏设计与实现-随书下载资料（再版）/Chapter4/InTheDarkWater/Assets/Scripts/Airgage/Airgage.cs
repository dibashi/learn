using UnityEngine;
using System.Collections;

/// <summary>
/// 显示空气的剩余量
/// </summary>
public class Airgage : MonoBehaviour {

    [SerializeField]
    private float offsetGageSize = 120.0f;  // 0表示画面边缘
    [SerializeField]
    private Vector2 offsetPixelGage = Vector2.zero;  // 0表示画面边缘
    [SerializeField]
    private Vector2 offsetPixelText = Vector2.zero;  // 0表示画面边缘
    
    [SerializeField]
    private float[] airUpdateTime = new float[] {
        8.0f, 5.0f, 3.0f, 2.0f, 1.0f, 0.5f
    };  // 氧气减少量的更新频度

    [SerializeField]
    private float airMax = 1000.0f;     // 空气量的最大值
    [SerializeField]
    private float step = 1.0f;          // 每次更新时的减少量

    [SerializeField]    // debug
    private float air = 0;              // 现在的空气量

    private int damageLv = 0;           // 损坏的程度
    private float counter = 0;

    private bool gameover = false;

    private GameObject meterObj;
    private GameObject damageLvObj;


    void Start()
    {
        meterObj = GameObject.Find("AirgageMeter");
        damageLvObj = GameObject.Find("DamageLvText");

        // 位置調整
        float w = (float)Screen.width;
        float h = (float)Screen.height;

        float aspect = w / h;
        offsetPixelGage.x += offsetGageSize;
        offsetPixelGage.y += offsetGageSize;
        float posX = aspect * (1.0f - offsetPixelGage.x / w);
        float posY = 1.0f - offsetPixelGage.y / h;
        meterObj.transform.position = new Vector3(posX, posY, 0.0f);

        posX = 1.0f - offsetPixelText.x / w;
        posY = 1.0f - offsetPixelText.y / h;
        damageLvObj.transform.position = new Vector3(posX, posY, 0.0f);

        OnStageReset();
    }

    void Update()
    {
        // 根据计数器进行更新
        if (!gameover)
        {
            counter += Time.deltaTime;
            if (counter > airUpdateTime[damageLv])
            {
                Deflate();
                counter = 0;
            }
        }
    }

    /// <summary>
    /// [BroadcastMessage]
    /// 收到损坏
    /// </summary>
    /// <param name="value">损坏量。一般是1</param>
    void OnDamage(int value)
    {
        // 累加损坏的程度
        damageLv += value;
        UpdateDamageLv();
    }

    void OnAddAir(int value )
    {
        air += value;
        if (airMax < air) air = airMax;
    }

    void OnStageReset()
    {
        air = airMax;
        damageLv = 0;
        UpdateAirgage();
        UpdateDamageLv();
        gameover = false;
    }

    /// <summary>
    /// 更新air
    /// </summary>
    private void Deflate()
    {
        // 更新值
        air -= step;
        if( air <= 0.0f ) {
            air = 0.0f;
            gameover = true;
        }
        // 向仪表传递值
        UpdateAirgage();

        if (gameover)
        {
            // 氧气耗尽。游戏结束（传递false值）
            GameObject adapter = GameObject.Find("/Adapter");
            adapter.SendMessage("OnGameEnd", false);
        }
    }

    private void UpdateDamageLv()
    {
        damageLv = Mathf.Clamp(damageLv, 0, airUpdateTime.Length - 1);
        BroadcastMessage("OnDisplayDamageLv", damageLv);
    }
    private void UpdateAirgage()
    {
        float threshold = Mathf.InverseLerp(0, airMax, air);
        meterObj.SendMessage("OnDisplayAirgage", threshold);
    }

    public float Air() { return air; }
    public int DamageLevel() { return damageLv; }
}

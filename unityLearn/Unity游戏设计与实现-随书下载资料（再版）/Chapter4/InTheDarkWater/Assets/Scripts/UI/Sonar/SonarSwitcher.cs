using UnityEngine;
using System.Collections;

/// <summary>
/// 主动声纳和被动声纳的切换
/// </summary>
public class SonarSwitcher : MonoBehaviour
{

    [SerializeField]
    private GameObject activeObj = null;
    [SerializeField]
    private GameObject passiveObj = null;
    [SerializeField]
    private int offsetPixel = 10;   // 距离左端位置的偏移
    [SerializeField]
    private float aspect = 0.4f;    // 和画面的尺寸比例
    [SerializeField]
    private int cameraRayoutPixel = 8;  // 在外观上，通过比纹理尺寸稍小的尺寸来决定摄像机的位置

    private GameObject currentObj = null;

    private float radius = 0;

    public enum SonarMode {
        None,
        PassiveSonar,
        ActiveSonar
    }
    private SonarMode mode = SonarMode.None;

	void Start () 
    {
        InitTexturePos();
    }

    void Update()
    {
        // 只有按住时才变成主动声纳
        if (Input.GetKeyDown(KeyCode.Space)) SetMode(SonarMode.ActiveSonar);
        if (Input.GetKeyUp(KeyCode.Space)) SetMode(SonarMode.PassiveSonar);
    }

    void SetMode( SonarMode mode_ )
    {
        if (mode == mode_) return;

        // 根据屏幕尺寸来调整位置和大小
        if (currentObj != null)
        {
            Destroy(currentObj);
        }

        switch (mode_)
        {
            case SonarMode.ActiveSonar:
                CreateSonar(activeObj);
                break;

            case SonarMode.PassiveSonar:
                CreateSonar(passiveObj);
                break;

            default:
                GetComponent<GUITexture>().enabled = false;
                break;
        }

        mode = mode_;
    }

    void OnGameStart()
    {
    }

    void OnAwakeStage(int index)
    { 
        InitCameraPos();
    }

    void OnStageReset()
    {
//        InitCameraPos();
        Debug.Log("OnStageReset");
        // 默认为被动声纳
        SetMode(SonarMode.PassiveSonar);
    }

    private void InitTexturePos()
    {
        float size = Screen.height * aspect;

        GetComponent<GUITexture>().enabled = true;
        GetComponent<GUITexture>().pixelInset = new Rect(offsetPixel, Screen.height - offsetPixel - size, size, size);
    }
    private void InitCameraPos()
    {
        // 将纹理尺寸传递给摄像机
        Rect cameraRect = new Rect(GetComponent<GUITexture>().pixelInset);
        cameraRect.x += cameraRayoutPixel;
        cameraRect.y += cameraRayoutPixel;
        cameraRect.width -= cameraRayoutPixel * 2;
        cameraRect.height -= cameraRayoutPixel * 2;

        GameObject cameraObj = GameObject.Find("/Field/Player/SonarCamera");
        if (cameraObj)
        {
            SonarCamera sonarCamera = cameraObj.GetComponent<SonarCamera>();
            sonarCamera.SetRect(cameraRect);
            radius = sonarCamera.Radius();
        }
//        SetSize(activeObj);
//        SetSize(passiveObj);
    }

    private void SetSize(GameObject obj)
    {
        ActiveSonar activeSonar = currentObj.GetComponent<ActiveSonar>();
        if (activeSonar) activeSonar.SetMaxRadius(radius);
        SonarEffect effecter = currentObj.GetComponent<SonarEffect>();
        if (effecter) effecter.Init(GetComponent<GUITexture>().pixelInset);
    }

    private void CreateSonar( GameObject obj ) 
    {
        currentObj = Object.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
        currentObj.transform.parent = transform;

        SetSize(currentObj);
   }
}

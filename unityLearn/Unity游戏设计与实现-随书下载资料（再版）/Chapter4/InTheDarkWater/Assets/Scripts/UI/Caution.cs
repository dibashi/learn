using UnityEngine;
using System.Collections;

/// <summary>
/// Caution值
/// </summary>
public class Caution : MonoBehaviour {

    [SerializeField]
    private Vector2 offsetPixel = Vector2.zero;  // 0表示在屏幕边缘
    [SerializeField]
    private int disitSize = 3;
    [SerializeField]
    private Color cautionColor = Color.yellow;

    private int cautionValue = 0;

	void Start () 
    {
        GUITexture texture = GetComponentInChildren<GUITexture>();
        if (texture)
        {
            // 调整位置
            float w = (float)Screen.width;
            float h = (float)Screen.height;
            float xPos = 1.0f - (texture.texture.width + offsetPixel.x)/w;
            float yPos = offsetPixel.y/h;
            transform.position = new Vector3(xPos, yPos, 0.0f);
        }

        GetComponent<GUIText>().material.color = cautionColor;
        GUIText[] textArr = GetComponentsInChildren<GUIText>();
        for (int i = 0; i < textArr.Length; i++ )
        {
            textArr[i].material.color = cautionColor;
        }
    }

    void OnStageReset()
    {
        OnUpdateCaution(0);
    }

    void OnUpdateCaution(int value)
    {
        cautionValue = value;
        GetComponent<GUIText>().text = cautionValue.ToString("D" + disitSize);
    }

    public int Value() { return cautionValue; }

}

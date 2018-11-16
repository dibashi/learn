using UnityEngine;
using System.Collections;

/// <summary>
/// 显示Airgage下的损坏提示文字
/// </summary>
public class DamageLvText : MonoBehaviour {

    [SerializeField]
    private int disitSize = 1;

    /// <summary>
    /// 更新[SendMessage]显示
    /// </summary>
    /// <param name="value"></param>
    void OnDisplayDamageLv(int value)
    {
        GetComponent<GUIText>().text = value.ToString("D" + disitSize);
    }
	
}

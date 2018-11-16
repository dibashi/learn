using UnityEngine;
using System.Collections;

/// <summary>
/// 空气残余量的表示，使用了Cutout shader
/// </summary>
public class AirgageMeter : MonoBehaviour {

    /// <summary>
    /// 更新[SendMessage]的值
    /// </summary>
    /// <param name="value">更新值[0,1]</param>
    void OnDisplayAirgage(float value)
    {
        //Debug.Log("OnDeflate: Air=" + value);
        // 通过改变shader的alpha cutoff值来更新显示.
        GetComponent<Renderer>().material.SetFloat("_Cutoff", value);
    }
}

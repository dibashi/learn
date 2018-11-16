using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 击中时销毁模型
/// </summary>
public class TorpedoModel : MonoBehaviour {


    void OnHit()
    {
        GetComponent<Renderer>().enabled = false;
    }
}

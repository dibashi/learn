using UnityEngine;
using System.Collections;

/// <summary>
/// 击中时消除粒子系统
/// </summary>
public class TorpedoMoveEffect : MonoBehaviour {

    void OnHit()
    {
        GetComponent<ParticleSystem>().Stop();
    }
}

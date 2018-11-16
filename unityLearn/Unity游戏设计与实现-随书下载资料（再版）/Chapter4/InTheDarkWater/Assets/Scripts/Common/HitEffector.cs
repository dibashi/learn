using UnityEngine;
using System.Collections;

/// <summary>
/// 碰撞特效专用
/// 在调用OnHit时，将播放粒子特效和音效
/// </summary>
public class HitEffector : MonoBehaviour {

    [SerializeField]
    private bool valid = true;

    void Start()
    { 
    }

    // 如果无效则提前调用
    void OnInvalidEffect()
    {
        Debug.Log("HitEffector.OnInvalid");
        valid = false;
    }

    // 碰撞时的行为管理和结束时间
    void OnHit()
    {

        Debug.Log("HitEffector.OnHit:" + transform.parent.gameObject.transform.parent.tag);
        if (valid)
        {
            if (GetComponent<ParticleSystem>())
            {
                Debug.Log("HitEffector => particle.Play");
                GetComponent<ParticleSystem>().Play();
            }
            if (GetComponent<AudioSource>())
            {
                Debug.Log("HitEffector => audio.Play");
                GetComponent<AudioSource>().Play();
            }
        }
        else Debug.Log("HitEffector.OnHit: Invalid");
    }

    public bool IsFinished()
    {
        bool result = true;
        if (GetComponent<ParticleSystem>()) result = result && !GetComponent<ParticleSystem>().isPlaying;
        if (GetComponent<AudioSource>()) result = result && !GetComponent<AudioSource>().isPlaying;
        return result;
    }
    public bool IsPlaying()
    {
        bool result = false;
        if (GetComponent<ParticleSystem>()) result = result || GetComponent<ParticleSystem>().isPlaying;
        if (GetComponent<AudioSource>()) result = result || GetComponent<AudioSource>().isPlaying;
        return result;
    }
}

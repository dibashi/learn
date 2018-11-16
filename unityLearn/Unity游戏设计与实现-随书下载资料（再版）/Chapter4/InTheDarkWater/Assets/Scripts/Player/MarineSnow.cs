using UnityEngine;
using System.Collections;

/// <summary>
/// 海雪特效的设定
/// </summary>
public class MarineSnow : MonoBehaviour {

    [SerializeField]
    private float maxSpeed = 30.0f;

    
    //void OnGameOver() 
    //{
        //particleSystem.Pause();
    //}

    public void SetSpeed( float rate ) 
    {
        GetComponent<ParticleSystem>().startSpeed = Mathf.Lerp(1.0f, maxSpeed, rate);
    }
}

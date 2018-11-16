using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーの衝突.
/// </summary>
public class PlayerCollider : MonoBehaviour {

    [SerializeField]
    private float speedDown = 2.0f;

    private PlayerController controller;
    private bool damage = true;

	void Start () 
    {
        // 控制器
        controller = GetComponent<PlayerController>();
	}

    void OnGameOver()
    {
        damage = false;
    }
    void OnGameClear()
    {
        damage = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (damage) CheckDamageCollision(collision.gameObject);
        else CheckTerrainCollision(collision.gameObject);
    }
    void OnCollisionStay(Collision collision)
    {
        if (damage) CheckDamageCollision(collision.gameObject);
    }

    private void CheckDamageCollision(GameObject target)
    {
        // 稍微进行减速调整（如果速度太快的话将无法explosion）
        if (target.CompareTag("Torpedo"))
        {
            controller.AddSpeed( -speedDown );
        }
    }

    private void CheckTerrainCollision(GameObject target)
    {
        // 和地面接触后播放沉没的声音
        if (target.CompareTag("Terrain"))
        {
            PlayAudioAtOnce();
        }
    }

    private void PlayAudioAtOnce()
    {
        if (!GetComponent<AudioSource>()) return;
        if (GetComponent<AudioSource>().isPlaying) return;
        GetComponent<AudioSource>().Play();
    }
}

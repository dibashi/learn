using UnityEngine;
using System.Collections;

public class Treasure : MonoBehaviour {

	public GameObject m_pickupEffect;	// 拾取时的特效
	public AudioClip m_SEPickuped; 		// 拾取时的的SE
	public AudioClip m_SEAppear; 		// 拾取时的的SE
	public int m_point = 1000;			// 拾取时的的得分
	
	// 消失时间
	public float m_lifeTime = 10.0f;
	
	// 初始化
	void Start () {
		AudioChannels audio = FindObjectOfType(typeof(AudioChannels)) as AudioChannels;
		if (audio != null)
			audio.PlayOneShot(m_SEAppear,1.0f,0.0f);
		Destroy(gameObject,m_lifeTime);
	}
	
	// Update
	void Update () {
	}
	
	public void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerController>() != null) {  // todo: 通过组件来判断是否为玩家可能会更好？.

			Hud.get().AddScore(m_point);
			Hud.get().CreateScoreBoard(this.transform.position, m_point);

			// 生成特效
			GameObject o = (GameObject)Instantiate(m_pickupEffect.gameObject,transform.position  + new Vector3(0,1.0f,0),Quaternion.identity);
			
			// 音效
			(FindObjectOfType(typeof(AudioChannels)) as AudioChannels).PlayOneShot(m_SEPickuped,1.0f,0.0f);
			Destroy(o,3.0f);
			Destroy(gameObject);
		}
	}
}

using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 生成玩家的射击
// ----------------------------------------------------------------------------
public class PlayerShotMaker : MonoBehaviour {
	
	public float fireInterval = 0.1f;		// 炮弹之间的间隔
	public float fireSetInterval = 0.3f;	// 等待下三发炮弹发射的时间
	public int fireOrderMax = 1;			// 发射指令的最大数量
	
	public GameObject shot;					// 炮弹
	
	public GameObject effectShot;
	
	private GameObject effectShot1;
	private GameObject effectShot2;
	private GameObject effectShot3;
	private ParticleSystem particleEffectShot1;		// 射击特效
	private ParticleSystem particleEffectShot2;		// 射击特效
	private ParticleSystem particleEffectShot3;		// 射击特效
	
	private ArrayList fireOrders = new ArrayList();	// 发射指令
	
	private bool isFiring = false;			// 正在发射
	
	void Start () 
	{
		// 获取射击特效的实例
		effectShot1 = Instantiate( effectShot, Vector3.zero, new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
		particleEffectShot1 = effectShot1.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
		effectShot2 = Instantiate( effectShot, Vector3.zero, new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
		particleEffectShot2 = effectShot2.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
		effectShot3 = Instantiate( effectShot, Vector3.zero, new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
		particleEffectShot3 = effectShot3.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
	}
	
	void Update ()
	{
		
		ReadFireOrder();
	}
	
	// Fire -> Memory
	public void SetFireOrder()
	{
		if ( fireOrders.Count < fireOrderMax )
		{
			fireOrders.Add( true );
		}
	}
	
	// Memory -> Fire
	private void ReadFireOrder()
	{
		if ( fireOrders.Count > 0 )
		{
			if ( !isFiring )
			{
				isFiring = true;
				fireOrders.RemoveAt(0);
				StartCoroutine( "Firing" );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 隔一定间隔射出3发炮弹
	// ------------------------------------------------------------------------
	IEnumerator Firing()
	{
		// 是否存在射击特效？
		if ( shot )
		{
			// 生成炮弹1
			Instantiate( shot, transform.position, transform.rotation );
			effectShot1.transform.rotation = transform.rotation;
			effectShot1.transform.position = effectShot1.transform.forward * 1.5f;
			// 修正特效的角度
			//  - 修正素材的方向。加上90f是为了修正素材的方向
			//  - 除以57.29578f后会出现在玩家的前方位置（如果ParticleSystem的startRotation不执行则会出现偏移）
			particleEffectShot1.startRotation = ( transform.rotation.eulerAngles.y + 90f ) / 57.29578f;
			// 播放特效
			particleEffectShot1.Play();
			
			// 等待一定的时间
			yield return new WaitForSeconds( fireInterval );
			
			// 生成炮弹2
			Instantiate( shot, transform.position, transform.rotation );
			effectShot2.transform.rotation = transform.rotation;
			effectShot2.transform.position = effectShot2.transform.forward * 1.5f;
			// 修正特效的角度
			//  - 修正素材的方向。加上90f是为了修正素材的方向
			//  - 除以57.29578f后会出现在玩家的前方位置（如果ParticleSystem的startRotation不执行则会出现偏移）
			particleEffectShot2.startRotation = ( transform.rotation.eulerAngles.y + 90f ) / 57.29578f;
			// 播放特效
			particleEffectShot2.Play();
			
			// 等待一定的时间
			yield return new WaitForSeconds( fireInterval );
			
			// 生成炮弹3
			Instantiate( shot, transform.position, transform.rotation );
			effectShot3.transform.rotation = transform.rotation;
			effectShot3.transform.position = effectShot3.transform.forward * 1.5f;
			// 修正特效的角度
			//  - 修正素材的方向。加上90f是为了修正素材的方向
			//  - 除以57.29578f后会出现在玩家的前方位置（如果ParticleSystem的startRotation不执行则会出现偏移）
			particleEffectShot3.startRotation = ( transform.rotation.eulerAngles.y + 90f ) / 57.29578f;
			// 播放特效
			particleEffectShot3.Play();
			
			// 等待一定时间后再发射
			yield return new WaitForSeconds( fireSetInterval );
			
			// 发射结束
			isFiring = false;
		}
	}

}

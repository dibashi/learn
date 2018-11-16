using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour {

	public GameObject 		appearEffectPrefab       = null;		// 开始显示时的特效
	public GameObject 		landingWaterEffectPrefab = null;		// 遇水时的特效
	public GameObject		fightingEffectPrefab     = null;		// 战斗时的特效

	protected GameObject	fighting_effect = null;					// 显示时的初始特效

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// 播放开始显示时的特效
	public void		playAppearEffect(BaseObject bo)
	{
		ParticleSystem	ps = GameObject.Instantiate(this.appearEffectPrefab).GetComponent<ParticleSystem>();


		// 计算位置，使得角色能够在前面显示
		Vector3		bo_center = bo.transform.position + 0.5f*(bo.getYTop() + bo.getYBottom())*Vector3.up;

		Quaternion	qt = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.x, Vector3.right);
		Ray 	ray = new Ray(bo_center, qt*-Vector3.forward);

		// 播放
		ps.transform.position = ray.GetPoint(100.0f);
		ps.Play();

		// 播放结束后删除GameObject
		GameObject.Destroy(ps.gameObject, 1.0f);
	}

	// 播放遇水时的特效
	public void		playLandingWaterEffect(BaseObject bo)
	{
		ParticleSystem	ps = GameObject.Instantiate(this.landingWaterEffectPrefab).GetComponent<ParticleSystem>();

		Vector3 position = bo.transform.position;

		// 高度和水面相同
		position.y = 70.0f;
		// 为了使角色不被水柱挡着稍微往前一些
		position.z -= 70.0f;

		// 播放
		ps.transform.position = position;
		ps.Play();

		// 播放结束后删除该特效GameObject
		GameObject.Destroy(ps.gameObject, 1.0f);
	}

	// 播放战斗时显示的特效
	public void		playFightingEffect(Vector3 position)
	{
		this.fighting_effect = GameObject.Instantiate(this.fightingEffectPrefab);

		this.fighting_effect.transform.position = position;
	}

	// 停止播放战斗时的特效
	public void		stopFightingEffect()
	{
		if(this.fighting_effect != null) {

			GameObject.Destroy(this.fighting_effect);
		}
	}

	// ================================================================ //
	// 实例

	protected static EffectManager	instance = null;

	public static EffectManager	get()
	{
		if(instance == null) {

			GameObject	go = GameObject.FindGameObjectWithTag("System");

			if(go == null) {

				Debug.Log("Can't find \"System\" GameObject.");

			} else {

				instance = go.GetComponent<EffectManager>();
			}
		}
		return(instance);
	}

}

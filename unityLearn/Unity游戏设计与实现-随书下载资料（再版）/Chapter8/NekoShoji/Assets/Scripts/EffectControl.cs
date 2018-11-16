using UnityEngine;
using System.Collections;

public class EffectControl : MonoBehaviour {

	public GameObject	eff_break = null;	// 和纸碰撞时的特效
	public GameObject	eff_miss  = null;	// 和铁板碰撞时的特效

	public GameObject	game_camera = null;

	// ================================================================ //
	// 继承于MonoBehaviour

	void 	Start()
	{
		this.game_camera = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	void	Update()
	{
	
	}

	// ================================================================ //

	// 和纸碰撞时的特效
	public void	createBreakEffect(SyoujiPaperControl paper, NekoControl neko)
	{
		GameObject 	go = Instantiate(this.eff_break) as GameObject;

		go.AddComponent<Effect>();

		Vector3	position = paper.transform.position;

		position.x = neko.transform.position.x;
		position.y = neko.transform.position.y;

		position.y += 0.3f;

		go.transform.position = position;

		// 为了一直显示在屏幕上，设置为摄像机的子结点（可以和摄像机一起移动）
		go.transform.parent = this.game_camera.transform;
	}

	public void	createMissEffect(NekoControl neko)
	{
		GameObject 	go = Instantiate(this.eff_miss) as GameObject;

		go.AddComponent<Effect>();

		Vector3	position = neko.transform.position;

		position.y += 0.3f;

		// 防止陷入铁板中
		position.z -= 0.2f;

		go.transform.position = position;
	}

	// ================================================================ //
	//																	//
	// ================================================================ //

	protected static	EffectControl instance = null;

	public static EffectControl	get()
	{
		if(EffectControl.instance == null) {

			GameObject		go = GameObject.Find("EffectControl");

			if(go != null) {

				EffectControl.instance = go.GetComponent<EffectControl>();

			} else {

				Debug.LogError("Can't find game object \"EffectControl\".");
			}
		}

		return(EffectControl.instance);
	}
}

using UnityEngine;
using System.Collections;

public class AttackColliderControl : MonoBehaviour {

	public PlayerControl	player = null;

	// 正在执行攻击判定？
	private bool		is_powered = false;

	// -------------------------------------------------------------------------------- //

	void Start ()
	{
		this.SetPowered(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
	}

	// OnTriggerEnter 只会在和碰撞对象发生接触的瞬间被调用，
	// 如果在产生攻击判定球时完全嵌入怪物的内侧，
	// 不容易捡出
	//void OnTriggerEnter(Collider other)
	void OnTriggerStay(Collider other)
	{
		do {

			if(!this.is_powered) {

				break;
			}

			if(other.tag != "OniGroup") {
	
				break;
			}

			OniGroupControl	oni = other.GetComponent<OniGroupControl>();

			if(oni == null) {

				break;
			}

			//

			oni.OnAttackedFromPlayer();

			// 重置“不能攻击”计时器（立刻可以攻击）
			this.player.resetAttackDisableTimer();

			// 播放攻击命中特效
			this.player.playHitEffect(oni.transform.position);

			// 发出攻击命中音效
			this.player.playHitSound();

		} while(false);
	}

	public void	SetPowered(bool sw)
	{
		this.is_powered = sw;

		if(SceneControl.IS_DRAW_PLAYER_ATTACK_COLLISION) {

			this.GetComponent<Renderer>().enabled = sw;
		}
	}
}

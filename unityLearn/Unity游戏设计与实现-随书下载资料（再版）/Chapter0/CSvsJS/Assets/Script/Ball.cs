using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

	public Launcher		launcher;		// 发射台对象的预设

	public	Vector3		velocity;		// 初始速度（接受Launcher 传来的值）
	public	bool		is_touched;		// 是否接触到了玩家？
	private	float		time;			// 运行过程中的定时器
	private bool		is_launched;	// 是否已经发射？（false则表示正在淡入过程中）

	// ---------------------------------------------------------------- //

	void Start ()
	{
		// 提前查找出发射台的游戏对象
		this.launcher = GameObject.FindGameObjectWithTag("Launcher").GetComponent<Launcher>();

		// 通过修改alpha通道值让其不可见

		Color	color = this.GetComponent<Renderer>().material.color;

		color.a = 0.0f;

		this.GetComponent<Renderer>().material.color = color;

		//

		this.GetComponent<Rigidbody>().useGravity = false;

		this.is_touched = false;
		this.is_launched = false;

		this.time = 0.0f;
	}

	void Update ()
	{
		// 发射前（淡入过程中）
		if(!this.is_launched) {

			float	delay = 0.5f;

			// 通过修改alpha通道值淡入

			Color color = this.GetComponent<Renderer>().material.color;

			// 将[0.0f ～ delay] 范围变换为 [0.0f ～ 1.0f]
			float	t = Mathf.InverseLerp(0.0f, delay, this.time);

			t = Mathf.Min(t, 1.0f);

			color.a = Mathf.Lerp(0.0f, 1.0f, t);

			this.GetComponent<Renderer>().material.color = color;

			// 经过一定时间后，发射
			if(this.time >= delay) {

				this.GetComponent<Rigidbody>().useGravity = true;
				this.GetComponent<Rigidbody>().velocity = this.velocity;

				this.is_launched = true;
			}
		}

		this.time += Time.deltaTime;

		DebugPrint.print(this.GetComponent<Rigidbody>().velocity.ToString());
	}

	// 跑出画面外时的处理
	void	OnBecameInvisible()
	{
		// 通知发射台销毁小球对象
		this.launcher.OnBallDestroy();

		// 如果未和玩家对象发生碰撞，则失败
		if(!this.is_touched) {

			if(this.launcher != null) {

				this.launcher.setResult(false);
			}
		}

		// 销毁游戏对象
		Destroy(this.gameObject);
	}

	// 和其他对象发生碰撞时的处理
	void OnCollisionEnter(Collision collision)
	{
		// 如果与之发生碰撞的是玩家对象……
		if(collision.gameObject.tag == "Player") {

			if(collision.gameObject.GetComponent<Player>().isLanded()) {

				// 如果玩家已经落地则失败

				this.launcher.setResult(false);

				// 记录下和玩家发生碰撞的状态
				this.is_touched = true;

			} else {

				// 如果玩家在跳跃过程中则成功

				this.launcher.setResult(true);
				this.is_touched = true;
			}
		}
	}
}

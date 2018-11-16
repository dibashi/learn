using UnityEngine;
using System.Collections;

public class OniControl : MonoBehaviour {

	// 玩家
	public PlayerControl		player = null;

	// 摄像机
	public GameObject	main_camera = null;

	// 碰撞盒的大小（1条边的长度）
	public const float collision_size = 0.5f;

	// 依然活着？
	private bool	is_alive = true;

	// 生成时的位置
	private Vector3	initial_position;

	// 左右波动时的波动周期
	public float	wave_angle_offset = 0.0f;

	// 左右波动时的幅度
	public float	wave_amplitude = 0.0f;

	// 怪物的状态
	enum STEP {

		NONE = -1,

		RUN = 0,			// 跑着逃开
		DEFEATED,			// 被斩杀并飞散开

		NUM,
	};

	// 当前的状态
	private	STEP		step      = STEP.NONE;

	// 下次迁移的状态
	private	STEP		next_step = STEP.NONE;

	// [sec] 状态迁移后的时间
	private float		step_time = 0.0f;

	// DEFEATED, FLY_TO_STACK 开始时的速度向量
	private Vector3		blowout_vector = Vector3.zero;
	private Vector3		blowout_angular_velocity = Vector3.zero;

	// -------------------------------------------------------------------------------- //

	void 	Start()
	{
		// 生成时的位置
		this.initial_position = this.transform.position;

		this.transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up);

		this.GetComponent<Collider>().enabled = false;

		// 不限制旋转的速度
		this.GetComponent<Rigidbody>().maxAngularVelocity = float.PositiveInfinity;

		// 模型的中心略靠下，重心作适当偏移
		//this.GetComponent<Rigidbody>().centerOfMass = new Vector3(0.0f, 0.5f, 0.0f);
	}

	void	Update()
	{
		this.step_time += Time.deltaTime;

		// 检查状态迁移
		// （此时，如果没有来自外部的请求将不会发生迁移）

		switch(this.step) {

			case STEP.NONE:
			{
				this.next_step = STEP.RUN;
			}
			break;
		}

		// 初始化
		// 状态发生迁移时的初始化处理

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.DEFEATED:
				{
					this.GetComponent<Rigidbody>().velocity = this.blowout_vector;

					// 旋转的角速度
					this.GetComponent<Rigidbody>().angularVelocity = this.blowout_angular_velocity;

					// 解除父子关系
					// 因为父对象（OniGroup）被删除时子对象也会被删除
					this.transform.parent = null;
			
					// 在摄像机的坐标系内运动
					// （和摄像机一起连动）
					if(SceneControl.IS_ONI_BLOWOUT_CAMERA_LOCAL) {
			
						this.transform.parent = this.main_camera.transform;
					}

					// 播放被攻击的动作
					this.transform.GetChild(0).GetComponent<Animation>().Play("oni_yarare");

					this.is_alive = false;

					// 隐藏阴影
					foreach(var renderer in this.GetComponentsInChildren<SkinnedMeshRenderer>()) {
					
						renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					}
				}
				break;
			}

			this.step = this.next_step;

			this.next_step = STEP.NONE;

			this.step_time = 0.0f;
		}

		// 各个状态的执行处理

		Vector3	new_position = this.transform.position;

		float low_limit = this.initial_position.y;

		switch(this.step) {

			case STEP.RUN:
			{
				// 活着的时候使它不会陷入地面中

				if(new_position.y < low_limit) {
		
					new_position.y = low_limit;
				}
	
				// 左右波动
	
				float	wave_angle = 2.0f*Mathf.PI*Mathf.Repeat(this.step_time, 1.0f) + this.wave_angle_offset;
	
				float	wave_offset = this.wave_amplitude*Mathf.Sin(wave_angle);
	
				new_position.z = this.initial_position.z + wave_offset;
	
				// 方向（Y轴旋转）也随之变化
				if(this.wave_amplitude > 0.0f) {
	
					this.transform.rotation = Quaternion.AngleAxis(180.0f - 30.0f*Mathf.Sin(wave_angle + 90.0f), Vector3.up);
				}

			}
			break;

			case STEP.DEFEATED:
			{
				// 死后的短时间内可能会陷入地面中，速度朝上（＝死后的瞬间）时，让其不落入地面中
				if(new_position.y < low_limit) {
	
					if(this.GetComponent<Rigidbody>().velocity.y > 0.0f) {
	
						new_position.y = low_limit;
					}
				}
	
				// 稍微向后移动
				if(this.transform.parent != null) {
	
					this.GetComponent<Rigidbody>().velocity += -3.0f*Vector3.right*Time.deltaTime;
				}
			}
			break;

		}

		this.transform.position = new_position;

		// 不需要时就删除
		//
		// ・跑出画面外时
		// ・被斩杀后
		// ・停止播放SE时
		//
		// OnBecameInvisible() 只在跑出画面外的瞬间才会被调用
		// 因此在“跑出画面后持续播放音效后一阵子了”再想删除对象是无法做到的
		//

		do {

			// 由于在画面外生成怪物（怪物分组），在生成的瞬间
			// 也会被调用。因此，要通过检测this.is_alive 来确保
			// 只有死亡后出现在画面之外时，才会删除
			if(this.GetComponent<Renderer>().isVisible) {

				break;
			}

			if(this.is_alive) {

				break;
			}

			// 播放SE 的时候不执行删除
			if(this.GetComponent<AudioSource>().isPlaying) {

				if(this.GetComponent<AudioSource>().time < this.GetComponent<AudioSource>().clip.length) {

					break;
				}
			}

			//

			Destroy(this.gameObject);

		} while(false);
	}

	// 设置动作的播放速度
	public void setMotionSpeed(float speed)
	{
		this.transform.GetChild(0).GetComponent<Animation>()["oni_run1"].speed = speed;
		this.transform.GetChild(0).GetComponent<Animation>()["oni_run2"].speed = speed;
	}

	// 开始执行被攻击时的处理
	public void AttackedFromPlayer(Vector3 blowout, Vector3	angular_velocity)
	{
		this.blowout_vector           = blowout;
		this.blowout_angular_velocity = angular_velocity;

		// 解除父子关系
		// 父对象（OniGroup）被删除后将被一并删除
		this.transform.parent = null;

		this.next_step = STEP.DEFEATED;
	}

}

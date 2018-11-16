using UnityEngine;
using System.Collections;

public class OniGroupControl : MonoBehaviour {

	// 玩家
	public PlayerControl	player = null;

	// 摄像机
	public GameObject	main_camera = null;

	// 场景控制
	public SceneControl	scene_control = null;

	// 怪物的预设
	public GameObject[]	OniPrefab;
	
	public AudioClip[]	YarareLevel1;
	public AudioClip[]	YarareLevel2;
	public AudioClip[]	YarareLevel3;

	// 分组内的 OniPrefab 实例
	public OniControl[]	onis;

	// -------------------------------------------------------------------------------- //

	// 碰撞盒的尺寸（1条边的长度）
	public static float collision_size = 2.0f;

	// 分组内的怪物数量
	private	int		oni_num;
	
	// 到目前为止的怪物最大数
	static private int	oni_num_max = 0;

	// 分组整体前进的速度
	public float	run_speed = SPEED_MIN;

	// 是否和玩家发生了接触？
	public bool	is_player_hitted = false;

	// -------------------------------------------------------------------------------- //

	// 类型

	public enum TYPE {

		NONE = -1,

		NORMAL = 0,			// 普通

		DECELERATE,			// 中途减速
		LEAVE,				// 在画面右侧迅速立刻（玩家失败后）
		NUM,
	};

	public TYPE		type = TYPE.NORMAL;

	// 速度控制的信息（TYPE = DECELERATE 时）
	public struct Decelerate {

		public bool		is_active;			// 正在减速中？
		public float	speed_base;			// 开始减速前的速度
		public float	timer;
	};

	public Decelerate	decelerate;

	// -------------------------------------------------------------------------------- //

	public static float		SPEED_MIN = 2.0f;			// 移动速度的最小值
	public static float		SPEED_MAX = 10.0f;			// 移动速度的最高值
	public static float		LEAVE_SPEED = 10.0f;		// 退场时的速度

	// ================================================================ //

	void	Start()
	{
		// 显示碰撞器（用于调试）
		this.gameObject.GetComponent<Renderer>().enabled = SceneControl.IS_DRAW_ONI_GROUP_COLLISION;

		this.decelerate.is_active = false;
		this.decelerate.timer     = 0.0f;
	}

	void	Update()
	{
		this.speed_control();

		this.transform.rotation = Quaternion.identity;

		// 退场模式情况下，出了画面之外就删除
		// （由于renderer被设置为disable，因此OnBecameInvisible无法使用）
		//
		if(this.type == TYPE.LEAVE) {

			// 分组内的怪物全部移动到画面外后，将整个分组删除

			bool	is_visible = false;

			foreach(var oni in this.onis) {

				if(oni.GetComponent<Renderer>().isVisible) {

					is_visible = true;
					break;
				}
			}

			if(!is_visible) {

				Destroy(this.gameObject);
			}
		}
	}

	void FixedUpdate()
	{
		Vector3	new_position = this.transform.position;

		new_position.x += this.run_speed*Time.deltaTime;

		this.transform.position = new_position;
	}

	// ================================================================ //

	// 控制奔跑的速度
	private void	speed_control()
	{
		switch(this.type) {

			case TYPE.DECELERATE:
			{
				// 和玩家的距离小于下列值，开始减速
				//
				const float	decelerate_start = 8.0f;

				if(this.decelerate.is_active) {

					// 1.加速逃跑
					// 2.和玩家保持相同速度逐渐靠近
					// 3.让情况变得“糟糕了”立刻减速

					float	rate;

					const float		time0 = 0.7f;
					const float		time1 = 0.4f;
					const float		time2 = 2.0f;

					const float		speed_max = 30.0f;
					      float		speed_min = OniGroupControl.SPEED_MIN;

					float	time = this.decelerate.timer;

					do {

						// 加速

						if(time < time0) {

							rate = Mathf.Clamp01(time/time0);
							rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, rate)) + 1.0f)/2.0f;

							this.run_speed = Mathf.Lerp(this.decelerate.speed_base, speed_max, rate);

							this.set_oni_motion_speed(2.0f);

							break;
						}
						time -= time0;

						// 减速直到和玩家速度相同

						if(time < time1) {

							rate = Mathf.Clamp01(time/time1);
							rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, rate)) + 1.0f)/2.0f;

							this.run_speed = Mathf.Lerp(speed_max, PlayerControl.RUN_SPEED_MAX, rate);

							break;
						}
						time -= time1;

						// 减速直到速度变得非常低

						if(time < time2) {

							rate = Mathf.Clamp01(time/time2);
							rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, rate)) + 1.0f)/2.0f;

							this.run_speed = Mathf.Lerp(PlayerControl.RUN_SPEED_MAX, speed_min, rate);

							this.set_oni_motion_speed(1.0f);

							break;
						}
						time -= time2;

						//

						this.run_speed = speed_min;

					} while(false);

					this.decelerate.timer += Time.deltaTime;

				} else {

					float	distance = this.transform.position.x - this.player.transform.position.x;

					if(distance < decelerate_start) {

						this.decelerate.is_active  = true;
						this.decelerate.speed_base = this.run_speed;
						this.decelerate.timer      = 0.0f;
					}
				}
			}
			break;

			case TYPE.LEAVE:
			{
				// プレイヤーに追い付かれないよう、プレイヤーのスピードを足す.
				this.run_speed = LEAVE_SPEED + this.player.run_speed;
			}
			break;

		}

	}

	// 生成怪物的分组
	public void	CreateOnis(int oni_num, Vector3 base_position)
	{
		this.oni_num = oni_num;
		oni_num_max = Mathf.Max( oni_num_max, oni_num );
		
		this.onis = new OniControl[this.oni_num];
		
		Vector3		position;

		for(int i = 0;i < this.oni_num;i++) {

			GameObject	go = Instantiate(this.OniPrefab[i%this.OniPrefab.Length]) as GameObject;

			this.onis[i] = go.GetComponent<OniControl>();

			// 随机分散怪物的位置

			position = base_position;

			if(i == 0) {

				// 因为必须让一个怪物和玩家正面接触，
				// 所以这里不对第0号怪物设置偏移
			
			} else {

				// 使用随机数分散位置

				Vector3	splat_range;
				
				// 分组内的怪物数（一次性出现的数量）越多，分散的范围就越广
				splat_range.x = OniControl.collision_size*(float)(oni_num - 1);
				splat_range.z = OniControl.collision_size*(float)(oni_num - 1)/2.0f;

				// 为了防止分散的范围太大
				// 将其限制在玩家挥刀能触及的区域外
				splat_range.x = Mathf.Min(splat_range.x, OniGroupControl.collision_size);
				splat_range.z = Mathf.Min(splat_range.z, OniGroupControl.collision_size/2.0f);
			
				position.x += Random.Range(0.0f, splat_range.x);
				position.z += Random.Range(-splat_range.z, splat_range.z);
			}

			position.y = 0.0f;
			
			this.onis[i].transform.position = position;
			this.onis[i].transform.parent = this.transform;

			this.onis[i].player      = this.player;
			this.onis[i].main_camera = this.main_camera;

			this.onis[i].wave_amplitude    = (i + 1)*0.1f;
			this.onis[i].wave_angle_offset = (i + 1)*Mathf.PI/4.0f;
		}
	}

	private static int	count = 0;

	// 受到玩家的攻击时
	public void OnAttackedFromPlayer()
	{

		// 累加被击倒的怪物数量
		// （后续部分也会进行计算评价，不过这里先执行一次）
		this.scene_control.AddDefeatNum(this.oni_num);

		// 怪物向四处飞散
		//
		// 在圆锥表面的形状上决定各个怪物飞散开的方向
		// 评价越高则圆锥的开口越大，这样就能飞散到更广的区域
		// 玩家的速度如果较快，圆锥会向前倾斜一些

		Vector3			blowout;				// 怪物飞散的方向（速度向量）
		Vector3			blowout_up;				// ↑的垂直分量
		Vector3			blowout_xz;				// ↑的水平分量

		float			y_angle;
		float 			blowout_speed;
		float			blowout_speed_base;

		float			forward_back_angle;		// 圆锥的前后倾斜角度

		float			base_radius;			// 圆锥的地面半径

		float			y_angle_center;
		float			y_angle_swing;			// 圆弧的中心（根据动作左右决定该值）

		float			arc_length;				// 圆弧的长度（圆周）

		switch(this.scene_control.evaluation) {

			default:
			case SceneControl.EVALUATION.OKAY:
			{
				base_radius = 0.3f;

				blowout_speed_base = 10.0f;

				forward_back_angle = 40.0f;

				y_angle_center = 180.0f;
				y_angle_swing  = 10.0f;
			}
			break;

			case SceneControl.EVALUATION.GOOD:
			{
				base_radius = 0.3f;

				blowout_speed_base = 10.0f;

				forward_back_angle = 0.0f;

				y_angle_center = 0.0f;
				y_angle_swing = 60.0f;
			}
			break;

			case SceneControl.EVALUATION.GREAT:
			{
				base_radius = 0.5f;

				blowout_speed_base = 15.0f;

				forward_back_angle = -20.0f;

				y_angle_center = 0.0f;
				y_angle_swing = 30.0f;
			}
			break;
		}

		forward_back_angle += Random.Range(-5.0f, 5.0f);

		arc_length = (this.onis.Length - 1)*30.0f;
		arc_length = Mathf.Min(arc_length, 120.0f);

		// 根据玩家的动作（左斩，右斩），改变左右飞散的方向

		y_angle = y_angle_center;

		y_angle += -arc_length/2.0f;

		if(this.player.attack_motion == PlayerControl.ATTACK_MOTION.RIGHT) {

			y_angle += y_angle_swing;

		} else {

			y_angle -= y_angle_swing;
		}

		y_angle += ((OniGroupControl.count*7)%11)*3.0f;

		// 让组内的怪物全部被击倒
		foreach(OniControl oni in this.onis) {

			//

			blowout_up = Vector3.up;

			blowout_xz = Vector3.right*base_radius;
			blowout_xz = Quaternion.AngleAxis(y_angle, Vector3.up)*blowout_xz;

			blowout = blowout_up + blowout_xz;

			blowout.Normalize();

			// 圆周向前后倾斜

			blowout = Quaternion.AngleAxis(forward_back_angle, Vector3.forward)*blowout;

			// 飞散开的速度

			blowout_speed = blowout_speed_base*Random.Range(0.8f, 1.2f);
			blowout *= blowout_speed;

			if(!SceneControl.IS_ONI_BLOWOUT_CAMERA_LOCAL) {

				// 全局坐标系下飞散开（不和摄像机发生连动）时，
				// 要加上玩家的速度
				blowout += this.player.GetComponent<Rigidbody>().velocity;
			}

			// 旋转

			Vector3	angular_velocity = Vector3.Cross(Vector3.up, blowout);

			angular_velocity.Normalize();
			angular_velocity *= 3.14f*8.0f*blowout_speed/15.0f*Random.Range(0.5f, 1.5f);

			//angular_velocity = Quaternion.AngleAxis(Random.Range(-30.0f, 30.0f), Vector3.up)*angular_velocity;

			//

			oni.AttackedFromPlayer(blowout, angular_velocity);

			//Debug.DrawRay(this.transform.position, blowout*2.0f, Color.white, 1000.0f);

			//

			y_angle += arc_length/(this.onis.Length - 1);

		}

		// 播放被击倒的音效
		// 太多的音效同时播放不容易听清，只播放一个
		//
		if(this.onis.Length > 0)
		{
			AudioClip[]	yarareSE = null;
			
			if( this.onis.Length >= 1 && this.onis.Length < 3 )
			{
				yarareSE = this.YarareLevel1;
			}
			else if( this.onis.Length >= 3 && this.onis.Length < 8 )
			{
				yarareSE = this.YarareLevel2;
			}
			else if( this.onis.Length >= 8 )
			{
				yarareSE = this.YarareLevel3;
			}
			
			if( yarareSE != null )
			{
				int index = Random.Range( 0, yarareSE.Length );
				
				this.onis[0].GetComponent<AudioSource>().clip = yarareSE[index];
				this.onis[0].GetComponent<AudioSource>().Play();
			}
		}

		OniGroupControl.count++;

		// 删除实例
		//
		// 执行Destroy(this) 后，　删除的不是OniGroupPrefab 实例，而是脚本（OniGroupControl）
		// 请注意
		//
		Destroy(this.gameObject);

	}

	// -------------------------------------------------------------------------------- //

	// 和玩家发生接触时的处理
	public void	onPlayerHitted()
	{
		this.scene_control.result.score_max += this.scene_control.eval_rate_okay * oni_num_max * this.scene_control.eval_rate;
		this.is_player_hitted = true;
	}

	// 开始退场处理
	public void	beginLeave()
	{
		this.GetComponent<Collider>().enabled = false;
		this.type = TYPE.LEAVE;
	}

	// 设置怪物动作的播放速度
	private void	set_oni_motion_speed(float speed)
	{
		foreach(OniControl oni in this.onis) {

			oni.setMotionSpeed(speed);
		}
	}

}

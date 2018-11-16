using UnityEngine;
using System.Collections;

public class NekoControl : MonoBehaviour {

	private RoomControl		room_control = null;
	private SceneControl	scene_control = null;
	public EffectControl	effect_control = null;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		STAND = 0,			// 站立
		RUN,				// 跑动
		JUMP,				// 起跳
		MISS,				// 失败
		GAMEOVER,			// 游戏结束

		FREE_MOVE,			// 自由移动（用于调试）

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;
	public bool			is_grounded;				// 是否落地？

	// ---------------------------------------------------------------- //

	// 站立状态的数据结构
	public struct ActionStand {

		public bool		is_fade_anim;				// 淡入淡出动画？（每帧都返回true）
	};

	// 跳跃状态的数据结构
	public struct ActionJump {

		public STEP		prevoius_step;				// 跳跃前的step（站立跳跃 or 奔跑中跳跃）

		public bool		is_key_released;			// 跳跃后，是否松开空格键？

		public Vector3	launch_velocity_xz;
	};

	// 失败状态数据结构
	public struct ActionMiss {

		public bool		is_steel;					// 是否和铁板碰撞？
	};

	public ActionJump	action_jump;
	public ActionMiss	action_miss;
	public ActionStand	action_stand;

	public Vector3		previous_velocity;

	private	bool		is_fallover = true;
		
	private	bool		is_auto_drive = false;		// 自动运行（清空以后）

	// ---------------------------------------------------------------- //

	public const float	JUMP_HEIGHT_MAX = 5.0f;								// 跳跃的高度
	public const float	JUMP_KEY_RELEASE_REDUCE = 0.5f;						// 跳跃中松开按键时，上升速度的衰减率

	public const float	RUN_SPEED_MAX   = 5.0f;								// 奔跑速度的最大值
	public const float	RUN_ACCELE      = RUN_SPEED_MAX/2.0f;				// 奔跑的加速度

	public const float	SLIDE_SPEED_MAX = 2.0f;								// 左右移动的速度
	public const float	SLIDE_ACCEL     = SLIDE_SPEED_MAX/0.1f;				// 左右移动的加速度

	public const float SLIDE_ACCEL_SCALE_JUMP = 0.1f;						// 左右移动加速度的衰减率（跳跃中）

	public const float	RUN_SPEED_DECELE_MISS      = RUN_SPEED_MAX/2.0f;	// 失败时的减速度
	public const float	RUN_SPEED_DECELE_MISS_JUMP = RUN_SPEED_MAX/5.0f;	// 失败时的减速度（跳跃中）

	public static Vector3 COLLISION_OFFSET = Vector3.up*0.2f;

	// ---------------------------------------------------------------- //

	public const float SLIDE_ROTATION_MAX         = 0.2f;						// 左右移动的旋转速度
	public const float SLIDE_ROTATION_SPEED       = SLIDE_ROTATION_MAX/0.1f;	// 左右移动的旋转加速度
	public const float SLIDE_ROTATION_COEFFICIENT = 2.0f;						// 左右移动的旋转加速度系数

	public const float JUMP_ROTATION_MAX         = 0.25f;						// 上下旋转速度（跳跃中）
	public const float JUMP_ROTATION_SPEED       = JUMP_ROTATION_MAX/0.1f;		// 上下的旋转加速度（跳跃中）
	public const float JUMP_ROTATION_COEFFICIENT = 0.25f;						// 上下旋转加速度的系数（跳跃中）

	public const float SLIDE_VELOCITY = 1.0f;									// 左右移动的旋转速度
	public const float JUMP_VELOCITY  = 4.0f;									// 上下的旋转速度（跳跃中）
	
	// ---------------------------------------------------------------- //

	public AudioClip START_SOUND         = null;
	public AudioClip FAILED_STEEL_SOUND  = null;
	public AudioClip FAILED_FUSUMA_SOUND = null;
	public AudioClip FAILED_NEKO_SOUND   = null;
	public AudioClip JUMP_SOUND          = null;
	public AudioClip LANDING_SOUND       = null;
	public AudioClip FALL_OVER_SOUND     = null;

	// ---------------------------------------------------------------- //

	NekoColiResult	coli_result;

	// ================================================================ //
	// 继承于MonoBehaviour 

	void 	Start()
	{
		this.room_control   = RoomControl.get();
		this.scene_control  = SceneControl.get();
		this.effect_control = EffectControl.get();

		//

		this.is_grounded = false;

		GetComponent<AudioSource>().clip = START_SOUND;
		GetComponent<AudioSource>().Play();

		this.previous_velocity = Vector3.zero;

		this.next_step = STEP.STAND;

		this.coli_result = new NekoColiResult();
		this.coli_result.neko = this;
		this.coli_result.create();
	
		this.action_stand.is_fade_anim = true;	

	}

	// Update is called once per frame
	void Update ()
	{
		Animator	animator = this.GetComponentInChildren<Animator>();

		// ---------------------------------------------------------------- //

		// 因为落地时会陷入地面中
		// （虽然这样很不好）

		if(this.transform.position.y < 0.0f) {

			this.is_grounded = true;

			Vector3	pos = this.transform.position;

			pos.y = 0.0f;

			this.transform.position = pos;
		}
		
		// ---------------------------------------------------------------- //
		// step内累加经过时间

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 检测是否迁移到下一个状态

		// 检测上一帧的碰撞结果

		if(this.step != STEP.MISS) {

			this.coli_result.resolveCollision();
		}

		//

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.NONE:
				{
					this.next_step = STEP.STAND;
				}
				break;
	
				case STEP.STAND:
				{
					// 按下Shift键开始奔跑
					if(Input.GetKeyDown(KeyCode.LeftShift)) {
	
						this.next_step = STEP.RUN;
					}
					// 空格键跳跃
					if(Input.GetKeyDown(KeyCode.Space)) {
	
						this.next_step = STEP.JUMP;
					}
				}
				break;
	
				case STEP.RUN:
				{
					if(!this.is_auto_drive) {

						if(Input.GetKeyDown(KeyCode.Space)) {
		
							this.next_step = STEP.JUMP;
						}
					}
				}
				break;

				case STEP.JUMP:
				{
					// 落地后切换为站立，或者奔跑
					if(this.is_grounded) {
					
						GetComponent<AudioSource>().clip = LANDING_SOUND;
						GetComponent<AudioSource>().Play();
						this.next_step = this.action_jump.prevoius_step;
					}
				}
				break;

				case STEP.MISS:
				{
					if(this.step_timer > 3.0f) {
					
						//GameObject.FindWithTag("MainCamera").transform.SendMessage("applyDamage", 1);
						SceneControl.get().onPlayerMissed();

						if(this.scene_control.getLifeCount() > 0) {

							this.transform.position = this.room_control.getRestartPosition();

							this.room_control.onRestart();

							// 不对动画进行补间
							this.action_stand.is_fade_anim = false;
						
							this.next_step = STEP.STAND;

						} else {

							this.next_step = STEP.GAMEOVER;
						}
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 状态迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.STAND:
				{
					Vector3 v = this.GetComponent<Rigidbody>().velocity;

					v.x = 0.0f;
					v.z = 0.0f;

					this.GetComponent<Rigidbody>().velocity = v;

					// 播放站立动画
					if(this.action_stand.is_fade_anim) {

						animator.SetTrigger("standing");

					} else {

						animator.SetTrigger("standing_no_fade");
					}
					this.action_stand.is_fade_anim = true;	
				}
				break;

				case STEP.RUN:
				{
					animator.SetTrigger("running");
				}
				break;

				case STEP.JUMP:
				{
					Vector3	v = this.GetComponent<Rigidbody>().velocity;

					v.y = Mathf.Sqrt(2.0f*9.8f*JUMP_HEIGHT_MAX);

					this.GetComponent<Rigidbody>().velocity = v;

					//

					this.action_jump.is_key_released = false;
					this.action_jump.prevoius_step   = this.step;

					this.action_jump.launch_velocity_xz = this.GetComponent<Rigidbody>().velocity;
					this.action_jump.launch_velocity_xz.y = 0.0f;

					//

					animator.SetTrigger("jump");

					GetComponent<AudioSource>().clip = JUMP_SOUND;
					GetComponent<AudioSource>().Play();
				}
				break;

				case STEP.MISS:
				{				
					// 向后跳回

					Vector3 v = this.GetComponent<Rigidbody>().velocity;

					v.z *= -0.5f;

					this.GetComponent<Rigidbody>().velocity = v;
						
					// 特效
					this.effect_control.createMissEffect(this);

					// 和铁板碰撞时的声音 or 和拉门碰撞的声音
					//
					if(this.action_miss.is_steel) {

						GetComponent<AudioSource>().PlayOneShot(FAILED_STEEL_SOUND);

					} else {

						GetComponent<AudioSource>().PlayOneShot(FAILED_FUSUMA_SOUND);
					}

					// 撞破纸张声音
					//
					GetComponent<AudioSource>().PlayOneShot(FAILED_NEKO_SOUND);
					
					animator.SetTrigger("failed_1");

					this.coli_result.lock_target.enable = false;

					this.is_fallover = false;

					SceneControl.get().onPlayerHitted();
				}
				break;

				case STEP.FREE_MOVE:
				{
					this.GetComponent<Rigidbody>().useGravity = false;

					this.GetComponent<Rigidbody>().velocity = Vector3.zero;
				}
				break;

			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 各个状态的执行处理

		// 左右移动和跳跃引起的旋转
		this.rotation_control();

		switch(this.step) {

			case STEP.STAND:
			{
			}
			break;

			case STEP.RUN:
			{
				// 向前加速

				Vector3	v = this.GetComponent<Rigidbody>().velocity;

				v.z += (RUN_ACCELE)*Time.deltaTime;

				v.z = Mathf.Clamp(v.z, 0.0f, RUN_SPEED_MAX);

				// 向左右平行移动

				if(this.is_auto_drive) {

					v = this.side_move_auto_drive(v, 1.0f);

				} else {

					v = this.side_move(v, 1.0f);
				}

				//

				this.GetComponent<Rigidbody>().velocity = v;
			}
			break;

			case STEP.JUMP:
			{
				Vector3 v = this.GetComponent<Rigidbody>().velocity;

				// 跳跃中松开按键，上升速度将减小
				// （长时间按住按键可以控制跳跃的高度）

				do {

					if(!Input.GetKeyUp(KeyCode.Space)) {
						break;
					}

					// 一旦松开后不能再控制（连续按键对策）
					if(this.action_jump.is_key_released) {
						break;
					}

					// 下降过程中不能控制
					if(this.GetComponent<Rigidbody>().velocity.y <= 0.0f) {
						break;
					}

					//

					v.y *= JUMP_KEY_RELEASE_REDUCE;

					this.GetComponent<Rigidbody>().velocity = v;

					this.action_jump.is_key_released = true;

				} while(false);

				// 往左右平行移动
				// （期望在跳跃中多少能进行一些控制）
				//
				if(this.is_auto_drive) {

					this.GetComponent<Rigidbody>().velocity = this.side_move_auto_drive(this.GetComponent<Rigidbody>().velocity, SLIDE_ACCEL_SCALE_JUMP);

				} else {

					this.GetComponent<Rigidbody>().velocity = this.side_move(this.GetComponent<Rigidbody>().velocity, SLIDE_ACCEL_SCALE_JUMP);
				}

				//

				// 撞到窗框上时，往格子中心处引导
				if(this.coli_result.shoji_hit_info.is_enable) {
	
					//
	
					v = this.GetComponent<Rigidbody>().velocity;
			
					if(this.coli_result.lock_target.enable) {

						v = this.coli_result.lock_target.position  - this.transform.position;
					}

					v.z = this.action_jump.launch_velocity_xz.z;
						
					this.GetComponent<Rigidbody>().velocity = v;
				}
			}
			break;


			case STEP.MISS:
			{
				// 慢慢减速

				Vector3 v = this.GetComponent<Rigidbody>().velocity;

				v.y = 0.0f;

				float	speed_xz = v.magnitude;

				if(this.is_grounded) {	

					speed_xz -= RUN_SPEED_DECELE_MISS*Time.deltaTime;

				} else {

					speed_xz -= RUN_SPEED_DECELE_MISS_JUMP*Time.deltaTime;
				}

				speed_xz = Mathf.Max(0.0f, speed_xz);

				v.Normalize();

				v *= speed_xz;

				v.y = this.GetComponent<Rigidbody>().velocity.y;

				this.GetComponent<Rigidbody>().velocity = v;

				// 
				do {

					AnimatorStateInfo	state_info = animator.GetCurrentAnimatorStateInfo(0);


					if(this.is_fallover) {
						break;
					}

					if(!this.is_grounded) {
						break;
					}

					// 直到最初的动作（failed_1）再次循环后
					if(!state_info.IsName("failed_1")) {
						break;
					}
					if(state_info.normalizedTime < 1.0f) {
						break;
					}

					animator.SetTrigger("failed_2");

					GetComponent<AudioSource>().clip = FALL_OVER_SOUND;
					GetComponent<AudioSource>().Play();

					this.is_fallover = true;

				} while(false);
			}
			break;

			case STEP.FREE_MOVE:
			{
				float	speed = 400.0f;

				Vector3	v = Vector3.zero;
				
				if(Input.GetKey(KeyCode.RightArrow)) {

					v.x = +speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.LeftArrow)) {

					v.x = -speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.UpArrow)) {

					v.y = +speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.DownArrow)) {

					v.y = -speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.LeftShift)) {

					v.z = +speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.RightShift)) {

					v.z = -speed*Time.deltaTime;
				}

				this.GetComponent<Rigidbody>().velocity = v;
			}
			break;

		}

		// ---------------------------------------------------------------- //

		this.is_grounded = false;

		this.coli_result.shoji_hit_info.is_enable = false;

		this.coli_result.hole_hit_infos.Clear();

		this.coli_result.obstacle_hit_info.is_enable = false;

		this.previous_velocity = this.GetComponent<Rigidbody>().velocity;
	}

	// ---------------------------------------------------------------- //
	// 碰撞相关

	void 	OnCollisionStay(Collision other)
	{
		this.on_collision_common(other);
	}
	void 	OnCollisionEnter(Collision other)
	{
		this.on_collision_common(other);
	}
	private void	on_collision_common(Collision other)
	{
		// 检测是否和窗户发生了碰撞
		//
		do {

			if(other.gameObject.tag != "Syouji") {

				break;
			}

			ShojiControl	shoji_control = other.gameObject.GetComponent<ShojiControl>();

			if(shoji_control == null) {

				break;
			}

			// 记录下和窗户发生了碰撞


			Vector3		position = this.transform.TransformPoint(NekoControl.COLLISION_OFFSET);

			ShojiControl.HoleIndex	hole_index = shoji_control.getClosetHole(position);

			this.coli_result.shoji_hit_info.is_enable = true;
			this.coli_result.shoji_hit_info.hole_index = hole_index;
			this.coli_result.shoji_hit_info.shoji_control = shoji_control;

		} while(false);

		// 是否和拉门发生了碰撞？
		
		do {
		
			if(other.gameObject.tag != "Obstacle") {
		
				break;
			}
		
			this.coli_result.obstacle_hit_info.is_enable = true;
			this.coli_result.obstacle_hit_info.go        = other.gameObject;
			this.coli_result.obstacle_hit_info.is_steel  = false;

		} while(false);
	}
	
	void 	OnTriggerEnter(Collider other)
	{
		this.on_trigger_common(other);
	}
	
	private void	on_trigger_common(Collider other)
	{
		// 穿过格子眼？

		do {

			if(other.gameObject.tag != "Hole") {

				break;
			}


			SyoujiPaperControl	paper_control = other.GetComponent<SyoujiPaperControl>();

			if(paper_control == null) {

				break;
			}

			// 记录下穿过了格子的触发器

			if(paper_control.isSteel()) {

				// 遇到铁板的情况下，按照碰撞到障碍物处理

				this.coli_result.obstacle_hit_info.is_enable = true;
				this.coli_result.obstacle_hit_info.go        = other.gameObject;
				this.coli_result.obstacle_hit_info.is_steel  = true;

			} else {

				// 窗户纸的情况
				NekoColiResult.HoleHitInfo		hole_hit_info;
			
				hole_hit_info.paper_control = paper_control;
			
				this.coli_result.hole_hit_infos.Add(hole_hit_info);
			}

		} while(false);
	}

	// ---------------------------------------------------------------- //

	public void	onRoomProceed()
	{
		this.coli_result.shoji_hit_info_first.is_enable = false;
	}

	public void	beginMissAction(bool is_steel)
	{
		this.GetComponent<Rigidbody>().velocity = this.previous_velocity;
		this.action_miss.is_steel = is_steel;

		this.next_step = STEP.MISS;
	}

	// ---------------------------------------------------------------- //

	// 左右移动，跳跃过程中的旋转
	private void rotation_control()
	{

		// ---------------------------------------------------------------- //
		// 上下的旋转
		Quaternion	current = this.transform.GetChild(0).transform.localRotation;
		Quaternion	rot     = current;

		if(this.transform.position.y > 0.0f || this.step == STEP.JUMP) {		
			// ↑为了方便处理，跳跃动作的第1帧中 y == 0.0f ，
			//   当step 为跳跃动作时的第1帧会执行这个处理
	
			rot.x = -this.GetComponent<Rigidbody>().velocity.y/20.0f;
		
			float	rot_x_diff = rot.x - current.x;
			float	rot_x_diff_limit = 2.0f;

			rot_x_diff = Mathf.Clamp(rot_x_diff, -rot_x_diff_limit*Time.deltaTime, rot_x_diff_limit*Time.deltaTime);

			rot.x = current.x + rot_x_diff;

		} else {
		
			rot.x = current.x;
			rot.x *= 0.9f;
		}

		if(this.step == STEP.MISS) {

			rot.x = current.x;

			if(this.is_grounded) {

				rot.x *= 0.9f;
			}
		}

		// ---------------------------------------------------------------- //
		// 左右的旋转

		rot.y = 0.0f;	
		
		rot.y = this.GetComponent<Rigidbody>().velocity.x/10.0f;
		
		float	rot_y_diff = rot.y - current.y;
		
		rot_y_diff = Mathf.Clamp(rot_y_diff, -0.015f, 0.015f);
		
		rot.y = current.y + rot_y_diff;

	
		rot.z = 0.0f;

		// ---------------------------------------------------------------- //

		// 只旋转子对象（模型）

		this.transform.GetChild(0).transform.localRotation = Quaternion.identity;
		this.transform.GetChild(0).transform.localPosition = Vector3.zero;

		this.transform.GetChild(0).transform.Translate(COLLISION_OFFSET);
		this.transform.GetChild(0).transform.localRotation *= rot;
		this.transform.GetChild(0).transform.Translate(-COLLISION_OFFSET);
	}

	// 往左右平行移动
	private	Vector3	side_move(Vector3 velocity, float slide_speed_scale)
	{

		if(Input.GetKey(KeyCode.LeftArrow)) {

			velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else if(Input.GetKey(KeyCode.RightArrow)) {

			velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else {

			// 左右键中某个被按下后，速度变为0

			if(velocity.x > 0.0f) {

				velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Max(velocity.x, 0.0f);

			} else {

				velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Min(velocity.x, 0.0f);
			}
		}

		velocity.x = Mathf.Clamp(velocity.x, -SLIDE_SPEED_MAX, SLIDE_SPEED_MAX);

		return(velocity);
	}

	// 往左右的平行移动（自动运行）
	private	Vector3	side_move_auto_drive(Vector3 velocity, float slide_speed_scale)
	{
		const float		center_x = 0.0001f;

		if(this.transform.position.x > center_x) {

			velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else if(this.transform.position.x < -center_x) {

			velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else {

			// 左右键中的某一个被按下后，速度将变为0

			if(velocity.x > 0.0f) {

				velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Max(velocity.x, 0.0f);

			} else {

				velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Min(velocity.x, 0.0f);
			}
		}

		// 接近最中心位置时，慢慢减少横向的移动（近似直线前进）
		velocity.x = Mathf.Clamp(velocity.x, -Mathf.Abs(this.transform.position.x), Mathf.Abs(this.transform.position.x));


		velocity.x = Mathf.Clamp(velocity.x, -SLIDE_SPEED_MAX, SLIDE_SPEED_MAX);

		return(velocity);
	}	
	
	// 开始自动运行（清空后）
	public void	beginAutoDrive()
	{
		this.is_auto_drive = true;
	}

}

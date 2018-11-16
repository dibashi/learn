using UnityEngine;
using System.Collections;

// 控制怪物的出现
public class LevelControl {

	// -------------------------------------------------------------------------------- //
	// 预设

	public GameObject	OniGroupPrefab = null;

	// -------------------------------------------------------------------------------- //

	public SceneControl		scene_control = null;
	public PlayerControl	player = null;

	// 生成怪物的位置
	// 玩家的X坐标如果超过这条线，则在玩家前方产生怪物
	private float		oni_generate_line;

	// 距离玩家前方 appear_margin 出产生怪物
	private float		appear_margin = 15.0f;

	// 1个分组的怪物数量（＝一次出现的怪物数量）
	private int			oni_appear_num = 1;

	// 连续成功的次数
	private int			no_miss_count = 0;

	// 怪物的类型
	public enum GROUP_TYPE {

		NONE = -1,

		SLOW = 0,			// 缓慢型
		DECELERATE,			// 中途减速型
		PASSING,			// 小组超越型
		RAPID,				// 间隔极短型

		NORMAL,				// 普通型

		NUM,
	};

	public GROUP_TYPE		group_type      = GROUP_TYPE.NORMAL;
	public GROUP_TYPE		group_type_next = GROUP_TYPE.NORMAL;

	private	bool	can_dispatch = false;

	// 随机控制（游戏一般情况）
	public	bool	is_random = true;

	// 下一个小组的生成位置（一般情况下，距离玩家的偏移值）
	private float			next_line = 50.0f;

	// 下一个分组的速度（一般情况下）
	private	float			next_speed = OniGroupControl.SPEED_MIN*5.0f;

	// 剩下的一般产生次数
	private int				normal_count = 5;

	// 剩下的时间产生次数
	private int				event_count = 1;

	// 正在产生的事件
	private GROUP_TYPE		event_type = GROUP_TYPE.NONE;
	
	// -------------------------------------------------------------------------------- //

	public const float	INTERVAL_MIN = 20.0f;			// 怪物出现间隔的最小值
	public const float	INTERVAL_MAX = 50.0f;			// 怪物出现间隔的最大值

	// -------------------------------------------------------------------------------- //

	public void	create()
	{
		// 为了在游戏开始后产生怪物，
		// 将生成位置初始化设置为处于玩家后方
		this.oni_generate_line = this.player.transform.position.x - 1.0f;

	}

	public void OnPlayerMissed()
	{
		// 重置怪物一次出现的数量
		this.oni_appear_num = 1;

		this.no_miss_count = 0;
	}

	public void	oniAppearControl()
	{
	#if false
		for(int i = 0;i < 4;i++) {

			if(Input.GetKeyDown((KeyCode)(KeyCode.Alpha1 + i))) {

				this.group_type_next = (GROUP_TYPE)i;

				this.is_random = false;
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha0)) {

			this.is_random = !this.is_random;
		}
	#endif

		// 玩家每前进一定距离后，生成怪物的分组

		if(this.can_dispatch) {

			// 已准备好生成下一个分组

		} else {

			// 未准备好生成下一个分组

			if(this.is_exclusive_group()) {

				// 特别模式情况下，等待怪物从画面中消失

				if(GameObject.FindGameObjectsWithTag("OniGroup").Length == 0) {

					this.can_dispatch = true;
				}

			} else {

				// 普通模式下，马上产生
				this.can_dispatch = true;
			}

			if(this.can_dispatch) {

				// 准备好出现后，通过玩家的当前位置计算应该出现的位置

				if(this.group_type_next == GROUP_TYPE.NORMAL) {

					this.oni_generate_line = this.player.transform.position.x + this.next_line;

				} else if(this.group_type_next == GROUP_TYPE.SLOW) {

					this.oni_generate_line = this.player.transform.position.x + 50.0f;

				} else {

					this.oni_generate_line = this.player.transform.position.x + 10.0f;
				}
			}
		}

		// 玩家前进了一定距离后，生成下一个分组

		do {

			if(this.scene_control.oni_group_num >= this.scene_control.oni_group_appear_max) {

				break;
			}
			if(!this.can_dispatch) {

				break;
			}
			if(this.player.transform.position.x <= this.oni_generate_line) {

				break;
			}

			//

			this.group_type = this.group_type_next;

			switch(this.group_type) {
	
				case GROUP_TYPE.SLOW:
				{
					this.dispatch_slow();
				}
				break;
	
				case GROUP_TYPE.DECELERATE:
				{
					this.dispatch_decelerate();
				}
				break;

				case GROUP_TYPE.PASSING:
				{
					this.dispatch_passing();
				}
				break;

				case GROUP_TYPE.RAPID:
				{
					this.dispatch_rapid();
				}
				break;

				case GROUP_TYPE.NORMAL:
				{
					this.dispatch_normal(this.next_speed);
				}
				break;
			}
	
			// 更新下次出现分组时的怪物数量
			// （逐渐增加）
			this.oni_appear_num++;
			this.oni_appear_num = Mathf.Min(this.oni_appear_num, SceneControl.ONI_APPEAR_NUM_MAX);

			this.can_dispatch = false;

			this.no_miss_count++;

			this.scene_control.oni_group_num++;
			
			if(this.is_random) {

				// 选择下次出现的分组
				this.select_next_group_type();
			}

		} while(false);
	}

	// 画面上只有一个分组？
	public bool	is_exclusive_group()
	{
		bool	ret;

		do {

			ret = true;

			if(this.group_type == GROUP_TYPE.PASSING || this.group_type_next == GROUP_TYPE.PASSING) {

				break;
			}
			if(this.group_type == GROUP_TYPE.DECELERATE || this.group_type_next == GROUP_TYPE.DECELERATE) {

				break;
			}
			if(this.group_type == GROUP_TYPE.SLOW || this.group_type_next == GROUP_TYPE.SLOW) {

				break;
			}

			ret = false;

		} while(false);

		return(ret);
	}

	public void select_next_group_type()
	{

		// 检测普通分组和事件分组的状态迁移

		if(this.event_type != GROUP_TYPE.NONE) {

			this.event_count--;

			if(this.event_count <= 0) {

				this.event_type = GROUP_TYPE.NONE;

				this.normal_count = Random.Range(3, 7);
			}

		} else {

			this.normal_count--;

			if(this.normal_count <= 0) {

				// 产生事件

				this.event_type = (GROUP_TYPE)Random.Range(0, 4);

				switch(this.event_type) {

					default:
					case GROUP_TYPE.DECELERATE:
					case GROUP_TYPE.PASSING:
					case GROUP_TYPE.SLOW:
					{
						this.event_count = 1;
					}
					break;

					case GROUP_TYPE.RAPID:
					{
						this.event_count = Random.Range(2, 4);
					}
					break;
				}
			}
		}

		// 生成普通分组和事件分组

		if(this.event_type == GROUP_TYPE.NONE) {

			// 普通类型的分组

			float		rate;
	
			rate = (float)this.no_miss_count/10.0f;
	
			rate = Mathf.Clamp01(rate);
	
			this.next_speed = Mathf.Lerp(OniGroupControl.SPEED_MAX, OniGroupControl.SPEED_MIN, rate);	

			this.next_line = Mathf.Lerp(LevelControl.INTERVAL_MAX, LevelControl.INTERVAL_MIN, rate);

			this.group_type_next = GROUP_TYPE.NORMAL;

		} else {

			// 事件类型的分组

			this.group_type_next = this.event_type;
		}

	}

	// 普通模式
	public void dispatch_normal(float speed)
	{
		Vector3	appear_position = this.player.transform.position;

		// 在玩家前方，稍微在画面外的位置生成
		appear_position.x += appear_margin;
		
		this.create_oni_group(appear_position, speed, OniGroupControl.TYPE.NORMAL);
	}

	// 缓慢型
	public void dispatch_slow()
	{
		Vector3	appear_position = this.player.transform.position;

		// 在玩家前方，稍微在画面外的位置生成
		appear_position.x += appear_margin;
		
		float		rate;

		rate = (float)this.no_miss_count/10.0f;

		rate = Mathf.Clamp01(rate);

		this.create_oni_group(appear_position, OniGroupControl.SPEED_MIN*5.0f, OniGroupControl.TYPE.NORMAL);
	}

	// 极短型
	public void dispatch_rapid()
	{
		Vector3	appear_position = this.player.transform.position;

		// 在玩家前方，稍微在画面外的位置生成
		appear_position.x += appear_margin;
		
		//this.create_oni_group(appear_position, OniGroupControl.SPEED_MIN, OniGroupControl.TYPE.NORMAL);
		this.create_oni_group(appear_position, this.next_speed, OniGroupControl.TYPE.NORMAL);
	}

	// 中途减速型
	public void dispatch_decelerate()
	{
		Vector3	appear_position = this.player.transform.position;

		// 在玩家前方，稍微在画面外的位置生成
		appear_position.x += appear_margin;
		
		this.create_oni_group(appear_position, 9.0f, OniGroupControl.TYPE.DECELERATE);
	}

	// 中途追赶超越型
	public void dispatch_passing()
	{
		float	speed_low  = 2.0f;
		float	speed_rate = 2.0f;
		float	speed_high = (speed_low - this.player.GetComponent<Rigidbody>().velocity.x)/speed_rate + this.player.GetComponent<Rigidbody>().velocity.x;

		// 更慢的怪物被超越时的位置（0.0玩家的位置～1.0 画面右端）
		float	passing_point = 0.5f;

		Vector3	appear_position = this.player.transform.position;

		// 为了让两个分组在途中重叠，调整生成的位置

		appear_position.x = this.player.transform.position.x + appear_margin;
		
		this.create_oni_group(appear_position, speed_high, OniGroupControl.TYPE.NORMAL);

		appear_position.x = this.player.transform.position.x + appear_margin*Mathf.Lerp(speed_rate, 1.0f, passing_point);
		
		this.create_oni_group(appear_position, speed_low, OniGroupControl.TYPE.NORMAL);
	}

	// -------------------------------------------------------------------------------- //

	// 生成怪物分组
	private void create_oni_group(Vector3 appear_position, float speed, OniGroupControl.TYPE type)
	{
		// -------------------------------------------------------- //
		// 生成分组整体的碰撞器（用于碰撞检测）

		Vector3	position = appear_position;

		// 生成 OniGroupPrefab 实例
		// 加上 "as GameObject" 表示将生成的对象转化为 GameObject 
		//
		GameObject 	go = GameObject.Instantiate(this.OniGroupPrefab) as GameObject;

		OniGroupControl new_group = go.GetComponent<OniGroupControl>();

		// 和地面接触的高度
		position.y = OniGroupControl.collision_size/2.0f;

		position.z = 0.0f;

		new_group.transform.position = position;

		new_group.scene_control  = this.scene_control;
		new_group.main_camera    = this.scene_control.main_camera;
		new_group.player         = this.player;
		new_group.run_speed      = speed;
		new_group.type           = type;

		// -------------------------------------------------------- //
		// 生成分组的怪物集合

		Vector3	base_position = position;

		int		oni_num = this.oni_appear_num;

		// 靠近碰撞盒的左端
		base_position.x -= (OniGroupControl.collision_size/2.0f - OniControl.collision_size/2.0f);

		// 和地面接触的高度
		base_position.y = OniControl.collision_size/2.0f;

		// 生成怪物
		new_group.CreateOnis(oni_num, base_position);

	}
}

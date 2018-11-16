using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	// -------------------------------------------------------------------------------- //
	// 预设

	public GameObject		OniGroupPrefab = null;
	public GameObject		OniPrefab = null;
	public GameObject		OniEmitterPrefab = null;
	public GameObject[]		OniYamaPrefab;

	// SE音效
	public AudioClip	GameStart = null;
	public AudioClip	EvalSound = null;			// 评价
	public AudioClip	ReturnSound = null;			// 返回

	// -------------------------------------------------------------------------------- //

	// 玩家
	public PlayerControl	player = null;

	// 得分
	public ScoreControl		score_control = null;
	
	// 摄像机
	public GameObject	main_camera = null;

	// 控制怪物的出现
	public LevelControl	level_control = null;
	
	// 控制得分计算
	public ResultControl result_control = null;

	// 用于在得分时从上方落下怪物的对象
	public OniEmitterControl	oni_emitter = null;

	// GUI（2D显示）的控制
	private GUIControl	gui_control = null;
	
	// 淡入淡出控制
	private FadeControl	fader = null;
	
	// -------------------------------------------------------------------------------- //

	// 游戏进行状态
	public enum STEP {

		NONE = -1,

		START,					// “开始”的文字出现
		GAME,					// 游戏进行中
		ONI_VANISH_WAIT,		// 游戏结束后，等待画面上所有的怪物消失
		LAST_RUN,				// 怪物不再出现后的状态
		PLAYER_STOP_WAIT,		// 等待玩家停止

		GOAL,					// 得分
		ONI_FALL_WAIT,			// 等待“怪物从上方落下”过程结束
		RESULT_DEFEAT,			// 显示斩杀数量的评价
		RESULT_EVALUATION,		// 显示击倒时机的评价
		RESULT_TOTAL,			// 综合评价

		GAME_OVER,				// 游戏结束
		GOTO_TITLE,				// 迁移到标题

		NUM,
	};

	public STEP	step      = STEP.NONE;			// 当前游戏的进行状态
	public STEP	next_step = STEP.NONE;			// 迁移状态
	public float	step_timer      = 0.0f;		// 状态迁移后经过的时间
	public float	step_timer_prev = 0.0f;

	// -------------------------------------------------------------------------------- //

	// 从点击按钮后到攻击命中经历的时间（用于成绩评价）
	public float		attack_time = 0.0f;


	// 评价
	// 斩杀怪物时靠得越近得分越高
	public enum EVALUATION {

		NONE = -1,

		OKAY = 0,		// 普通
		GOOD,			// 不错
		GREAT,			// 好极了

		MISS,			// 失败（发生了接触）

		NUM,
	};
	public static string[] evaluation_str = {

		"okay",
		"good",
		"great",
		"miss",
	};
	
	public EVALUATION	evaluation = EVALUATION.NONE;

	// -------------------------------------------------------------------------------- //

	// 游戏整体的结果
	public struct Result {

		public int		oni_defeat_num;			// 斩杀的怪物数量（总和）
		public int[]	eval_count;				// 各个评价的次数

		public int		rank;					// 游戏整体的结果
		
		public float	score;					// 当前得分
		public float	score_max;				// 游戏中的最高记录
		
	};

	public Result	result;

	// -------------------------------------------------------------------------------- //

	// 每次出现怪物的最大值
	// 如果不发生失误则一直增加，但不会超过该数值
	public const int	ONI_APPEAR_NUM_MAX = 10;

	// 游戏结束时怪物的分组数量
	public int				oni_group_appear_max = 50;
	
	// 失败时减少的怪物出现数量
	public static int		oni_group_penalty = 1;
	
	// 隐藏得分时的出现数量
	public static float		SCORE_HIDE_NUM = 40;
	
	// 出现小组的数量
	public int				oni_group_num = 0;

	// 击中 or 发生接触的怪物分组数量
	public int				oni_group_complite = 0;
	
	// 击中的怪物分组数量
	public int				oni_group_defeat_num = 0;
	
	// 发生接触的怪物分组数量
	public int				oni_group_miss_num = 0;
	
	// 游戏开始后“开始”文字出现的时间长度
	private const float	START_TIME = 2.0f;

	// 显示得分时，“怪物堆积的位置”和“玩家停下的位置”之间的距离
	private const float	GOAL_STOP_DISTANCE = 8.0f;

	// 对操作进行评价时，按钮按下后到攻击命中所经过的时间标准
	public const float	ATTACK_TIME_GREAT = 0.05f;
	public const float	ATTACK_TIME_GOOD  = 0.10f;

	// -------------------------------------------------------------------------------- //
	// 用于调试的各种标记位
	// 请尝试适当改变其中的值看看游戏发生了怎么样的变化

	// 如果设置为true ，倒下的怪物将按照摄像机的本地坐标系移动
	// 因为和摄像机一起发生连动，即使摄像机突然停下来，也不会有不自然的感觉
	//
	public const bool	IS_ONI_BLOWOUT_CAMERA_LOCAL = true;

	// 显示怪物分组的碰撞器（用于调试）
	// 由于很多只怪物在一起出现，按分组使用同一个碰撞器
	//
	// 这是为了
	//
	// ・便于调整玩家和怪物发生碰撞时的行为
	// ・增强怪物被击退时向四处飞开的效果
	//
	// などの理由です.
	//
	public const bool	IS_DRAW_ONI_GROUP_COLLISION = false;

	// 玩家攻击时，显示攻击判定
	public const bool	IS_DRAW_PLAYER_ATTACK_COLLISION = false;

	// 用于调试的全自动机能
	// 设置为true 将一直执行攻击判定
	//
	public const bool	IS_AUTO_ATTACK = false;

	// AUTO_ATTACK 时的评价
	public EVALUATION	evaluation_auto_attack = EVALUATION.GOOD;
	
	// 讨伐数消失瞬间的讨伐数
	private int         backup_oni_defeat_num = -1;
	
	// 显示用于调试的背景模型（红，蓝，绿）
	public const bool	IS_DRAW_DEBUG_FLOOR_MODEL = false;

	public	float		eval_rate_okay  = 1.0f;
	public	float		eval_rate_good  = 2.0f;
	public	float		eval_rate_great = 4.0f;
	public	int			eval_rate		= 1;
	
	// -------------------------------------------------------------------------------- //
	
	void	Start()
	{
		// 查找玩家的实例对象
		this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

		this.player.scene_control = this;

		// 查找得分的实例
		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.level_control = new LevelControl();
		this.level_control.scene_control = this;
		this.level_control.player = this.player;
		this.level_control.OniGroupPrefab = this.OniGroupPrefab;
		this.level_control.create();
		
		this.result_control = new ResultControl();

		// GUI 控制脚本（组件）
		this.gui_control = GUIControl.get();
		
		// 添加淡入淡出控制.
		this.score_control = this.gui_control.score_control;
		
		// 清空游戏的结果
		this.result.oni_defeat_num = 0;
		this.result.eval_count = new int[(int)EVALUATION.NUM];
		this.result.rank = 0;
		this.result.score = 0;
		this.result.score_max = 0;
		
		for(int i = 0;i < this.result.eval_count.Length;i++) {

			this.result.eval_count[i] = 0;
		}
		
		// 开始淡入
		this.fader = FadeControl.get();
		this.fader.fade( 3.0f, new Color( 0.0f, 0.0f, 0.0f, 1.0f ), new Color( 0.0f, 0.0f, 0.0f, 0.0f ) );
		
		this.next_step = STEP.START;
	}

	void	Update()
	{
		if(Input.GetKeyDown(KeyCode.P)) {

			Debug.Break();
		}

		// 管理游戏的当前状态
		this.step_timer_prev = this.step_timer;
		this.step_timer += Time.deltaTime;

		// 检测是否迁移到下一个状态
		switch(this.step) {
		
			case STEP.START:
			{
				if(this.step_timer > SceneControl.START_TIME) {

					GUIControl.get().setVisibleStart(false);
					this.next_step = STEP.GAME;
				}
			}
			break;

			case STEP.GAME:
			{
				// 超过允许出现的最大数量，停止生成怪物
				if(this.oni_group_complite >= this.oni_group_appear_max) {

					next_step = STEP.ONI_VANISH_WAIT;
				}
			
				if(this.oni_group_complite >= SCORE_HIDE_NUM && this.backup_oni_defeat_num == -1) {

					this.backup_oni_defeat_num = this.result.oni_defeat_num;
				}
			}
			break;

			case STEP.ONI_VANISH_WAIT:
			{
				do {

					// 等待怪物全部倒下
					if(GameObject.FindGameObjectsWithTag("OniGroup").Length > 0) {

						break;
					}

					// 等待玩家加速
					// 在画面外生成怪物小山，当最后的怪物倒下后使其移动一定距离
					if(this.player.GetSpeedRate() < 0.5f) {

						break;
					}

					//

					next_step = STEP.LAST_RUN;

				} while(false);
			}
			break;

			case STEP.LAST_RUN:
			{
				if(this.step_timer > 2.0f) {

					// 使玩家停下来
					next_step = STEP.PLAYER_STOP_WAIT;
				}
			}
			break;

			case STEP.PLAYER_STOP_WAIT:
			{
				// 玩家停下后，开始显示得分成绩
				if(this.player.IsStopped()) {
			
					this.gui_control.score_control.setNumForce(this.backup_oni_defeat_num);
					this.gui_control.score_control.setNum(this.result.oni_defeat_num);
					next_step = STEP.GOAL;
				}
			}
			break;

			case STEP.GOAL:
			{
				// 等待怪物全部出现在画面上
				if(this.oni_emitter.oni_num == 0) {

					this.next_step = STEP.ONI_FALL_WAIT;
				}
			}
			break;

			case STEP.ONI_FALL_WAIT:
			{
				if(!this.score_control.isActive() && this.step_timer > 1.5f) {
					this.next_step = STEP.RESULT_DEFEAT;
				}
			}
			break;

			case STEP.RESULT_DEFEAT:
			{
				if(this.step_timer >= 0.4f && this.step_timer_prev < 0.4f )
				{
					// SE（“咚咚”）
					this.GetComponent<AudioSource>().PlayOneShot(this.EvalSound);
				}
				// 等待评价显示结束
				//
				if(this.step_timer > 0.5f) {

					this.next_step = STEP.RESULT_EVALUATION;
				}
			}
			break;
			
			case STEP.RESULT_EVALUATION:
			{
				if(this.step_timer >= 0.4f && this.step_timer_prev < 0.4f )
				{
					// SE（“咚咚”）
					this.GetComponent<AudioSource>().PlayOneShot(this.EvalSound);
				}
				// 等待评价显示结束
				//
				if(this.step_timer > 2.0f) {

					this.next_step = STEP.RESULT_TOTAL;
				}
			}
			break;
			
			case STEP.RESULT_TOTAL:
			{
				if(this.step_timer >= 0.4f && this.step_timer_prev < 0.4f )
				{
					// SE（“咚咚”）
					this.GetComponent<AudioSource>().PlayOneShot(this.EvalSound);
				}
				// 等待评价显示结束
				//
				if(this.step_timer > 2.0f) {

					this.next_step = STEP.GAME_OVER;
				}
			}
			break;

			case STEP.GAME_OVER:
			{
				// 鼠标被按下后淡出回到标题画面
				//
				if(Input.GetMouseButtonDown(0)) {
				
					// 淡出
					this.fader.fade( 1.0f, new Color( 0.0f, 0.0f, 0.0f, 0.0f ), new Color( 0.0f, 0.0f, 0.0f, 1.0f ) );
					this.GetComponent<AudioSource>().PlayOneShot(this.ReturnSound);
					
					this.next_step = STEP.GOTO_TITLE;
				}
			}
			break;
			
			case STEP.GOTO_TITLE:
			{
				// 淡入淡出结束后返回标题画面
				//
				if(!this.fader.isActive()) { 
					UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
				}
			}
			break;
		}

		// 状态变化时的初始化处理

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
			
				case STEP.START:
				{
					// 显示“开始！”文字
					GUIControl.get().setVisibleStart(true);
				}
				break;

				case STEP.PLAYER_STOP_WAIT:
				{
					// 使玩家停下来
					this.player.StopRequest();

					// -------------------------------------------------------- //
					// 生成“怪物堆积而成的小山”
					
					if( this.result_control.getTotalRank() > 0 ) {
						GameObject	oni_yama = Instantiate(this.OniYamaPrefab[this.result_control.getTotalRank() - 1]) as GameObject;
				
						Vector3		oni_yama_position = this.player.transform.position;
				
						oni_yama_position.x += this.player.CalcDistanceToStop();
						oni_yama_position.x += SceneControl.GOAL_STOP_DISTANCE;
	
						oni_yama_position.y = 0.0f;
				
						oni_yama.transform.position = oni_yama_position;
					}
					else{
						
					}

					// -------------------------------------------------------- //
				}
				break;

				case STEP.GOAL:
				{
					// 开始显示得分成绩

					// 生成使“怪物从上方掉下来”的发射器

					GameObject	go = Instantiate(this.OniEmitterPrefab) as GameObject;
	
					this.oni_emitter = go.GetComponent<OniEmitterControl>();

					Vector3		emitter_position = oni_emitter.transform.position;

					// 放在怪物小山的上方

					emitter_position.x  = this.player.transform.position.x;
					emitter_position.x += this.player.CalcDistanceToStop();
					emitter_position.x += SceneControl.GOAL_STOP_DISTANCE;
	
					this.oni_emitter.transform.position = emitter_position;

					// 根据最终的评价改变掉落怪物的数量

					int		oni_num = 0;

					switch(this.result_control.getTotalRank()) {
						case 0:		oni_num = Mathf.Min( this.result.oni_defeat_num, 10 );	break;
						case 1:		oni_num = 6;	break;
						case 2:		oni_num = 10;	break;
						case 3:		oni_num = 20;	break;
					}
				
					this.oni_emitter.oni_num = oni_num;
					if( oni_num == 0 )
					{
						this.oni_emitter.is_enable_hit_sound = false;
					}
				}
				break;

				case STEP.RESULT_DEFEAT:
				{
					// 得分评价出现后，停止发出怪物落下的声音
					this.oni_emitter.is_enable_hit_sound = false;
					// 开始显示“击杀数量”的评价
					this.gui_control.startDispDefeatRank();
				}
				break;

				case STEP.RESULT_EVALUATION:
				{
					// 开始显示“击杀敏捷度”的评价
					this.gui_control.startDispEvaluationRank();
				}
				break;

				case STEP.RESULT_TOTAL:
				{
					// 隐藏“击杀数量”和“击杀敏捷度”的评价
					this.gui_control.hideDefeatRank();
					this.gui_control.hideEvaluationRank();

					// 开始显示总体评价
					this.gui_control.startDispTotalRank();
				}
				break;

				case STEP.GAME_OVER:
				{
					// 显示“返回！”文本
					this.gui_control.setVisibleReturn(true);
				}
				break;
			}

			this.step = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
			this.step_timer_prev = -1.0f;
		}

		// 各个状态的执行处理

		switch(this.step) {

			case STEP.GAME:
			{
				// 控制怪物的出现
				this.level_control.oniAppearControl();
			}
			break;
		}

	}

	// 玩家失败时的处理
	public void	OnPlayerMissed()
	{
		this.oni_group_miss_num++;
		this.oni_group_complite++;
		this.oni_group_appear_max -= oni_group_penalty;
		
		this.level_control.OnPlayerMissed();

		this.evaluation = EVALUATION.MISS;

		this.result.eval_count[(int)this.evaluation]++;

		// 让画面上的所有分组都退场

		GameObject[] oni_groups = GameObject.FindGameObjectsWithTag("OniGroup");

		foreach(var oni_group in oni_groups) {
			this.oni_group_num--;
			oni_group.GetComponent<OniGroupControl>().beginLeave();
		}
	}

	// 追加被击倒的怪物数量
	public void	AddDefeatNum(int num)
	{
		this.oni_group_defeat_num++;
		this.oni_group_complite++;
		this.result.oni_defeat_num += num;
		
		// 通过点击按钮后结果的时间来决定评价的好坏
		// （点击后到攻击命中的时间短＝在非常精确的时刻斩杀了怪物）

		this.attack_time = this.player.GetComponent<PlayerControl>().GetAttackTimer();

		if(this.evaluation == EVALUATION.MISS) {

			// 失败（慢吞吞地运行中）后，只会出现OKAY 
			this.evaluation = EVALUATION.OKAY;

		} else {

			if(this.attack_time < ATTACK_TIME_GREAT) {
	
				this.evaluation = EVALUATION.GREAT;
	
			} else if(this.attack_time < ATTACK_TIME_GOOD) {
	
				this.evaluation = EVALUATION.GOOD;
	
			} else {
	
				this.evaluation = EVALUATION.OKAY;
			}
		}

		if(SceneControl.IS_AUTO_ATTACK) {

			this.evaluation = this.evaluation_auto_attack;
		}

		this.result.eval_count[(int)this.evaluation] += num;
		
		// 计算得分
		float[] score_list = { this.eval_rate_okay, this.eval_rate_good, this.eval_rate_great, 0 };
		this.result.score_max += num * this.eval_rate_great;
		this.result.score += num * score_list[(int)this.evaluation];
		
		this.result_control.addOniDefeatScore(num);
		this.result_control.addEvaluationScore((int)this.evaluation);
	}
	
	//是否能显示得分
	public bool IsDrawScore()
	{
		if( this.step >= STEP.GOAL )
		{
			return true;
		}
		
		if(this.oni_group_complite >= SCORE_HIDE_NUM )
		{
			return false;
		}
		
		return true;
	}

	// ================================================================ //
	// 对象实例

	protected	static SceneControl	instance = null;

	public static SceneControl	get()
	{
		if(SceneControl.instance == null) {

			GameObject		go = GameObject.Find("SceneControl");

			if(go != null) {

				SceneControl.instance = go.GetComponent<SceneControl>();

			} else {

				Debug.LogError("Can't find game object \"SceneControl\".");
			}
		}

		return(SceneControl.instance);
	}

}

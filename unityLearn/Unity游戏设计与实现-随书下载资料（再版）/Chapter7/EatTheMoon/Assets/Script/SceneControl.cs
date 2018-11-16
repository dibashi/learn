using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneControl : MonoBehaviour {


	public GameObject	StackBlockPrefab = null;


	public PlayerControl	player_control = null;

	public StackBlockControl	stack_control = null;
	public BGControl			bg_control = null;
	public GoalSceneControl		goal_scene = null;
	public VanishEffectControl	vanish_fx_control = null;

	public float	slider_value = 0.5f;

	// 各种颜色的材质（Blockl.cs）
	//
	// ・只有一个实体
	// ・能通过Inspector 进行修改
	//
	// 根据这两个条件，只能生成一个实例，通过SceneControl 来管理
	//
	public Material[]	block_materials;


	// ---------------------------------------------------------------- //

	public int		height_level = 0;

	public static int	MAX_HEIGHT_LEVEL = 50;

	public int			player_stock;				// 玩家的残余数量

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		PLAY = 0,			// 游戏中
		GOAL_ACT,			// 得分动作
		MISS,				// 失败动作

		GAMEOVER,			// 游戏结束

		NUM,
	};

	public STEP			step;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;


	// ---------------------------------------------------------------- //

	public enum SE {

		NONE = -1,

		DROP = 0,			// 放下方块时
		DROP_CONNECT,		// 消除方块时（4个同色方块排列时）
		LANDING,			// 从上方落下的方块着陆时
		SWAP,				// 上下方块旋转替换时
		EATING,				// 吃蛋糕时
		JUMP,				// 从上方落下的方块着陆时
		COMBO,				// 发生连锁时

		CLEAR,				// 清空
		MISS,				// 失败

		NUM,
	};

	public AudioClip[]	audio_clips;

	public AudioSource[]	audios;

	// ---------------------------------------------------------------- //

	public void	playSe(SE se)
	{
		if(se == SE.SWAP) {

			this.audios[1].clip = this.audio_clips[(int)se];
			this.audios[1].Play();

		} else {

			this.audios[0].PlayOneShot(this.audio_clips[(int)se]);
		}
	}

	void	Awake()
	{
		this.player_stock = 3;
	}

	void	Start()
	{

		//

		Block.materials = this.block_materials;

		this.stack_control = new StackBlockControl();

		this.stack_control.StackBlockPrefab = this.StackBlockPrefab;
		this.stack_control.scene_control = this;
		this.stack_control.create();

		this.vanish_fx_control = GameObject.FindGameObjectWithTag("VanishEffectControl").GetComponent<VanishEffectControl>();

		//

		this.player_control = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		this.player_control.scene_control = this;

		this.bg_control = GameObject.FindGameObjectWithTag("BG").GetComponent<BGControl>();

		this.goal_scene = new GoalSceneControl();
		this.goal_scene.scene_control = this;
		this.goal_scene.create();

		//

		this.audios = this.GetComponents<AudioSource>();

		//

		this.slider_value = Mathf.InverseLerp(RotateAction.ROTATE_TIME_SWAP_MIN, RotateAction.ROTATE_TIME_SWAP_MAX, RotateAction.rotate_time_swap);

		this.height_level = 0;

		this.bg_control.setHeightRateDirect((float)this.height_level/(float)MAX_HEIGHT_LEVEL);

	}
	
	void	Update()
	{
		this.step_timer += Time.deltaTime;

	#if false
		if(Input.GetKeyDown(KeyCode.G)) {

			this.next_step = STEP.GOAL_ACT;
		}
		if(Input.GetKeyDown(KeyCode.W)) {

			this.height_level = MAX_HEIGHT_LEVEL - 1;
	
			this.bg_control.setHeightRateDirect((float)this.height_level/(float)MAX_HEIGHT_LEVEL);
		}
	#endif

		// -------------------------------------------------------- //
		// 检测是否迁移到下一状态

		switch(this.step) {

			case STEP.PLAY:
			{
				do {

					if(this.player_control.isHungry()) {

						this.player_stock--;
						this.player_stock = Mathf.Max(0, this.player_stock);

						GUIControl.get().setStockCount(this.player_stock);

						this.next_step = STEP.MISS;

						break;
					}
	
					if(this.height_level >= MAX_HEIGHT_LEVEL) {
	
						this.next_step = STEP.GOAL_ACT;
						break;
					}

				} while(false);
			}
			break;

			case STEP.MISS:
			{
				if(this.step_timer > 1.0f) {

					if(	this.player_stock == 0) {

						this.next_step = STEP.GAMEOVER;

					} else {

						this.player_control.revive();
						this.next_step = STEP.PLAY;
					}
				}
			}
			break;

			case STEP.GOAL_ACT:
			case STEP.GAMEOVER:
			{
				// 点击鼠标时
				//
				if(Input.GetMouseButtonDown(0)) {
		
					UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
				}
			}
			break;
		}

		// -------------------------------------------------------- //
		// 状态迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.MISS:
				{
					this.playSe(SE.MISS);
				}
				break;

				case STEP.GAMEOVER:
				{
					GUIControl.get().setDispGameOver(true);
				}
				break;

				case STEP.GOAL_ACT:
				{
					this.goal_scene.start();
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// -------------------------------------------------------- //
		// 各个状态的执行处理

		switch(this.step) {

			case STEP.GOAL_ACT:
			{
				this.goal_scene.execute();
			}
			break;
		}

		// ---------------------------------------------------------------- //
				
		this.stack_control.update();

		GUIControl.get().setStomachRate(this.player_control.getLifeRate());

	}

	public void		heightGain()
	{
		if(this.height_level < MAX_HEIGHT_LEVEL) {

			this.height_level++;
	
			this.bg_control.setHeightRate((float)this.height_level/(float)MAX_HEIGHT_LEVEL);
		}
	}

}

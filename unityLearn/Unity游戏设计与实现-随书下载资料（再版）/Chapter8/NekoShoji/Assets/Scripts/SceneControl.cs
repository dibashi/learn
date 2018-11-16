using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	public const int	LIFE_COUNT = 5;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		GAME = 0,			// 游戏中
		GAMEOVER,			// 游戏结束
		CLEAR,				// 游戏过关
		DONE,				// 返回入口

		NUM,
	};

	public Step<STEP>			step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	protected int	combo_count = 0;		//  连击数　连续撞破窗户纸的次数
		
	// Life Setting
	protected int	lifecnt = LIFE_COUNT;

	// ---------------------------------------------------------------- //

	protected float		disp_end_timer = 0.0f;

	public enum COMBO {

		FAILED = -1,
		NORMAL = 0,
		
		CHAIN01,
		CHAIN02,
	};

	public COMBO combo = COMBO.NORMAL;

	// ---------------------------------------------------------------- //
	// Audio
	public AudioClip COMBO_SOUND_01      = null;
	public AudioClip COMBO_SOUND_02      = null;
	public AudioClip COMBO_SOUND_03      = null;
	
	public AudioClip CLEAR_SOUND         = null;
	public AudioClip CLEAR_NEKO_SOUND    = null;
	public AudioClip CLEAR_LOOP_SOUND    = null;
	public AudioClip GAMEOVER_SOUND      = null;
	public AudioClip GAMEOVER_NEKO_SOUND = null;

	public NekoControl	neko_control = null;
	public RoomControl	room_control = null;

	// ================================================================ //
	// 继承于MonoBehaviour 

	void	Awake()
	{
		this.neko_control = GameObject.FindGameObjectWithTag("NekoPlayer").GetComponent<NekoControl>();

		this.room_control = RoomControl.get();
	}

	void	Start()
	{
		this.step.set_next(STEP.GAME);
	}
	
	void	Update()
	{

		float	delta_time = Time.deltaTime;

		AudioSource		audio = GetComponent<AudioSource>();

		// ---------------------------------------------------------------- //
		// 检测是否迁移到下一状态

		switch(this.step.do_transition()) {

			case STEP.NONE:
			{
				this.step.set_next(STEP.GAME);
			}
			break;

			case STEP.GAME:
			{
				do {

					int		shoji_num = this.getPaperNum();

					if(shoji_num == 0) {

						this.step.set_next(STEP.CLEAR);
						break;
					}

					if(this.lifecnt <= 0) {
	
						this.step.set_next(STEP.GAMEOVER);
						break;
					}

				} while(false);
			}
			break;

			case STEP.CLEAR:
			{
				do {

					if(this.disp_end_timer <= 1.0f) {
						break;
					}

					if(!Input.GetMouseButtonDown(0)) {
						break;
					}

					this.step.set_next(STEP.DONE);

				} while(false);
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 状态迁移时的初始化

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.GAME:
				{
					this.clearComboCount();
					lifecnt = LIFE_COUNT;
				}
				break;

				case STEP.CLEAR:
				{
					audio.clip = CLEAR_NEKO_SOUND;
					audio.Play();

					// 自動運転開始.
					this.neko_control.beginAutoDrive();
				}
				break;

				case STEP.GAMEOVER:
				{
					audio.clip = GAMEOVER_NEKO_SOUND;
					audio.Play();

					// 立刻显示“结束”
					Navi.get().dispEnd();
				}
				break;

				case STEP.DONE:
				{
					UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 各个状态的执行处理

		switch(this.step.do_execution(delta_time)) {

			case STEP.CLEAR:
			{
				if(this.step.is_acrossing_time(CLEAR_NEKO_SOUND.length)) {

					audio.clip = CLEAR_SOUND;
					audio.Play();
				}
				if(this.step.is_acrossing_time(CLEAR_NEKO_SOUND.length + CLEAR_SOUND.length)) {

					audio.clip = CLEAR_LOOP_SOUND;
					audio.loop = true;
					audio.Play();
				}

				if(Input.GetMouseButtonDown(0)) {

					if(!Navi.get().isDispEnd()) {

						Navi.get().dispEnd();
					}
				}

				// 经过一定时间后，自动显示“结束”
				if(this.step.is_acrossing_time(CLEAR_NEKO_SOUND.length + CLEAR_SOUND.length + CLEAR_LOOP_SOUND.length)) {

					if(!Navi.get().isDispEnd()) {

						Navi.get().dispEnd();
					}
				}

				if(Navi.get().isDispEnd()) {

					this.disp_end_timer += delta_time;
				}
			}
			break;
			
			case STEP.GAMEOVER:
			{
				if(this.step.is_acrossing_time(GAMEOVER_NEKO_SOUND.length)) {

					audio.clip = GAMEOVER_SOUND;
					audio.Play();
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	// 撞破窗户纸的瞬间
	public void 	onPlayerHitted()
	{
		this.combo = COMBO.FAILED;

		Navi.get().setCombo(this.combo);
	}

	// 错过机会后落地时
	public void 	onPlayerMissed()
	{
		this.clearComboCount();
		this.lifecnt -= 1;
    }
	
	public int getLifeCount()
	{
		return(this.lifecnt);
	}

	// 取得窗户纸的剩余张数
	public int getPaperNum()
	{
		return(this.room_control.getPaperNum());
	}

	// 增加连击数
	public void	addComboCount()
	{
		this.combo_count++;
		
		switch(this.combo_count) {
			
			case 0:
			{
				GetComponent<AudioSource>().clip = COMBO_SOUND_01;
			}
			break;

			case 1:
			{
				GetComponent<AudioSource>().clip = COMBO_SOUND_01;
			}
			break;

			case 2:
			{
				GetComponent<AudioSource>().clip = COMBO_SOUND_02;
				this.combo = COMBO.CHAIN01;

				Navi.get().setCombo(this.combo);
			}
			break;

			default:
			{
				GetComponent<AudioSource>().clip = COMBO_SOUND_03;
				this.combo = COMBO.CHAIN02;

				Navi.get().setCombo(this.combo);
			}
			break;

		}
		
		GetComponent<AudioSource>().Play();
	}

	// 将连击数设置为0
	public void	clearComboCount()
	{
		this.combo_count = 0;
		this.combo = COMBO.NORMAL;

		Navi.get().setCombo(this.combo);
	}
	
	// ================================================================ //
	//																	//
	// ================================================================ //

	protected static	SceneControl instance = null;

	public static SceneControl	get()
	{
		if(SceneControl.instance == null) {

			GameObject		go = GameObject.Find("GameSceneControl");

			if(go != null) {

				SceneControl.instance = go.GetComponent<SceneControl>();

			} else {

				Debug.LogError("Can't find game object \"SceneControl\".");
			}
		}

		return(SceneControl.instance);
	}
}

using UnityEngine;
using System.Collections;

public class TitleSceneControl : MonoBehaviour {

	// 进行状态
	public enum STEP {

		NONE = -1,

		TITLE = 0,				// 显示标题（等待按钮按下）
		WAIT_SE_END,			// 等待开始音效结束
		FADE_WAIT,				// 等待淡入淡出结束

		NUM,
	};

	private STEP	step = STEP.NONE;
	private STEP	next_step = STEP.NONE;
	private float	step_timer = 0.0f;

	private FadeControl	fader = null;					// 淡入淡出控制
	
	public UnityEngine.UI.Image		uiImageStart;		// “开始”的UI.Image
	
	// 按下开始按钮时播放动画的时间
	private const float	TITLE_ANIME_TIME = 0.1f;
	private const float	FADE_TIME = 1.0f;
	
	// -------------------------------------------------------------------------------- //

	void 	Start()
	{
		// 不允许玩家操作
		PlayerControl	player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		player.UnPlayable();
		
		// 添加淡入淡出控制
		this.fader = FadeControl.get();
		this.fader.fade( 1.0f, new Color( 0.0f, 0.0f, 0.0f, 1.0f ), new Color( 0.0f, 0.0f, 0.0f, 0.0f) );
		
		this.next_step = STEP.TITLE;
	}

	void 	Update()
	{
		this.step_timer += Time.deltaTime;

		// 检测是否迁移到下一个状态
		switch(this.step) {

			case STEP.TITLE:
			{
				// 鼠标被按下
				//
				if(Input.GetMouseButtonDown(0)) {

					this.next_step = STEP.WAIT_SE_END;
				}
			}
			break;

			case STEP.WAIT_SE_END:
			{
				// 播放SE 结束后淡出
			
				bool	to_finish = true;

				do {

					if(!this.GetComponent<AudioSource>().isPlaying) {

						break;
					}

					if(this.GetComponent<AudioSource>().time >= this.GetComponent<AudioSource>().clip.length) {

						break;
					}

					to_finish = false;

				} while(false);

				if(to_finish) {

					this.fader.fade( FADE_TIME, new Color( 0.0f, 0.0f, 0.0f, 0.0f ), new Color( 0.0f, 0.0f, 0.0f, 1.0f) );
				
					this.next_step = STEP.FADE_WAIT;
				}
			}
			break;
			
			case STEP.FADE_WAIT:
			{
				// 淡入淡出结束后，载入游戏场景后结束
				if(!this.fader.isActive()) 
				{
					UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
				}
			}
			
			break;
		}

		// 状态变化时的初始化处理

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.WAIT_SE_END:
				{
					// 播放开始的SE
					this.GetComponent<AudioSource>().Play();
				}
				break;
			}

			this.step = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// 各个状态的执行处理

		switch(this.step) {

			case STEP.WAIT_SE_END:
			{
				float	scale	= 1.0f;
				
				float	rate = this.step_timer/TITLE_ANIME_TIME;
					
				scale = Mathf.Lerp(2.0f, 1.0f, rate);

				this.uiImageStart.GetComponent<RectTransform>().localScale = Vector3.one*scale;
			}
			break;
		}

	}
}

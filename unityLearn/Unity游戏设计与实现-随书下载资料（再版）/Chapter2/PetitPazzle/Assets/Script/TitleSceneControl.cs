using UnityEngine;
using System.Collections;

public class TitleSceneControl : MonoBehaviour {

	enum STEP {

		NONE = -1,

		WAIT = 0,		// 等待清空
		PLAY_JINGLE,	// 播放开始音乐

		NUM,
	};

	private STEP	step      = STEP.WAIT;
	private STEP	next_step = STEP.NONE;

	private float		step_timer = 0.0f;

	// -------------------------------------------------------- //

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		// ---------------------------------------------------------------- //

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 检测状态迁移

		switch(this.step) {

			case STEP.WAIT:
			{
				if(Input.GetMouseButtonDown(0)) {

					this.next_step = STEP.PLAY_JINGLE;
				}
			}
			break;

			case STEP.PLAY_JINGLE:
			{
				// SE 播放结束后，载入游戏场景

				if(this.step_timer > this.GetComponent<AudioSource>().clip.length + 0.5f) {

					UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene0");
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.PLAY_JINGLE:
				{
					this.GetComponent<AudioSource>().Play();
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 执行处理

		switch(this.step) {

			case STEP.WAIT:
			{
			}
			break;
		}

	}

	void OnGUI()
	{
	}
}

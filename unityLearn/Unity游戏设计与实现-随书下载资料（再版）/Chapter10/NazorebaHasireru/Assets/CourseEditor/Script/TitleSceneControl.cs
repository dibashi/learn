using UnityEngine;
using System.Collections;

public class TitleSceneControl : MonoBehaviour {

	// 進行状態.
	public enum STEP {

		NONE = -1,

		TITLE = 0,				// 显示标题（等待按键）
		WAIT_SE_END,			// 等待开始音效结束

		NUM,
	};

	private STEP	step = STEP.NONE;
	private STEP	next_step = STEP.NONE;
	private float	step_timer = 0.0f;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Start()
	{
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
				// SE 播放结束后，结束场景载入

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

		/*switch(this.step) {

			case STEP.TITLE:
			{
			}
			break;
		}*/

	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelControl : MonoBehaviour {

	public enum LEVEL {

		NONE = -1,

		EASY = 0,
		NORMAL = 1,
		HARD = 2,

		NUM,
	};

	public LEVEL	level = LEVEL.EASY;

	public SceneControl	scene_control = null;

	private bool	random_bool_prev;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
		this.scene_control = SceneControl.get();

		this.random_bool_prev = Random.Range(0, 2) == 0 ? true : false;
	}

	void	Start()
	{
		switch(GlobalParam.GetInstance().difficulty()) {

			case 0:
			{	
				this.level = LEVEL.EASY;
			}
			break;
		
			case 1:
			{	
				this.level = LEVEL.NORMAL;
			}
			break;
			
			case 2:
			{	
				this.level = LEVEL.HARD;
			}
			break;

			default:
			{	
				this.level = LEVEL.EASY;
			}
			break;
		}
	}
	
	void 	Update()
	{
	
	}

	// ================================================================ //

	// 窗户开始关闭时的距离
	public float getCloseDistance()
	{
		// 如果值设为很小的话窗户开始关闭的时机变得很晚，会比较难
		
		float	close_distance = 14.0f;
		
		int		paper_num = this.scene_control.getPaperNum();

		switch(this.level) {
		
			case LEVEL.EASY:
			{
				close_distance = 14.0f;
			}
			break;
			
			case LEVEL.NORMAL:
			{
				close_distance = 14.0f;
			}
			break;
			
			case LEVEL.HARD:
			{
				if(paper_num >= 8) {

					close_distance = 12.0f;

				} else if(paper_num >= 5) {

					close_distance = 12.0f;

				} else {

					close_distance = FloorControl.SHUTTER_POSITION_Z;
				}
			}
			break;

			default:
			{
				close_distance = 14.0f;
			}
			break;
		}


		return(close_distance);
	}
	
	// 获得关闭窗户的模式
	public void	getClosingPattern(out FloorControl.CLOSING_PATTERN_TYPE out_type, out bool out_is_flip, out FloorControl.ClosingPatternParam out_param)
	{
		int		paper_num   = this.scene_control.getPaperNum();
		bool	random_bool = Random.Range(0, 2) == 0 ? true : false;

		out_param.as_float = 0.0f;
		out_param.as_bool  = false;

		switch(this.level) {

			case LEVEL.EASY:
			{
				// easy 只有 normal

				out_type = FloorControl.CLOSING_PATTERN_TYPE.NORMAL;

				out_is_flip = false;
			}
			break;

			case LEVEL.NORMAL:
			{
				if(9 >= paper_num && paper_num >= 8) {

					// 第1，2张为 NORMAL.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.NORMAL;

					out_is_flip = false;

				} else if(paper_num == 7) {

					// 还剩下7张时为 OVERSHOOT.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.OVERSHOOT;

					out_is_flip = false;

				} else if(paper_num == 6) {

					// 还剩下6张时为 SECONDTIME.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.SECONDTIME;

					out_is_flip = false;

				} else if(paper_num == 5) {

					// 还剩下5张时为 ARCODION.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.ARCODION;

					out_is_flip = false;

				} else if(4 >= paper_num && paper_num >= 3) {

					// 还剩下4～3张时为 DELAY（is_flip 是随机）

					out_type = FloorControl.CLOSING_PATTERN_TYPE.DELAY;

					out_is_flip = random_bool;

					if(paper_num == 4) {

						// 还剩4张时从右开始

						out_param.as_bool = false;

					} else {

						// 还剩3张时从右开始（拉门的里边开始）
						out_param.as_bool = true;
					}

				} else if(paper_num == 2) {

					// 还剩2张时一定是 FALLDOWN.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.FALLDOWN;

					out_is_flip = false;

				} else {

					// 还剩1张时一定是 FLIP（is_flip 是随机）

					out_type = FloorControl.CLOSING_PATTERN_TYPE.FLIP;

					out_is_flip = random_bool;
				}
			}
			break;

			case LEVEL.HARD:
			{
				if(paper_num >= 8) {

					// 还剩9～8张时为 NORMAL.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.NORMAL;

					if(paper_num == 9) {

						out_is_flip = random_bool;

					} else {

						out_is_flip = !this.random_bool_prev;
					}

				} else if(paper_num >= 5) {

					// 还剩7～5张时为 SLOW.
					// 关闭后渐渐变慢

					out_type = FloorControl.CLOSING_PATTERN_TYPE.SLOW;

					if(paper_num == 7) {

						out_is_flip        = random_bool;
						out_param.as_float = 1.5f;

					} else if(paper_num == 6) {

						out_is_flip        = !this.random_bool_prev;
						out_param.as_float = 1.7f;

						// 为下一次做准备
						// （使7，6，5 一定会有交互）
						random_bool = !this.random_bool_prev;

					} else {

						out_is_flip        = !this.random_bool_prev;
						out_param.as_float = 2.0f;
					}

				} else {

					// 还剩4～1张时 SUPER_DELAY.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.SUPER_DELAY;

					out_is_flip = random_bool;

					if(paper_num >= 4) {

						out_param.as_float = 0.6f;

					} else if(paper_num >= 3) {

						out_param.as_float = 0.7f;

					} else {

						out_param.as_float = 0.9f;
					}
				}
			}
			break;

			default:
			{
				out_type = FloorControl.CLOSING_PATTERN_TYPE.NORMAL;

				out_is_flip = false;
			}
			break;
		}

		this.random_bool_prev = random_bool;
	}

	// 获取“几次穿空（从撞破后的洞中穿过）后会变成铁板？”的值
	public int	getChangeSteelCount()
	{
		// 值为-1 时不会变成铁板
		int	count = -1;

		int	paper_num = this.scene_control.getPaperNum();

		switch(this.level) {
		
			case LEVEL.EASY:
			{
				// easy 不存在铁板的情况
				count = -1;
			}
			break;

			case LEVEL.NORMAL:
			{
				// hardは鉄板化なし.
				count = -1;
			}
			break;

			case LEVEL.HARD:
			{
				// 设置使得剩下的张数越少铁板就越容易出现

				if(paper_num >= 8) {
				
					count = -1;
				
				} else if(paper_num >= 6) {

					count = 5;

				} else if(paper_num >= 3) {

					count = 2;

				} else { 

					count = 1;
				}

			}
			break;

			default:
			{
				count = -1;
			}
			break;
		}

		return(count);
	}

	// ================================================================ //
	//																	//
	// ================================================================ //

	protected static	LevelControl instance = null;

	public static LevelControl	get()
	{
		if(LevelControl.instance == null) {

			GameObject		go = GameObject.Find("GameSceneControl");

			if(go != null) {

				LevelControl.instance = go.GetComponent<LevelControl>();

			} else {

				Debug.LogError("Can't find game object \"LevelControl\".");
			}
		}

		return(LevelControl.instance);
	}
}

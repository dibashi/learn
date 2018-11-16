using UnityEngine;
using System.Collections;

public class GUIControl : MonoBehaviour {

	public SceneControl		scene_control = null;
	public ScoreControl		score_control = null;
	
	public GameObject	uiImageStart;		// “开始”
	public GameObject	uiImageReturn;		// “返回”

	public RankDisp		rankSmallDefeat;				// 击杀怪物数量的评价
	public RankDisp		rankSmallEval;					// 敏捷评价
	public RankDisp		rankTotal;						// 总体评价

	public UnityEngine.Sprite[]	uiSprite_GradeSmall;	// 用于显示怪物击杀数量评价的小字号文字（优/良/可/不可）
	public UnityEngine.Sprite[]	uiSprite_Grade;			// 用于显示总体评价的文字（优/良/可/不可）

	// ================================================================ //

	void	Awake()
	{
		this.scene_control = SceneControl.get();
		this.score_control = GetComponent<ScoreControl>();
		
		this.score_control.setNumForce( this.scene_control.result.oni_defeat_num );

		this.rankSmallDefeat.uiSpriteRank = this.uiSprite_GradeSmall;
		this.rankSmallEval.uiSpriteRank   = this.uiSprite_GradeSmall;
		this.rankTotal.uiSpriteRank       = this.uiSprite_Grade;
	}

	void	Start()
	{
	}
	
	void	Update()
	{
		// 设置显示“击杀怪物的数量”得分
		this.score_control.setNum(this.scene_control.result.oni_defeat_num);

		// ---------------------------------------------------------------- //
		// 调试用
	#if false
		SceneControl	scene = this.scene_control;

		dbPrint.setLocate(10, 5);
		dbPrint.print(scene.attack_time);
		dbPrint.print(scene.evaluation);
		if(this.scene_control.level_control.is_random) {

			dbPrint.print("RANDOM(" + scene.level_control.group_type_next + ")");

		} else {

			dbPrint.print(scene.level_control.group_type_next);
		}

		dbPrint.print(scene.result.oni_defeat_num);

		// 统计击杀的评价（是否近距离斩杀？）
		for(int i = 0;i < (int)SceneControl.EVALUATION.NUM;i++) {

			dbPrint.print(((SceneControl.EVALUATION)i).ToString() + " " + scene.result.eval_count[i].ToString());
		}
	#endif
	}

	// 显示/隐藏“开始！”
	public void	setVisibleStart(bool is_visible)
	{
		this.uiImageStart.SetActive(is_visible);
	}

	// 显示/隐藏“返回！”
	public void	setVisibleReturn(bool is_visible)
	{
		this.uiImageReturn.SetActive(is_visible);
	}

	// 开始显示“击杀怪物数量”的评价
	public void	startDispDefeatRank()
	{
		int		rank  = this.scene_control.result_control.getDefeatRank();

		this.rankSmallDefeat.startDisp(rank);
	}

	// 隐藏“击杀怪物数量”
	public void	hideDefeatRank()
	{
		this.rankSmallDefeat.hide();
	}

	// 开始显示“击杀敏捷度”的评价
	public void	startDispEvaluationRank()
	{
		int		rank  = this.scene_control.result_control.getEvaluationRank();

		this.rankSmallEval.startDisp(rank);
	}

	// 隐藏“击杀敏捷度”的评价
	public void	hideEvaluationRank()
	{
		this.rankSmallEval.hide();
	}

	// 开始显示总体评价
	public void	startDispTotalRank()
	{
		int		rank  = this.scene_control.result_control.getTotalRank();

		this.rankTotal.startDisp(rank);
	}

	void	OnGUI()
	{			
	}

	// ================================================================ //
	// 对象实例

	protected	static GUIControl	instance = null;

	public static GUIControl	get()
	{
		if(GUIControl.instance == null) {

			GameObject		go = GameObject.Find("GameCanvas");

			if(go != null) {

				GUIControl.instance = go.GetComponent<GUIControl>();

			} else {

				Debug.LogError("Can't find game object \"GUIControl\".");
			}
		}

		return(GUIControl.instance);
	}
}

using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 调试工具
// ----------------------------------------------------------------------------
public class DebugTools : MonoBehaviour {
	
	private StageController stageController;
	
	void Start () {
		
		// 获取关卡控制器实例
		stageController =
			GameObject.Find ("StageController").GetComponent<StageController>();
	}
	
	void OnGUI () {
		
		// 选择关卡
		GUILayout.BeginArea(new Rect(0, 0, 50, 500));
		GUILayout.Label("STAGE: " + stageController.GetStage() );
		if ( GUILayout.Button("STAR") ) stageController.SetStage("START");
		if ( GUILayout.Button("PAT1") ) stageController.SetStage("PATROL1");
		if ( GUILayout.Button("ATT1") ) stageController.SetStage("ATTACK1");
		if ( GUILayout.Button("PAT2") ) stageController.SetStage("PATROL2");
		if ( GUILayout.Button("ATT2") ) stageController.SetStage("ATTACK2");
		if ( GUILayout.Button("PAT3") ) stageController.SetStage("PATROL3");
		if ( GUILayout.Button("ATT3") ) stageController.SetStage("ATTACK3");
		if ( GUILayout.Button("SILE") ) stageController.SetStage("SILENCE");
		if ( GUILayout.Button("BOSS") ) stageController.SetStage("BOSS");
		if ( GUILayout.Button("OVER") ) stageController.SetStage("GAMECLEAR");
		GUILayout.EndArea();
		
		// 选择游戏关卡
		GUILayout.BeginArea(new Rect(55, 0, 105, 500));
		GUILayout.Label("LEVEL:"+stageController.GetLevelText());
		stageController.SetLevel(
			(int)GUILayout.HorizontalSlider (stageController.GetLevel(), 0, 3));
		GUILayout.EndArea();

	}
}

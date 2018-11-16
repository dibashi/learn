using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//取得原始的输入信息，决定玩家的行为等
public class InputManager : MonoBehaviour {

	void Awake(){
		Application.targetFrameRate = 60;
	}

	// Use this for initialization
	void Start () {
		m_musicManager=GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_playerAction=GameObject.Find("PlayerAvator").GetComponent<PlayerAction>();
		m_scoringManager=GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
	}

	// Update is called once per frame
	void Update () {
		//在Input的Update中记录节拍数
		//MusicManager的Update中节拍数的记录和输入与节拍数
		//最大只相差一帧
		//演奏中点击画面时玩家的动作
		if( Input.GetMouseButtonDown(0) && m_musicManager.IsPlaying() ){
			PlayerActionEnum actionType;
			if (m_scoringManager.temper < ScoringManager.temperThreshold){
				actionType=PlayerActionEnum.HeadBanging;
			}
			else{
				actionType
					=m_musicManager.currentSongInfo.onBeatActionSequence[
						m_scoringManager.GetNearestPlayerActionInfoIndex()
					].playerActionType;
			}
			m_playerAction.DoAction(actionType);
		}
	}

	//private variables
	private MusicManager	m_musicManager;
	private PlayerAction	m_playerAction;
	private ScoringManager	m_scoringManager;
}

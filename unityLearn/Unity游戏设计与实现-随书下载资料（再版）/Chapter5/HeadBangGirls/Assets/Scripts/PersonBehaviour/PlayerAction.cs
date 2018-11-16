using UnityEngine;
using System.Collections;
//表示玩家动作种类的枚举
public enum PlayerActionEnum{
	None,
	HeadBanging,
	Jump
};
//玩家的动作
public class PlayerAction : MonoBehaviour {
	public AudioClip headBangingSoundClip_GOOD;
	public AudioClip headBangingSoundClip_BAD;
	//玩家当前的动作
	public PlayerActionEnum currentPlayerAction{
		get{ return m_currentPlayerAction; }
	}
	//玩家的最后一个动作
	public OnBeatActionInfo lastActionInfo{
		get{ return m_lastActionInfo; }
	}
	// Use this for initialization
	void Start () {
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
	}
	
	// Update is called once per frame
	void Update () {
		m_currentPlayerAction = m_newPlayerAction;
		m_newPlayerAction = PlayerActionEnum.None;
	}
	public void DoAction(PlayerActionEnum actionType){
		m_newPlayerAction = actionType;

		OnBeatActionInfo actionInfo = new OnBeatActionInfo();
		actionInfo.triggerBeatTiming = m_musicManager.beatCountFromStart;
		actionInfo.playerActionType = m_newPlayerAction;
		m_lastActionInfo = actionInfo;

		if(actionType == PlayerActionEnum. HeadBanging){
			gameObject.GetComponent<SimpleSpriteAnimation>().BeginAnimation(2, 1, false);
		}
		else if (actionType == PlayerActionEnum.Jump)
		{	
			gameObject.GetComponent<SimpleActionMotor>().Jump();
			gameObject.GetComponent<SimpleSpriteAnimation>().BeginAnimation(1, 1, false);
		}
	}
	//执行对应于输入的动作
	//Private variables
	MusicManager m_musicManager;
	OnBeatActionInfo m_lastActionInfo=new OnBeatActionInfo();
	PlayerActionEnum m_currentPlayerAction;
	PlayerActionEnum m_newPlayerAction;
}

using UnityEngine;
using System.Collections;

// 游戏规则说明画面的功能类.
public class InstructionGUI : MonoBehaviour {

	public SimpleSpriteAnimation sampleBandMemberAniamtion;
	public SimpleSpriteAnimation playerAvatorAnimation;

	// Use this for initialization
	void 	Start()
	{
	}
	
	// Update is called once per frame
	void 	Update()
	{
		//每隔一定时间让角色播放动画
		animationCounter+=Time.deltaTime;
		if( animationCounter > 1.0f){
			sampleBandMemberAniamtion.BeginAnimation(1,1,false);
			playerAvatorAnimation.BeginAnimation(2,1,false);
			animationCounter=0;
		}

		//通过点击进入下一个
		if( Input.GetMouseButton(0) ){
			GameObject.Find("PhaseManager").GetComponent<PhaseManager>().SetPhase("Play");
		}
	}

	float animationCounter=0;
	
}

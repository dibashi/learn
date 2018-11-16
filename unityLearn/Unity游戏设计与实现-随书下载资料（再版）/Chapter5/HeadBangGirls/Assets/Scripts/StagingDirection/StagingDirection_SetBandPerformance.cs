using UnityEngine;
using System.Collections;

public class StagingDirection_SetBandMemberAction : StagingDirection {
	public string m_memberName="";
	public string m_actionName="";
	public StagingDirection_SetBandMemberAction(){}
	public override void OnBegin(){
		switch( m_actionName ){
		case "jump":
			GameObject.Find(m_memberName).GetComponent<BandMember>().Jump();
			break;
		case "actionA":
			GameObject.Find(m_memberName).GetComponent<SimpleSpriteAnimation>().BeginAnimation(1,1);
			break;
		case "actionB":
			GameObject.Find(m_memberName).GetComponent<SimpleSpriteAnimation>().BeginAnimation(4,4);
			break;
		}
	}
	public override StagingDirectionEnum GetEnum()
	{
		return StagingDirectionEnum.SetBandMemberAction;
	}
	public override void ReadCustomParameterFromString(string[] parameters){
		m_memberName = parameters[3];
		m_actionName = parameters[2];
	}
};
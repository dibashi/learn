using UnityEngine;
using System.Collections;


public class StagingDirection_SetBandMemberDefaultAnimation : StagingDirection
{
	public string m_memberName = "";
	public int m_animationFromIndex = 0;
	public int m_animationToIndex = 0;
	public StagingDirection_SetBandMemberDefaultAnimation() { }
	public override void OnBegin()
	{
		GameObject.Find(m_memberName).GetComponent<SimpleSpriteAnimation>().SetDefaultAnimation(m_animationFromIndex, m_animationToIndex);
	}
	public override StagingDirectionEnum GetEnum()
	{
		return StagingDirectionEnum.SetBandMemberDefaultAnimation;
	}
	public override void ReadCustomParameterFromString(string[] parameters)
	{
		m_memberName = parameters[4];
		m_animationFromIndex = int.Parse(parameters[2]);
		m_animationToIndex = int.Parse(parameters[3]);
	}
};
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
public enum StagingDirectionEnum{
	None,
	FireBlast,
	LightFlash,
	LightShuffle,
	LightFade,
	SetBandMemberAction,
	SetBandMemberDefaultAnimation,
	Applause
};
public abstract class StagingDirection : MusicalElement{
	public StagingDirection(){}
	public virtual void OnBegin(){}
	public virtual void OnEnd(){}
	public virtual void Update(){}
	public virtual bool IsFinished(){return true;}
	public virtual StagingDirectionEnum GetEnum(){return StagingDirectionEnum.None;}
};

public class StagingDirectionFactory{
	public static StagingDirection CreateStagingDirectionFromEnum(StagingDirectionEnum stagingDirectionEnum){
		if( stagingDirectionEnum == StagingDirectionEnum.FireBlast ){
			return new StagingDirection_FireBlast(0,1);
		}
		else if( stagingDirectionEnum == StagingDirectionEnum.LightFlash ){
			return new StagingDirection_LightFlash(0);
		}
		else if( stagingDirectionEnum == StagingDirectionEnum.LightShuffle ){
			return new StagingDirection_LightShuffle(0,1);
		}
		else if( stagingDirectionEnum == StagingDirectionEnum.LightFade ){
			return new StagingDirection_LightFade(0,1.0f);
		}
		else if( stagingDirectionEnum == StagingDirectionEnum.SetBandMemberAction ){
			return new StagingDirection_SetBandMemberAction();
		}
		else if (stagingDirectionEnum == StagingDirectionEnum.SetBandMemberDefaultAnimation)
		{
			return new StagingDirection_SetBandMemberDefaultAnimation();
		}
		else if (stagingDirectionEnum == StagingDirectionEnum.Applause)
		{
			return new StagingDirection_Applause();
		}
		return null;
	}
}
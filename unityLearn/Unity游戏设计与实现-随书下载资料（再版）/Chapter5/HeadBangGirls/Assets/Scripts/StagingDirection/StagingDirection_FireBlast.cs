using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
public class StagingDirection_FireBlast : StagingDirection {
	int m_fireBlasterID=0;
	int m_fireBlasterEmmitionCount=10;
	public StagingDirection_FireBlast(){}
	public StagingDirection_FireBlast( int fireBlasterID, int fireBlasterEmmitionCount ){
		m_fireBlasterID = fireBlasterID;
		m_fireBlasterEmmitionCount = fireBlasterEmmitionCount;
	}
	public override void OnBegin(){
		GameObject.Find(
			"Fire" + m_fireBlasterID.ToString()).GetComponent<ParticleSystem>().Emit(m_fireBlasterEmmitionCount
		);
		GameObject.Find("Fire" + m_fireBlasterID.ToString()).GetComponent<AudioSource>().Play();
	}
	public override void ReadCustomParameterFromString(string[] parameters){
		m_fireBlasterID = int.Parse(parameters[2]);
		m_fireBlasterEmmitionCount = int.Parse(parameters[3]);
	}
	public override bool IsFinished(){
		return true;
	}
	public override StagingDirectionEnum GetEnum(){
		return StagingDirectionEnum.FireBlast;
    }
};
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
[System.Serializable]
public class StagingDirection_LightFlash : StagingDirection {
	Light m_light=null;
	int m_lightID=0;
	float m_lightIntensityAtBeginning;
	public StagingDirection_LightFlash( int lightID ){
		m_lightID=lightID;
	}
	public override void OnBegin(){
		m_light = GameObject.Find("Light" + m_lightID.ToString()).GetComponent<Light>();
		m_lightIntensityAtBeginning = m_light.intensity;
		m_light.intensity *= 3.0f;
	}
	public override void OnEnd(){
		m_light.intensity = m_lightIntensityAtBeginning;
	}
	public override void Update(){
		m_light.intensity *= 0.80f;
	}
	public override bool IsFinished(){
		return Mathf.Abs(m_light.intensity-m_lightIntensityAtBeginning) < 0.3;
	}
	public override StagingDirectionEnum GetEnum(){
		return StagingDirectionEnum.LightFlash;
	}
	public override void ReadCustomParameterFromString(string[] parameters){
		m_lightID = int.Parse(parameters[2]);
	}
};
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

[System.Serializable]
public class StagingDirection_LightFade : StagingDirection {
	Light m_light=null;
	int m_lightID=0;
	float m_intensityFadeTo = 1;
	public StagingDirection_LightFade( int lightID, float intensityFadeTo ){
		m_lightID=lightID;
		m_intensityFadeTo = intensityFadeTo;
	}
	public override void OnBegin(){
		m_light = GameObject.Find("Light" + m_lightID.ToString()).GetComponent<Light>();
	}
	public override void OnEnd(){
		m_light.intensity = m_intensityFadeTo;
	}
	public override void Update(){
		m_light.intensity += ( m_intensityFadeTo - m_light.intensity )*0.4f;
	}
	public override bool IsFinished(){
		return Mathf.Abs(m_intensityFadeTo-m_light.intensity) < 0.1;
	}
	public override StagingDirectionEnum GetEnum(){
		return StagingDirectionEnum.LightFade;
	}
	public override void ReadCustomParameterFromString(string[] parameters){
		m_lightID = int.Parse(parameters[2]);
		m_intensityFadeTo = float.Parse(parameters[3]);
	}
};
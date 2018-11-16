using UnityEngine;
using System.Collections;

[System.Serializable]
public class StagingDirection_LightShuffle : StagingDirection {
	Light[] m_light={null,null};
	int[] m_lightID={0,0};
	float m_shuffleSpeed = 5.0f;
	Vector3[] m_lightPosition={new Vector3(),new Vector3()};
	Quaternion[] m_lightRotation={new Quaternion(),new Quaternion()};
	public StagingDirection_LightShuffle( int lightIDOne, int lightIDAnother ){
		m_lightID[0]=lightIDOne;
		m_lightID[1]=lightIDAnother;
	}
	public override void OnBegin(){
		m_light[0]=GameObject.Find("Light" + m_lightID[0].ToString()).GetComponent<Light>();
		m_light[1]=GameObject.Find("Light" + m_lightID[1].ToString()).GetComponent<Light>();
		m_lightPosition[0]=m_light[0].transform.position;
		m_lightPosition[1]=m_light[1].transform.position;
		m_lightRotation[0]=m_light[0].transform.rotation;
		m_lightRotation[1]=m_light[1].transform.rotation;
	}
	public override void OnEnd(){
		m_light[0].transform.position=m_lightPosition[1];
		m_light[0].transform.rotation=m_lightRotation[1];
		m_light[1].transform.position=m_lightPosition[0];
		m_light[1].transform.rotation=m_lightRotation[0];
	}
	public override void Update(){
		m_light[0].transform.position=
			Vector3.MoveTowards( m_light[0].transform.position, m_lightPosition[1], m_shuffleSpeed );
		m_light[1].transform.position=
			Vector3.MoveTowards( m_light[1].transform.position, m_lightPosition[0], m_shuffleSpeed );
	}
	public override bool IsFinished(){
		return Mathf.Abs(m_light[0].transform.position.x-m_lightPosition[1].x) < 0.5;
	}
	public override StagingDirectionEnum GetEnum(){
		return StagingDirectionEnum.LightShuffle;
	}
	public override void ReadCustomParameterFromString(string[] parameters){
		m_lightID[0] = int.Parse(parameters[2]);
		m_lightID[1] = int.Parse(parameters[3]);
		m_shuffleSpeed = float.Parse(parameters[4]);
	}
};
using UnityEngine;
using System.Collections;
//舞台上的演出执行所必备的动作
public class SimpleActionMotor : MonoBehaviour {
	public bool isWaveBegin = false;
	public float wavePhaseOffset = 0;
	public float jumpInitialVelocity = 1.0f;
	public bool isJustJump{
		get { return m_isJustJump; }
	}
	public void Jump()
	{	m_isJumpTriggered=true;
		velocity = new Vector3(0, jumpInitialVelocity, 0);
	}
	// Use this for initialization
	void Start()
	{
		basePosition = transform.position;
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
	}

	// Update is called once per frame
	void Update()
	{
		m_isJustJump=false;
		if( m_isJumpTriggered ){
			m_isJustJump=true;
			m_isJumpTriggered=false;
		}
		positionOffset += velocity;
		if (positionOffset.y < 0) velocity.y = 0;
		else velocity.y -= gravity;
		if (isWaveBegin){
			if (m_musicManager.IsPlaying()){
				basePosition
					= new Vector3(
						basePosition.x
						, basePosition.y + Mathf.Sin((m_musicManager.beatCountFromStart + wavePhaseOffset) * Mathf.PI) * 0.03f
						, basePosition.z);
			}
		}
		transform.position = basePosition + positionOffset;
	}
	Vector3 basePosition = new Vector3();
	Vector3 velocity = new Vector3();
	Vector3 positionOffset = new Vector3();
	float gravity = 0.2f;
	bool m_isJumpTriggered=false;
	bool m_isJustJump=false;
	MusicManager m_musicManager;
}

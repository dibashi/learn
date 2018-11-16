using UnityEngine;
using System.Collections;

public class TreasureGenerator : MonoBehaviour {
	private const int GENERATE_TIMES = 2;
	public float[] m_generateTime = new float[GENERATE_TIMES];
	private bool[] m_generated = new bool[GENERATE_TIMES];
	private GameObject[] m_treasureInstances = new GameObject[GENERATE_TIMES];
	public float m_timer;
	
	public GameObject m_treasure;
	
	// Use this for initialization
	void Start () {
		m_timer = 0;
		for (int i = 0; i < GENERATE_TIMES; i++)
			m_generated[i] = false;		
	}
	
	// Update is called once per frame
	void Update () {
		for (int cnt = 0; cnt < GENERATE_TIMES; cnt++) {
			if (!m_generated[cnt] && m_generateTime[cnt] <= m_timer) {
				m_treasureInstances[cnt] = Instantiate(m_treasure,transform.position,transform.rotation) as GameObject;
				m_treasureInstances[cnt].transform.parent = transform;
				m_generated[cnt] = true;
			}
		}
		m_timer += Time.deltaTime;
	}
/*	
	public void OnStageStart()
	{
		m_timer = 0;
		for (int i = 0; i < GENERATE_TIMES; i++) {
			m_generated[i] = false;
			if (m_treasureInstances[i] != null) {
				Destroy(m_treasureInstances[i]);
				m_treasureInstances[i] = null;
			}
		}
		
	}
	*/
	
	public void OnRestart()
	{
		for (int i = 0; i < GENERATE_TIMES; i++) {
			if (m_treasureInstances[i] != null) {
				Destroy(m_treasureInstances[i]);
				m_treasureInstances[i] = null;
			}
		}
		m_timer -= 5.0f;
	}
		
}

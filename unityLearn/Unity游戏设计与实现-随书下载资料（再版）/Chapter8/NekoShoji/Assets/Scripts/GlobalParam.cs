using UnityEngine;
using System.Collections;

public class GlobalParam : MonoBehaviour {
	
	private int		m_difficultyMode = 0;
	private static	GlobalParam m_instance = null;

	public static GlobalParam GetInstance()
	{
		if (m_instance == null) {
			GameObject g = new GameObject("GlobalParam");
			m_instance = g.AddComponent<GlobalParam>();
			DontDestroyOnLoad(g);
		}
		return m_instance;
	}
	
	public int difficulty()
	{
		return m_difficultyMode;
	}
	
	public void SetMode(int difficulty)
	{
		m_difficultyMode = difficulty;
	}
}

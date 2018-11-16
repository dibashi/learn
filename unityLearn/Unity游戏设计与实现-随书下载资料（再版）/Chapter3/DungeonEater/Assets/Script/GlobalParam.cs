using UnityEngine;
using System.Collections;

public class GlobalParam : MonoBehaviour {
	private bool m_advertiseMode = false;
	private static GlobalParam m_instance = null;
	public static GlobalParam GetInstance()
	{
		if (m_instance == null) {
			GameObject g = new GameObject("GlobalParam");
			m_instance = g.AddComponent<GlobalParam>();
			DontDestroyOnLoad(g);
		}
		return m_instance;
	}
	
	public bool IsAdvertiseMode()
	{
		return m_advertiseMode;
	}
	
	public void SetMode(bool advertizeMode)
	{
		m_advertiseMode = advertizeMode;
	}
}

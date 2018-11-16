#if false
using UnityEngine;
using System.Collections;

// 全局（跨场景）使用的参数
public class GlobalParam : MonoBehaviour {
	
	// ================================================================ //

	public void		initialize()
	{
	}

	// ================================================================ //

	private static	GlobalParam instance = null;

	public static GlobalParam	get()
	{
		if(instance == null) {

			GameObject	go = new GameObject("GlobalParam");

			instance = go.AddComponent<GlobalParam>();

			instance.initialize();

			DontDestroyOnLoad(go);
		}

		return(instance);
	}

}
#endif
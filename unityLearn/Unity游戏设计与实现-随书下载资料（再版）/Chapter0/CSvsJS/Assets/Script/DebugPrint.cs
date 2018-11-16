using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugPrint : MonoBehaviour {

	private static DebugPrint	instance = null;

	private List<string>	texts;

	// ---------------------------------------------------------------- //

	public static DebugPrint	getInstance()
	{
		if(DebugPrint.instance == null) {

			GameObject	go = new GameObject("DebugPrint");

			DebugPrint.instance = go.AddComponent<DebugPrint>();
			DebugPrint.instance.create();

			DontDestroyOnLoad(go);
		}

		return(DebugPrint.instance);
	}

	public static void	print(string text)
	{
		DebugPrint	dp = DebugPrint.getInstance();

		dp.texts.Add(text);
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnGUI()
	{
		int		x = 100;
		int		y = 100;

		foreach(var text in this.texts.ToArray()) {

			GUI.Label(new Rect(x, y, 100, 20), text);

			y += 20;
		}

		// 绘制结束后，清空缓冲区
		if(UnityEngine.Event.current.type == UnityEngine.EventType.Repaint) {

			this.texts.Clear();
		}
	}

	public void	create()
	{
		this.texts = new List<string>();
	}

}

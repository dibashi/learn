using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugPrint : MonoBehaviour {

	private static DebugPrint	instance = null;

	public struct TextItem {

		public int		x, y;
		public string	text;
		public float	lifetime;
	};

	private List<TextItem>	items;
	private int				locate_x, locate_y;

	private static int		CHARA_W = 20;
	private static int		CHARA_H = 20;


	// ------------------------------------------------------------------------ //

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

	// 显示文本
	public static void	print(string text, float lifetime = 0.0f)
	{
		DebugPrint	dp = DebugPrint.getInstance();

		dp.add_text(text, lifetime);
	}

	// 设置显示的位置
	public static void	setLocate(int x, int y)
	{
		DebugPrint	dp = DebugPrint.getInstance();

		dp.set_locate(x, y);
	}

	// ------------------------------------------------------------------------ //

	void Start ()
	{
		this.clear();
	}
	
	void Update ()
	{

	}

	void OnGUI()
	{
		// 显示驻留在缓冲区中的文本

		int		x, y;

		foreach(var item in this.items) {

			x = item.x*DebugPrint.CHARA_W;
			y = item.y*DebugPrint.CHARA_H;

			GUI.Label(new Rect(x, y, item.text.Length*DebugPrint.CHARA_W, DebugPrint.CHARA_H), item.text);

			y += DebugPrint.CHARA_H;
		}

		// 清空缓冲区

		if(UnityEngine.Event.current.type == UnityEngine.EventType.Repaint) {

			this.clear();
		}
	}

	public void	create()
	{
		this.items = new List<TextItem>();
	}

	// 清空缓冲区
	private void	clear()
	{
		this.locate_x = 0;
		this.locate_y = 0;
		//this.items.Clear();

		for(int i = 0;i < this.items.Count;i++) {

			TextItem	item = this.items[i];

			if(item.lifetime >= 0.0) {

				item.lifetime -= Time.deltaTime;
	
				this.items[i] = item;
	
				if(item.lifetime <= 0.0f) {
	
					this.items.Remove(this.items[i]);
				}
			}
		}
	}

	// 设置显示位置
	private void	set_locate(int x, int y)
	{
		this.locate_x = x;
		this.locate_y = y;
	}

	// 添加文本
	private void	add_text(string text, float lifetime = 0.0f)
	{
		TextItem	item;

		item.x        = this.locate_x;
		item.y        = this.locate_y;
		item.text     = text;
		item.lifetime = lifetime;

		this.items.Add(item);

		this.locate_y++;
	}
}

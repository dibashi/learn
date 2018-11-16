using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

//配合音乐进行舞台演出和得分评估等处理的抽象类
public abstract class MusicalElement {

	//可以开始处理的时机
	public float triggerBeatTiming = 0;

	//参数值的字符串数组（用于读入CSV等）
	public virtual void ReadCustomParameterFromString(string[] parameters) {}

	//指定triggerBeatTiming的原点后生成克隆
	public virtual MusicalElement GetClone()
	{
		MusicalElement clone = this.MemberwiseClone() as MusicalElement;
		return clone;
	}

	public System.Xml.Schema.XmlSchema GetSchema() { return null; }
};

// 存储音乐的片段信息（旋律，停顿等）
public class SequenceRegion : MusicalElement {

	public float	totalBeatCount;
	public string	name;
	public float	repeatPosition;
};

//玩家要和着音乐进行的动作的信息
public class OnBeatActionInfo : MusicalElement {

	public PlayerActionEnum		playerActionType;	// 动作的种类
	public int					line_number;		// 原始文本中的行号

	public string	GetCustomParameterAsString_CSV()
	{
		return "SingleShot," + triggerBeatTiming.ToString() + "," + playerActionType.ToString();
	}

}

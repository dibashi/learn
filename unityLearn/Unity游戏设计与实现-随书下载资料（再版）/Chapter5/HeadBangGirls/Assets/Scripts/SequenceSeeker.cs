using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

//快速查找序列，获取最近元素的索引值
public class SequenceSeeker<ElementType> where ElementType: MusicalElement
{
	//重置查找序列
	public void SetSequence( List<ElementType> sequence )
	{
		m_sequence = sequence;
		m_nextIndex=0;
		m_currentBeatCount=0;
		m_isJustPassElement=false;
	}
	//表示最近的元素的索引编号
	public int nextIndex
	{
			get{return m_nextIndex;}
	}
	//通过要素的触发器位置时为true
	public bool isJustPassElement
	{
			get{return m_isJustPassElement;}
	}

	//每帧处理
	public void ProceedTime(float deltaBeatCount)
	{
		// 累计数量
		m_currentBeatCount += deltaBeatCount;
		// 清除用于表示“到达查找位置”瞬间的标记
		m_isJustPassElement = false;

		// 获取当前时刻后面紧跟的标记的索引值
		int		index = find_next_element(m_nextIndex);

		//判断“紧跟的下一个标记”是否和“定位到的位置”相同？
		if(index!=m_nextIndex){

			// 更新查找的位置
			m_nextIndex = index;

			// 设置表示“到达查找位置”瞬间的标记
			m_isJustPassElement=true;
		}
	}

	//查找
	public void Seek(float beatCount)
	{
		m_currentBeatCount = beatCount;

		m_nextIndex = find_next_element(0);
	}

	// 探测m_currentBeatCount 后存在的元素
	//
	private int	find_next_element(int start_index)
	{
		// 初始化用于表示“已通过最后标记的时刻”的值
		int ret = m_sequence.Count;

		for (int i = start_index;i < m_sequence.Count; i++) {

			// 存在位于m_currentBeatCount 之后的标记 ＝ 查找到了
			if(m_sequence[i].triggerBeatTiming > m_currentBeatCount) {
				ret = i;
				break;
			}
		}

		return(ret);
	}

//private variables
	private int		m_nextIndex         = 0;		//查找位置（＝从现在时刻来看，下一个元素的索引）
	private float	m_currentBeatCount  = 0;		//现在时刻
	private bool	m_isJustPassElement = false;	//只有到达了查找位置的那一帧才会为true

	private List<ElementType> m_sequence;			//查找的序列数据
}


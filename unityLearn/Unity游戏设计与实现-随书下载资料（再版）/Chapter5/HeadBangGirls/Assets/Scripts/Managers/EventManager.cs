using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//舞台呈现等事件管理
public class EventManager : MonoBehaviour {

	// Use this for initialization
	void	Start()
	{
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
	}
	public void		BeginEventSequence()
	{
		m_seekUnit.SetSequence(m_musicManager.currentSongInfo.stagingDirectionSequence);
	}
	public void		Seek(float beatCount)
	{
		m_seekUnit.Seek( beatCount );
		m_previousIndex = m_seekUnit.nextIndex;

		//定位时清空当前的列表
		for(LinkedListNode<StagingDirection> it = m_activeEvents.First; it != null; it = it.Next) {

			it.Value.OnEnd();
			m_activeEvents.Remove(it);
		}
	}
	void Update () {

		SongInfo	song = m_musicManager.currentSongInfo;

		// 检测到新的事件
		if( m_musicManager.IsPlaying() ) {

			// 保存旧的定位位置
			m_previousIndex = m_seekUnit.nextIndex;

			m_seekUnit.ProceedTime(m_musicManager.beatCount - m_musicManager.previousBeatCount);

			// 开始执行“上次的定位位置”和“更新后的定位位置”之间存在的事件
			for(int i = m_previousIndex;i < m_seekUnit.nextIndex;i++) {

				// 拷贝事件数据
				StagingDirection clone = song.stagingDirectionSequence[i].GetClone() as StagingDirection;

				clone.OnBegin();

				// 添加到“执行中的事件列表”
				m_activeEvents.AddLast(clone);
			}
		}

		// 执行“执行中的事件”
		for ( LinkedListNode<StagingDirection> it = m_activeEvents.First; it != null; it = it.Next) {

			StagingDirection	activeEvent = it.Value;

			activeEvent.Update();

			// 执行已经完成？
			if(activeEvent.IsFinished()) {

				activeEvent.OnEnd();

				// 从“执行中事件列表”中删除
				m_activeEvents.Remove(it);
			}
		}
	}

	//private variables

	MusicManager m_musicManager;

	// 定位单元
	SequenceSeeker<StagingDirection>	m_seekUnit = new SequenceSeeker<StagingDirection>();

	// 执行中的事件
	LinkedList<StagingDirection>		m_activeEvents = new LinkedList<StagingDirection>();

	int		m_previousIndex = 0;			// 上次定位的位置
}



using UnityEngine;


/// <summary>该类用于从标题画面开始启动游戏</summary>
public class TitleControl : MonoBehaviour
{
	//==============================================================================================
	// 内部数据类型

	/// <summary>迁移状态</summary>
	private enum STEP
	{
		NONE = -1,
		SELECT = 0,   // 选择中
		PLAY_JINGLE,  // 主题音乐播放中
		START_GAME,   // 开始游戏
		NUM
	}

	/// <summary>游戏章节</summary>
	private enum CHAPTER
	{
		NONE = -1,
		PROLOGUE = 0,
		C1,
		C2,
		C3_0,
		C3_1,
		C4,
		C5,
		EPILOGUE,
		NUM
	}


	//==============================================================================================
	// MonoBehaviour 相关的成员变量／方法

	/// <summary>主题画面的纹理</summary>
	public Texture2D m_titleTexture = null;

	/// <summary>主题曲音频片段</summary>
	public AudioClip m_startSound = null;

	/// <summary>启动方法</summary>
	private void Start()
	{
		m_chapterNames = new string[ ( int ) CHAPTER.NUM ];

		m_chapterNames[ ( int ) CHAPTER.PROLOGUE ] = "开场";
		m_chapterNames[ ( int ) CHAPTER.C1 ]       = "第一章";
		m_chapterNames[ ( int ) CHAPTER.C2 ]       = "第二章";
		m_chapterNames[ ( int ) CHAPTER.C3_0 ]     = "第三章　上";
		m_chapterNames[ ( int ) CHAPTER.C3_1 ]     = "第三章　下";
		m_chapterNames[ ( int ) CHAPTER.C4 ]       = "第四章";
		m_chapterNames[ ( int ) CHAPTER.C5 ]       = "第五章";
		m_chapterNames[ ( int ) CHAPTER.EPILOGUE ] = "尾声";

		//

		m_textManager = TextManager.get();

		m_textManager.showTitle();
#if UNITY_EDITOR
		m_textManager.createButtons(m_chapterNames, Color.black, new Color(1.0f, 1.0f, 1.0f, 0.5f));
#endif
	}

	/// <summary>每帧更新方法</summary>
	private void Update()
	{
		// 检测step内的迁移
		if ( m_nextStep == STEP.NONE )
		{
			switch ( m_step )
			{
			case STEP.NONE:
				m_nextStep = STEP.SELECT;
				break;

			case STEP.SELECT:
#if UNITY_EDITOR
				do {

					if(m_textManager.selected_button == "") {
						break;
					}

					int		selected_index = System.Array.IndexOf(m_chapterNames, m_textManager.selected_button);

					if(selected_index < 0) {
						break;
					}

					m_selectedChapter = selected_index;
					m_nextStep        = STEP.PLAY_JINGLE;

				} while(false);
#else
				if ( Input.GetMouseButtonDown( 0 ) )
				{
					m_nextStep = STEP.PLAY_JINGLE;
				}
#endif //!UNITY_EDITOR
				break;

			case STEP.PLAY_JINGLE:
				if ( !GetComponent<AudioSource>().isPlaying )
				{
					m_nextStep = STEP.START_GAME;
				}
				break;
			}
		}

		// 状态迁移时的初始化
		while ( m_nextStep != STEP.NONE )
		{
			m_step = m_nextStep;
			m_nextStep = STEP.NONE;

			switch ( m_step )
			{
			case STEP.PLAY_JINGLE:
				// 播放主题曲
				GetComponent<AudioSource>().clip = m_startSound;
				GetComponent<AudioSource>().Play();
				break;

			case STEP.START_GAME:
#if !UNITY_EDITOR
				// 从开场开始
				GlobalParam.getInstance().setStartScriptFiles("c00_main", "c00_sub");
#else
				// 依据选择读取不同文件
				switch ( m_selectedChapter )
				{
				case ( int ) CHAPTER.PROLOGUE:
					GlobalParam.getInstance().setStartScriptFiles("c00_main", "c00_sub");
					break;

				case ( int ) CHAPTER.C1:
					GlobalParam.getInstance().setStartScriptFiles("c01_main", "c01_sub");
					break;

				case ( int ) CHAPTER.C2:
					GlobalParam.getInstance().setStartScriptFiles("c02_main", "c02_sub");
					break;

				case ( int ) CHAPTER.C3_0:
					GlobalParam.getInstance().setStartScriptFiles("c03_0_main", "c03_0_sub");
					break;

				case ( int ) CHAPTER.C3_1:
					GlobalParam.getInstance().setStartScriptFiles("c03_1_main", "c03_1_sub");
					break;

				case ( int ) CHAPTER.C4:
					GlobalParam.getInstance().setStartScriptFiles("c04_main", "c04_sub");
					break;

				case ( int ) CHAPTER.C5:
					GlobalParam.getInstance().setStartScriptFiles("c05_main", "c05_sub");
					break;

				case ( int ) CHAPTER.EPILOGUE:
					GlobalParam.getInstance().setStartScriptFiles("c90_main", "c90_sub");
					break;
				}
#endif //!UNITY_EDITOR

				// 加载游戏场景
				UnityEngine.SceneManagement.SceneManager.LoadScene( "GameScene" );

				break;
			}
		}
	}

	//==============================================================================================
	// 非公开成员变量

	/// <summary>现在的状态</summary>
	private STEP m_step = STEP.NONE;

	/// <summary>接下来要迁移的状态</summary>
	private STEP m_nextStep = STEP.NONE;

	/// <summary>各章名称</summary>
	private string[] m_chapterNames;

	private TextManager	m_textManager;

#if UNITY_EDITOR
	/// <summary>通过调试模式选择的章节
	private int m_selectedChapter = 0;
#endif //UNITY_EDITOR
}

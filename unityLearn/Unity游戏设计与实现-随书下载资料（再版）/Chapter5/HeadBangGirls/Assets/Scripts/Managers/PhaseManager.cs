using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

//用于控制游戏的状态迁移的管理类
public class PhaseManager : MonoBehaviour {

	public string currentPhase{
		get{ return m_currentPhase; }
	}
	public GameObject[] guiList;

	public StartupMenuGUI		startupMenuGUI;
	public InstructionGUI		instructionGUI;
	public OnPlayGUI			onPlayGUI;
	public ShowResultGUI		showResultGUI;
	public DevelopmentModeGUI	developmentGUI;

	private MusicManager	m_musicManager;
	private ScoringManager	m_scoringManager;
	private string			m_currentPhase = "Startup";

	// ================================================================ //

	void	Start()
	{
		m_musicManager   = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager = GameObject.Find("ScoringManager").GetComponent<ScoringManager>();

		SetPhase("Startup");
	}
	
	void	Update()
	{

		switch (currentPhase) {

			case "Play":
			{
				if( m_musicManager.IsFinished() ){
					SetPhase("GameOver");
				}
				/*if(Input.GetKeyDown(KeyCode.E)) {
					SetPhase("GameOver");
				}*/
			}
			break;
		}
	}

	public void SetPhase(string nextPhase)
	{
		switch(nextPhase){

			// 开始菜单
			case "Startup":
			{
				DeactiveateAllGUI();
				this.startupMenuGUI.gameObject.SetActive(true);
			}
			break;

			// 说明
			case "OnBeginInstruction":
			{
				DeactiveateAllGUI();
				this.instructionGUI.gameObject.SetActive(true);
				this.onPlayGUI.gameObject.SetActive(true);
			}
			break;

			//主体游戏
			case "Play":
			{
				DeactiveateAllGUI();
				this.onPlayGUI.gameObject.SetActive(true);

				//从csv读取音乐数据
				TextReader textReader
					= new StringReader(
						System.Text.Encoding.UTF8.GetString((Resources.Load("SongInfo/songInfoCSV") as TextAsset).bytes )
					);
				SongInfo songInfo = new SongInfo();
				SongInfoLoader loader=new SongInfoLoader();
				loader.songInfo=songInfo;
				loader.ReadCSV(textReader);
				m_musicManager.currentSongInfo = songInfo;
	
				foreach (GameObject audience in GameObject.FindGameObjectsWithTag("Audience"))
				{
					audience.GetComponent<SimpleActionMotor>().isWaveBegin = true;
				}
				//事件（舞台演出等）开始
				GameObject.Find("EventManager").GetComponent<EventManager>().BeginEventSequence();
				//开始评价得分
				m_scoringManager.BeginScoringSequence();

				//开始绘制旋律的时间序列
				this.onPlayGUI.BeginVisualization();
				this.onPlayGUI.isDevelopmentMode = false;
				//开始演奏
				m_musicManager.PlayMusicFromStart();
			}
			break;

			case "DevelopmentMode":
			{
				DeactiveateAllGUI();
				this.developmentGUI.gameObject.SetActive(true);
				this.onPlayGUI.gameObject.SetActive(true);

				//从csv读取音乐数据
				TextReader textReader
					= new StringReader(
						System.Text.Encoding.UTF8.GetString((Resources.Load("SongInfo/songInfoCSV") as TextAsset).bytes )
					);
				SongInfo songInfo = new SongInfo();
				SongInfoLoader loader=new SongInfoLoader();
				loader.songInfo=songInfo;
				loader.ReadCSV(textReader);
				m_musicManager.currentSongInfo = songInfo;
	
				foreach (GameObject audience in GameObject.FindGameObjectsWithTag("Audience"))
				{
					audience.GetComponent<SimpleActionMotor>().isWaveBegin = true;
				}
				//事件（舞台演出等）开始
				GameObject.Find("EventManager").GetComponent<EventManager>().BeginEventSequence();
				//开始评价得分
				m_scoringManager.BeginScoringSequence();

				//开始绘制旋律时序图
				this.onPlayGUI.BeginVisualization();
				this.onPlayGUI.isDevelopmentMode = true;

				//开始绘制develop模式下的专用GUI时序图
				GameObject.Find("DevelopmentModeGUI").GetComponent<DevelopmentModeGUI>().BeginVisualization();
				//开始演奏
				m_musicManager.PlayMusicFromStart();
			}
			break;

			case "GameOver":
			{
				DeactiveateAllGUI();
				this.showResultGUI.gameObject.SetActive(true);

				//显示保存得分的信息
				//Debug.Log( m_scoringManager.scoreRate );
				//Debug.Log(ScoringManager.failureScoreRate);

				ShowResultGUI.RESULT	result = ShowResultGUI.RESULT.GOOD;

				if(m_scoringManager.scoreRate <= ScoringManager.failureScoreRate) {

					result = ShowResultGUI.RESULT.BAD;
					GameObject.Find("Vocalist").GetComponent<BandMember>().BadFeedback();
					
				} else if(m_scoringManager.scoreRate >= ScoringManager.excellentScoreRate) {

					result = ShowResultGUI.RESULT.EXCELLENT;
					GameObject.Find("Vocalist").GetComponent<BandMember>().GoodFeedback();
					GameObject.Find("AudienceVoice").GetComponent<AudioSource>().Play();

				} else {

					result = ShowResultGUI.RESULT.GOOD;
					GameObject.Find("Vocalist").GetComponent<BandMember>().GoodFeedback();
				}

				this.showResultGUI.BeginVisualization(result);
			}
			break;

			case "Restart":
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
			}
			break;

			default:
			{
				Debug.LogError("unknown phase: " + nextPhase);
			}
			break;

		} // end of switch

		m_currentPhase = nextPhase;
	}

	private void DeactiveateAllGUI()
	{
		this.startupMenuGUI.gameObject.SetActive(false);
		this.instructionGUI.gameObject.SetActive(false);
		this.onPlayGUI.gameObject.SetActive(false);
		this.showResultGUI.gameObject.SetActive(false);
		this.developmentGUI.gameObject.SetActive(false);
	}
}

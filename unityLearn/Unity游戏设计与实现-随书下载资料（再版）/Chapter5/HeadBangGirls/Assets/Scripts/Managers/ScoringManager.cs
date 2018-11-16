using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//根据玩家的动作进行得分加减的管理
public class ScoringManager : MonoBehaviour {

	public static float		timingErrorToleranceGood      = 0.22f;		// 延迟程度小于该值则 Good.
	public static float		timingErrorTorelanceExcellent = 0.12f;		// 延迟程度小于该值则 Excellent.
	public static float		missScore = -1.0f;
	public static float		goodScore = 2.0f;
	public static float		excellentScore     = 4.0f;
	public static float		failureScoreRate   = 0.3f;		// 中途判定点判定为“失败”的得分率（得分／理论上的最高得分）
	public static float		excellentScoreRate = 0.85f;		// 中途判定点判定为“优秀”的得分率（得分／理论上的最高得分）
	public static float		missHeatupRate  = -0.08f;
	public static float		goodHeatupRate  = 0.01f;
	public static float		bestHeatupRate  = 0.02f;
	public static float		temperThreshold = 0.5f;			//决定演出是否发生变化的阀值.
	public bool outScoringLog = true;

	//现在的合计得分
	public float	score
	{
		get{ return m_score; }
	}
	private float	m_score;

	//涨势的数值化 0.0 - 1.0
	public float	temper
	{
		get { return m_temper; }
		set { m_temper = Mathf.Clamp(value, 0, 1); }
	}
	float m_temper = 0;

	//当前帧的得分变动合计值
	public float scoreJustAdded
	{
		get{ return m_additionalScore; }
	}

	//现在的得分率（得分／理论上的最高得分）
	public float scoreRate
	{
		get { return m_scoreRate; }
	}
	private float m_scoreRate = 0;

	//开始评估得分
	public void BeginScoringSequence()
	{
		m_scoringUnitSeeker.SetSequence(m_musicManager.currentSongInfo.onBeatActionSequence);
	}
	// Use this for initialization
    void Start()
    {
		m_musicManager  = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_playerAction  = GameObject.Find("PlayerAvator").GetComponent<PlayerAction>();
		m_bandMembers   = GameObject.FindGameObjectsWithTag("BandMember");
		m_audiences     = GameObject.FindGameObjectsWithTag("Audience");
		m_noteParticles = GameObject.FindGameObjectsWithTag("NoteParticle");
		m_phaseManager  = GameObject.Find("PhaseManager").GetComponent<PhaseManager>();

		//由于GUI对象存在执行Inactive的可能性，因此不能直接用Find来访问
		m_onPlayGUI     = m_phaseManager.guiList[1].GetComponent<OnPlayGUI>();
#if UNITY_EDITOR 
        m_logWriter = new StreamWriter("Assets/PlayLog/scoringLog.csv");
#endif
    }

	public void Seek(float beatCount)
	{
		m_scoringUnitSeeker.Seek(beatCount);
		m_previousHitIndex = -1;
	}

	// 确认最近的ActionInfo的索引值
	public int	GetNearestPlayerActionInfoIndex()
	{

		SongInfo	song = m_musicManager.currentSongInfo;
		int 		nearestIndex = 0;

		if(m_scoringUnitSeeker.nextIndex == 0) {

			// 定位位置如果在头部，不存在上一个标记所以不比较
			nearestIndex = 0;

		} else if(m_scoringUnitSeeker.nextIndex >= song.onBeatActionSequence.Count) {

			// 如果定位位置比数组的尺寸还大（超过了最后标记的时刻）

			nearestIndex = song.onBeatActionSequence.Count - 1;

		} else {

			// 比较前后时间点的偏差

			OnBeatActionInfo	crnt_action = song.onBeatActionSequence[m_scoringUnitSeeker.nextIndex];			// 定位位置
			OnBeatActionInfo	prev_action = song.onBeatActionSequence[m_scoringUnitSeeker.nextIndex - 1];		// 上一个定位位置

			float				act_timing = m_playerAction.lastActionInfo.triggerBeatTiming;

			if(crnt_action.triggerBeatTiming - act_timing < act_timing - prev_action.triggerBeatTiming) {

				// 离定位位置（m_scoringUnitSeeker.nextIndex）更近
				nearestIndex = m_scoringUnitSeeker.nextIndex;

			} else {

				// 离定位位置上一个位置（m_scoringUnitSeeker.nextIndex）更近 
				nearestIndex = m_scoringUnitSeeker.nextIndex - 1;
			}
		}

		return(nearestIndex);
	}

	// Update is called once per frame
	void Update () {

		m_additionalScore = 0;

		float	additionalTemper = 0;
		bool	hitBefore = false;
		bool	hitAfter = false;

		if( m_musicManager.IsPlaying() ){

			float	delta_count = m_musicManager.beatCount - m_musicManager.previousBeatCount;

			// 执行单元定位
			m_scoringUnitSeeker.ProceedTime(delta_count);

			// 如果玩家发生了操作，则进行成绩判定
			if(m_playerAction.currentPlayerAction != PlayerActionEnum.None) {

				// 取得位于玩家输入的时机之后，或者之前（取更近的）的标记索引位置
				int		nearestIndex = GetNearestPlayerActionInfoIndex();

				SongInfo	song = m_musicManager.currentSongInfo;

				// 标记的位置（marker_act）
				OnBeatActionInfo 	marker_act = song.onBeatActionSequence[nearestIndex];
				// 玩家的操作输入（player_act）.
				OnBeatActionInfo 	player_act = m_playerAction.lastActionInfo;

				// 计算玩家输入和标记的时间差
				m_lastResult.timingError = player_act.triggerBeatTiming - marker_act.triggerBeatTiming;

				m_lastResult.markerIndex = nearestIndex;

				//比较“距离最近的标记”和“最后输入成功的标记”
				if(nearestIndex == m_previousHitIndex){

					// 对于执行过一次判断的标记，再次输入时
					m_additionalScore = 0;

				} else {

					// 首次被点击的标记
					// 进行时机判断
					m_additionalScore = CheckScore(nearestIndex, m_lastResult.timingError, out additionalTemper);
				}

				if(m_additionalScore > 0){

					// 输入成功

					// 为了避免同一个标记被判断两次，保存最后判断的标记
					m_previousHitIndex = nearestIndex;

					// 判断过程中会用到
					// ・定位位置的标记（hitAftere）
					// ・定位位置前一个标记（hitBefore）
					//
					if(nearestIndex == m_scoringUnitSeeker.nextIndex) {

						hitAfter = true;

					} else {

						hitBefore = true;
					}

					//成功时的演出
					OnScoreAdded(nearestIndex);

				} else {

					// 输入失败（时机相差太多）

					//发生了动作不是加分就是减分
					m_additionalScore = missScore;

					additionalTemper = missHeatupRate;
				}
				m_score += m_additionalScore;

				temper += additionalTemper;

				m_onPlayGUI.RythmHitEffect(m_previousHitIndex, m_additionalScore);

				// 输入用于调试的日志
				DebugWriteLogPrev();
				DebugWriteLogPost(hitBefore, hitAfter);
			}
			if(m_scoringUnitSeeker.nextIndex > 0) {

				m_scoreRate = m_score / (m_scoringUnitSeeker.nextIndex * excellentScore);
			}
		}
	}

	// 判断输入的结果（优秀／不好／失败）
	float CheckScore(int actionInfoIndex, float timingError, out float heatup){

		float	score = 0;

		// 对“偏差时间”取绝对值
		// 太早（负数）或者太晚（正数）都会得到同样的判定
		timingError = Mathf.Abs(timingError);

		do {

			// 大于Good 的范围 → 失败
			if(timingError >= timingErrorToleranceGood) {

				score  = 0.0f;
				heatup = 0;
				break;
			}
			
			// Good 和 Excellent 之间 → Good.
			if(timingError >= timingErrorTorelanceExcellent) {

				score  = goodScore;
				heatup = goodHeatupRate;
				break;
			}

			// Excellent 范围内 → Excellent.
			score  = excellentScore;
			heatup = bestHeatupRate;

		} while(false);

		return(score);
	}

	// 输出调试用的日志
	private	void	DebugWriteLogPrev()
	{
#if UNITY_EDITOR
		if( m_scoringUnitSeeker.isJustPassElement ){
			if(outScoringLog){
				OnBeatActionInfo onBeatActionInfo
					= m_musicManager.currentSongInfo.onBeatActionSequence[m_scoringUnitSeeker.nextIndex-1];
				m_logWriter.WriteLine(
					onBeatActionInfo.triggerBeatTiming.ToString() + ","
					+ "IdealAction,,"
					+ onBeatActionInfo.playerActionType.ToString()
				);
				m_logWriter.Flush();
			}
		}
#endif
	}
	private void	OnScoreAdded(int nearestIndex){
		SongInfo song = m_musicManager.currentSongInfo;
		if (song.onBeatActionSequence[nearestIndex].playerActionType == PlayerActionEnum.Jump
			&& temper > temperThreshold)
		{
			foreach (GameObject bandMember in m_bandMembers)
			{
				bandMember.GetComponent<BandMember>().Jump();
			}
			foreach (GameObject audience in m_audiences)
			{
				audience.GetComponent<Audience>().Jump();
			}
			foreach (GameObject noteParticle in m_noteParticles)
			{
				noteParticle.GetComponent<ParticleSystem>().Emit(20);
			}
		}
		else if (song.onBeatActionSequence[nearestIndex].playerActionType == PlayerActionEnum.HeadBanging)
		{
			foreach (GameObject bandMember in m_bandMembers)
			{
				bandMember.GetComponent<SimpleSpriteAnimation>().BeginAnimation(1, 1);
			}
		}
	}
	// 输出调试用的日志
	private void	DebugWriteLogPost(bool hitBefore, bool hitAfter)
	{
#if UNITY_EDITOR
		if(outScoringLog){
			string relation="";
			if(hitBefore){
				relation = "HIT ABOVE";
			}
			if(hitAfter){
				relation = "HIT BELOW";
			}
			string scoreTypeString = "MISS";
			if( m_additionalScore>=excellentScore )
				scoreTypeString = "BEST";
			else if( m_additionalScore>=goodScore )
				scoreTypeString = "GOOD";
			m_logWriter.WriteLine(
				m_playerAction.lastActionInfo.triggerBeatTiming.ToString() + ","
				+ " PlayerAction,"
				+ relation + " " + scoreTypeString + ","
				+ m_playerAction.lastActionInfo.playerActionType.ToString() + ","
				+ "Score=" + m_additionalScore
			);
			m_logWriter.Flush();
		}
#endif
	}

	//Private
	SequenceSeeker<OnBeatActionInfo>	m_scoringUnitSeeker = new SequenceSeeker<OnBeatActionInfo>();

	float			m_additionalScore;
	MusicManager	m_musicManager;
	PlayerAction	m_playerAction;
	OnPlayGUI		m_onPlayGUI;
	int				m_previousHitIndex = -1;
	GameObject[]	m_bandMembers;
	GameObject[]    m_audiences;
	GameObject[]    m_noteParticles;
    TextWriter		m_logWriter;
	PhaseManager	m_phaseManager;

	// 玩家输入的结果
	public struct Result {

		public float	timingError;		// 时机的偏差（负数⋯⋯太早 正数⋯⋯太晚）
		public int		markerIndex;		// 被比较的标记的索引
	};

	// 上一次玩家输入的结果
	public Result	m_lastResult;
}


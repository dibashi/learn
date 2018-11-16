using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//显示游戏中GUI
public class OnPlayGUI : MonoBehaviour
{
	public static float markerEnterOffset = 2.5f;	// 开始显示标记的时刻（几拍后动作将出现）
	public static float markerLeaveOffset =-1.0f;	// 结束显示标记的时刻（几拍后动作将出现）

	public static int 	messageShowFrameDuration = 40;


	public bool 		isDevelopmentMode = false;
	protected Vector2 	markerOrigin = new Vector2(-140.0f, -60.0f);		// 节奏圆的位置

	public GameObject	uiCanvas;
	public GameObject	markerPrefab = null;

	public UnityEngine.UI.Text		uiScoreText;				// "Score : " + 得分文字
	public UnityEngine.UI.RawImage	uiTemperBarRawImage;		// 伸缩条
	public UnityEngine.UI.Text		uiTemperText;				// "Temper" 文字
	public UnityEngine.UI.Image		uiExcellentImage;			// 用于表示输入结果的图片 Excellent
	public UnityEngine.UI.Image		uiGoodImage;				// 用于表示输入结果的图片　Good.
	public UnityEngine.UI.Image		uiMissImage;				// 用于表示输入结果的图片　Miss.
	public UnityEngine.UI.Image		uiHitImage;					// 输入成功时的特效
	
	// ---------------------------------------------------------------- //

	protected UnityEngine.UI.Image 		ui_message_image;		// 显示中的输入结果图片

	protected const int		MARKER_POOL_COUNT = 16;		// 最多允许显示的标记个数
	protected const float	HIT_EFFECT_ZOOM_DURATION = 10.0f/60.0f;
	protected const float	HIT_EFFECT_DISP_DURATION = 15.0f/60.0f;

	protected List<Marker>	m_markers = new List<Marker>();

	protected float		m_pixelsPerBeats          = Screen.width * 1.0f/markerEnterOffset;
	protected int		m_messageShowCountDown    = 0;
	protected float		m_hit_effect_timer        = -1.0f;
	protected float		m_lastInputScore          = 0;


	// 时间轴上的检索单元（结束位置）
	protected SequenceSeeker<OnBeatActionInfo>	m_seekerFront = new SequenceSeeker<OnBeatActionInfo>();

	// 时间轴上的检索单元（开始位置）
	protected SequenceSeeker<OnBeatActionInfo>	m_seekerBack = new SequenceSeeker<OnBeatActionInfo>();

	protected MusicManager		m_musicManager;
	protected ScoringManager	m_scoringManager;
	protected GameObject		m_playerAvator;

	// ================================================================ //

	void	Awake()
	{
		m_markers.Clear();

		for(int i = 0;i < MARKER_POOL_COUNT;i++) {

			GameObject	marker_go = GameObject.Instantiate(this.markerPrefab);
			Marker		marker    = marker_go.GetComponent<Marker>();

			marker_go.GetComponent<RectTransform>().SetParent(this.uiCanvas.GetComponent<RectTransform>());
			marker.setVisible(false);

			m_markers.Add(marker);
		}
	}

	void Start()
	{
		m_musicManager   = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager = GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
		m_playerAvator   = GameObject.Find("PlayerAvator");

	}
	void	Update()
	{
		if(m_musicManager.IsPlaying()) {

			m_seekerBack.ProceedTime( m_musicManager.beatCountFromStart - m_musicManager.previousBeatCountFromStart);
			m_seekerFront.ProceedTime(m_musicManager.beatCountFromStart - m_musicManager.previousBeatCountFromStart);
		}

		// 显示标记
		this.draw_markers();

		// 显示得分
		this.uiScoreText.text = "Score: " + m_scoringManager.score;

		// 进度条
		this.draw_temper_guage();

		if(m_musicManager.IsPlaying()) {

			// 显示消息（Excellent/Good/Miss）
			this.draw_message();

			// 命中时的特效
			this.draw_hit_effect();
		}
	}

	// ================================================================ //

	public void BeginVisualization()
	{
		m_musicManager   = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager = GameObject.Find("ScoringManager").GetComponent<ScoringManager>();

		m_seekerBack.SetSequence(m_musicManager.currentSongInfo.onBeatActionSequence);
		m_seekerFront.SetSequence(m_musicManager.currentSongInfo.onBeatActionSequence);
		m_seekerBack.Seek(markerLeaveOffset);
		m_seekerFront.Seek(markerEnterOffset);
	}

    public void RythmHitEffect(int actionInfoIndex, float score)
    {
		m_lastInputScore          = score;
		m_hit_effect_timer = 0.0f;
		m_messageShowCountDown    = messageShowFrameDuration;

		AudioClip	clip;

		if(score < 0) {

			clip = m_playerAvator.GetComponent<PlayerAction>().headBangingSoundClip_BAD;
			ui_message_image = uiMissImage;

		} else if(score <= ScoringManager.goodScore) {

			clip = m_playerAvator.GetComponent<PlayerAction>().headBangingSoundClip_GOOD;
			ui_message_image = uiGoodImage;

		} else{

			clip = m_playerAvator.GetComponent<PlayerAction>().headBangingSoundClip_GOOD;
			ui_message_image = uiExcellentImage;
		}

		m_playerAvator.GetComponent<AudioSource>().clip = clip;
		m_playerAvator.GetComponent<AudioSource>().Play();
    }

	public void Seek(float beatCount)
	{
		m_seekerBack.Seek(beatCount + markerLeaveOffset);
		m_seekerFront.Seek(beatCount + markerEnterOffset);
	}

	// ================================================================ //

	// 显示所有的标记
	private void	draw_markers()
	{
		foreach(var marker in m_markers) {

			marker.setVisible(false);
			marker.hideLineNumberText();
		}

		if(m_musicManager.IsPlaying()) {

			SongInfo	song =  m_musicManager.currentSongInfo;

			// 开始显示的标记（检索单元上更晚的那个位置）
			int		begin = m_seekerBack.nextIndex;
			// 结束显示的标记（检索单元上更早的那个位置）
			int		end   = m_seekerFront.nextIndex;

			float	x_offset;
			int		marker_draw_index = 0;

			// 绘制用于提示命中时机的图标
			for(int drawnIndex = begin;drawnIndex < end;drawnIndex++) {

				float 	size = ScoringManager.timingErrorToleranceGood * m_pixelsPerBeats;

				OnBeatActionInfo	info = song.onBeatActionSequence[drawnIndex];

				// tension值高时，跳跃动作的标记也变大
				if(m_scoringManager.temper > ScoringManager.temperThreshold && info.playerActionType == PlayerActionEnum.Jump) {
					size *= 1.5f;
				}

				// 计算节奏圆到标记的X坐标偏移值
				x_offset = info.triggerBeatTiming - m_musicManager.beatCount;
				x_offset *= m_pixelsPerBeats;

				float	pos_x = markerOrigin.x + x_offset;
				float	pos_y = markerOrigin.y;

				m_markers[marker_draw_index].draw(pos_x, pos_y, size);

				// 在开发模式下，显示文本文件中的行号
				if(isDevelopmentMode) {

					m_markers[marker_draw_index].dispLineNumberText(info.line_number);
				}

				marker_draw_index++;
			}
		}
	}

	// 显示进度条
	protected void	draw_temper_guage()
	{
		float	temper = m_scoringManager.temper;

		this.uiTemperBarRawImage.GetComponent<RectTransform>().localScale = new Vector3(temper, 1.0f, 1.0f);
		this.uiTemperBarRawImage.uvRect = new Rect(0.0f, 0.0f, temper, 1.0f);

		// 随进度变化的颜色

		Color	blink_color = Color.white;

		if(m_scoringManager.temper > ScoringManager.temperThreshold) {

			 int	frame_rate = Application.targetFrameRate;

			float	c = 0.7f + 0.3f*Mathf.Abs(Time.frameCount%frame_rate - frame_rate/2)/(float)frame_rate;

			blink_color.g = c;
			blink_color.b = c;
		}

		this.uiTemperBarRawImage.color = blink_color;
		this.uiTemperText.color = blink_color;
	}

	// 显示消息（Excellent/Good/Miss）
	protected void	draw_message()
	{
		this.uiExcellentImage.gameObject.SetActive(false);
		this.uiGoodImage.gameObject.SetActive(false);
		this.uiMissImage.gameObject.SetActive(false);

		if(m_messageShowCountDown > 0) {

			float	alpha = 1.0f;

			if(m_messageShowCountDown > 20.0f) {

				alpha = 1.0f;

			} else {

				alpha = Mathf.InverseLerp(0.0f, 20.0f, m_messageShowCountDown);
			}


			this.ui_message_image.gameObject.SetActive(true);
			this.ui_message_image.color = new Color(1.0f, 1.0f, 1.0f, alpha);
			m_messageShowCountDown--;
		}
	}

	// 命中时的特效
	protected void	draw_hit_effect()
	{

		if(m_hit_effect_timer >= 0.0f) {

			if(m_hit_effect_timer > HIT_EFFECT_DISP_DURATION) {

				m_hit_effect_timer = -1.0f;
			}
		}

		if(m_hit_effect_timer >= 0.0f) {

			float	rate = Mathf.Clamp01(m_hit_effect_timer/HIT_EFFECT_ZOOM_DURATION);

			rate = Mathf.Pow(rate, 0.5f);

			float	scale = Mathf.Lerp(0.5f, 2.0f, rate);

			if(m_lastInputScore >= ScoringManager.excellentScore) {

				scale *= 2.0f;

			} else if(m_lastInputScore > ScoringManager.missScore) {

				scale *= 1.2f;

			} else {

				scale *= 0.5f;
			}

			this.uiHitImage.gameObject.SetActive(true);
			this.uiHitImage.GetComponent<RectTransform>().localScale = Vector3.one*scale;

			m_hit_effect_timer += Time.deltaTime;

		} else {

			this.uiHitImage.gameObject.SetActive(false);
		}
	}


}

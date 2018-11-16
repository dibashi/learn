
using UnityEngine;


/// <summary>该类用于管理画面的淡入淡出</summary>
public class FadeInOutManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 相关的成员变量／方法

	/// <summary>用于淡入淡出的纹理</summary>
	public GUITexture m_fadeInOutObject = null;

	public GameObject			fade_go;
	public UnityEngine.UI.Image	ui_image;

	// ================================================================ //

	void Awake()
	{
		this.fade_go  = this.transform.FindChild("Fade").gameObject;
		this.ui_image = this.fade_go.GetComponent<UnityEngine.UI.Image>();
	}

	/// <summary>启动方法</summary>
	private void Start()
	{
		this.m_currentAlpha = 0.0f;
		this.fade_go.SetActive(false);
		this.ui_image.color = new Color(0.0f, 0.0f, 0.0f, m_currentAlpha);
	}

	/// <summary>每帧更新方法</summary>
	private void Update()
	{
		if ( m_isFading )
		{
			if ( Time.time >= m_endTime )
			{
				// 经过淡入淡出时间后首次 Update

				// 将alpha值改为目标值
				m_currentAlpha = m_destinationAlpha;
				this.ui_image.color = new Color(0.0f, 0.0f, 0.0f, m_currentAlpha);

				// 淡入过程结束后将 GUITexture 设置为无效
				if ( m_destinationAlpha < 0.25f )	// 因为是float类型所以不能通过 == 0.0f 来判断
				{
					this.fade_go.SetActive(false);
				}

				// 淡入淡出结束
				m_isFading = false;
			}
			else
			{
				// 进度（0.0～1.0）
				float ratio = Mathf.InverseLerp( m_beginTime, m_endTime, Time.time );

				// 修改alpha值
				this.ui_image.color = new Color(0.0f, 0.0f, 0.0f, Mathf.Lerp(m_currentAlpha, m_destinationAlpha, ratio));
			}
		}
	}


	//==============================================================================================
	// 公开方法

	/// <summary>执行淡入淡出</summary>
	public void fadeTo( float destinationAlpha, float duration )
	{
		m_destinationAlpha = destinationAlpha;
		m_beginTime        = Time.time;
		m_endTime          = m_beginTime + duration;
		m_isFading         = true;

		this.fade_go.SetActive(true);
	}

	/// <summary>执行淡出</summary>
	public void fadeOut( float duration )
	{
		fadeTo( 1.0f, duration );
	}

	/// <summary>执行淡入</summary>
	public void fadeIn( float duration )
	{
		fadeTo( 0.0f, duration );
	}

	/// <summary>判断是否正在执行淡入淡出</summary>
	public bool isFading()
	{
		return m_isFading;
	}


	//==============================================================================================
	// 非公开变量

	/// <summary>当前的alpha值</summary>
	private float m_currentAlpha = 1.0f;

	/// <summary>目标alpha值</summary>
	private float m_destinationAlpha = 0.0f;

	/// <summary>淡入淡出的开始时间</summary>
	private float m_beginTime = 0.0f;

	/// <summary>淡入淡出的结束时间</summary>
	private float m_endTime = 0.0f;

	/// <summary>是否正在执行淡入淡出</summary>
	private bool m_isFading = false;

	// ================================================================ //
	// 实例

	public	static FadeInOutManager	instance = null;

	public static FadeInOutManager	get()
	{
		if(FadeInOutManager.instance == null) {

			GameObject		go = GameObject.Find("TextManager");

			if(go != null) {

				FadeInOutManager.instance = go.GetComponent<FadeInOutManager>();

			} else {

				Debug.LogError("Can't find game object \"FadeInOutManager\".");
			}
		}

		return(FadeInOutManager.instance);
	}
}

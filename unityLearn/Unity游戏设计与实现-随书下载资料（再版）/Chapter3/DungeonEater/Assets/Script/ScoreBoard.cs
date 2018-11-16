using UnityEngine;
using System.Collections;

public class ScoreBoard : MonoBehaviour {

 	protected const float	SPEED = 100.0f; 			// 上升的速度

	protected float		m_lifeTime = 3.0f;				// [sec] 显示的时间
	
	protected Vector3	m_position;
	protected float		m_offsetY;

	protected UnityEngine.UI.Text	m_ui_text;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
		m_ui_text = this.GetComponent<UnityEngine.UI.Text>();
	}

	void	Start()
	{
		m_offsetY = 0;
	}

	void 	Update()
	{
		this.GetComponent<RectTransform>().localPosition = m_position + new Vector3(0,m_offsetY,0);

		m_offsetY += SPEED*Time.deltaTime;
	
		m_lifeTime -= Time.deltaTime;
		if (m_lifeTime < 0.0f)
			Destroy(gameObject);
	}

	// ================================================================ //

	// 设置开始显示的位置
	public void	SetPosition(Vector3 position)
	{
		position = Camera.main.WorldToScreenPoint(position);

		position.x -= Screen.width/2.0f;
		position.y -= Screen.height/2.0f;

		m_position = position;
	}	

	// 设置得分
	public void SetScore(int score)
	{
		m_ui_text.text = score.ToString();
	}
	
}

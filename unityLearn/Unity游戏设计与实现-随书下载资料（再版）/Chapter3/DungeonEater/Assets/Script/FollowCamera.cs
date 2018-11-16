// 有効にすると、スペースキーでズームイン/ズームアウトができるようになります.
//#define	ENABLE_ZOOM_IN_DEBUG

using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

	private const	float	ZOOM_IN_DURATION  = 0.2f;		// [sec] 推近所持续的时间
	private const	float	ZOOM_OUT_DURATION = 1.5f;		// [sec] 拉远持续的时间

	public Vector3		m_position_offset;
	public Vector3		m_position_offset_zoom_in;

	public Transform	m_target;
	private bool		m_zoom_in;
	private float		m_zoom_in_timer = 0.0f;

	// Use this for initialization
	void Start ()
	{
		// 推近结束=拉远状态开始
		m_zoom_in = false;
		m_zoom_in_timer = ZOOM_OUT_DURATION;
	}

	void	Update()
	{
#if ENABLE_ZOOM_IN_DEBUG
		if(Input.GetKeyDown(KeyCode.Space)) {

			if(m_zoom_in) {

				OnEndAttack();

			} else {

				OnAttack();
			}
		}
#endif

	}
	
	// Update is called once per frame
	void	LateUpdate()
	{
		float	rate = 0.0f;

		if(m_zoom_in) {

			// 推近
			rate = m_zoom_in_timer/ZOOM_IN_DURATION;			// 将“时间”变换为“整体进度比率”
			rate = Mathf.Clamp01(rate);							// 限制在0.0 到 1.0 之间
			rate = Mathf.Sin(rate*Mathf.PI/2.0f);
			rate = Mathf.Pow(rate, 0.5f);

		} else {

			// 拉远
			rate = m_zoom_in_timer/ZOOM_OUT_DURATION;
			rate = Mathf.Clamp01(rate);
			rate = Mathf.Sin(rate*Mathf.PI/2.0f);
			rate = Mathf.Pow(rate, 0.5f);

			// 计时器值为 0.0 ~ 1.0 时，缩放率值处于 0.0 ~ 1.0
			rate = 1.0f - rate;
		}

		// 计算位置偏移
		Vector3		offset = Vector3.Lerp(m_position_offset, m_position_offset_zoom_in, rate);
		transform.position = m_target.position + offset;

		// 计算视角，设定
		float	fov = Mathf.Lerp(60.0f, 30.0f, rate);
		this.GetComponent<Camera>().fieldOfView = fov;

		m_zoom_in_timer += Time.deltaTime;
	}
	
	// 攻击/被攻击时调用
	public void OnAttack()
	{
#if false
		m_zoom_in = true;
		m_zoom_in_timer = 0.0f;
#else
	// 拉远的过程中，即使开始推近，也不会出问题
	// 代码可能有些复杂，短时间内不能理解也不要紧

		if(m_zoom_in) {

			// 因为正在推近因此无需任何处理

		} else {

			m_zoom_in = true;

			if(m_zoom_in_timer < ZOOM_OUT_DURATION) {

				// 因为正处于拉远过程中，因此在同一位置切换为推近状态
				// 变换计时器

				float	rate;

				// 根据计时器求出缩放率
				rate = m_zoom_in_timer/ZOOM_OUT_DURATION;
				rate = Mathf.Clamp01(rate);
				rate = Mathf.Sin(rate*Mathf.PI/2.0f);
				rate = Mathf.Pow(rate, 0.5f);
				rate = 1.0f - rate;

				// 根据缩放率，对推近时的计时器值进行逆变换
				rate = Mathf.Pow(rate, 1.0f/0.3f);
				rate = Mathf.Asin(rate)/(Mathf.PI/2.0f);
				m_zoom_in_timer = ZOOM_IN_DURATION*rate;


			} else {

				m_zoom_in_timer = 0.0f;
			}
		}
#endif
	}
	
	public void OnEndAttack()
	{
#if false
		m_zoom_in = false;
		m_zoom_in_timer = 0.0f;
#else
	// 在推近的过程中，即使开始拉远，也不会出现问题
	// 代码可能有些复杂，短时间内不能理解也不要紧
		if(!m_zoom_in) {

		} else {

			m_zoom_in = false;

			if(m_zoom_in_timer < ZOOM_IN_DURATION) {

				float	rate;

				rate = m_zoom_in_timer/ZOOM_IN_DURATION;
				rate = Mathf.Clamp01(rate);
				rate = Mathf.Sin(rate*Mathf.PI/2.0f);
				rate = Mathf.Pow(rate, 0.3f);

				rate = 1.0f - rate;
				rate = Mathf.Pow(rate, 1.0f/0.5f);
				rate = Mathf.Asin(rate)/(Mathf.PI/2.0f);
				m_zoom_in_timer = ZOOM_OUT_DURATION*rate;

			} else {

				m_zoom_in_timer = 0.0f;
			}
		}

#endif

	}
}

using UnityEngine;
using System.Collections;

public class CharaAnimatorBase : MonoBehaviour {

	protected GridMove	m_move;
	protected bool 		m_dead = false;

	protected  const uint PAUSE_NONE       = 0;
	protected  const uint PAUSE_GAME       = 1;
	protected  const uint PAUSE_ATTACK     = 2;
	protected  const uint PAUSE_STAGECLESR = 4;
	
	protected uint  	m_pauseFlag = PAUSE_NONE; // 暂停中标记.

	protected Animator	m_animator;
}

public class CharaAnimator : CharaAnimatorBase {


	// Use this for initialization
	void	Awake()
	{
		m_animator = GetComponent<Animator>();

		if(m_animator == null) {

			Debug.LogError("Can't find Animator component.");
		}
	}
	void 	Start()
	{
		m_move = GetComponent<GridMove>();
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;
	}
	
	public void OnRestart()
	{
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;

		m_animator.Play("idle", 0);
		m_animator.Play("idle", 1);
	}
	public void OnGameStart()
	{
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;

		m_animator.Play("idle", 0);
		m_animator.Play("idle", 1);
	}
	public void OnStageStart()
	{
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;

		m_animator.Play("idle", 0);
		m_animator.Play("idle", 1);
	}
	
	
	// Update is called once per frame
	public virtual void Update () {

		if ((m_pauseFlag & PAUSE_GAME) != 0)
			return;
		// 旋转
		Quaternion targetRotation = Quaternion.LookRotation(m_move.GetDirection());
		if ((m_pauseFlag & PAUSE_ATTACK) == 0) {
			float t = 1.0f - Mathf.Pow(0.75f,Time.deltaTime*30.0f);
			transform.localRotation = MathUtil.Slerp(transform.localRotation,targetRotation,t);
		}

		m_animator.SetBool("is_walking", m_move.IsRunning());
	}
	
	public void OnDead()
	{
		m_dead = true;

		m_animator.SetTrigger("begin_dead");
		m_animator.SetTrigger("begin_dead_l_arm");
	}
	
	public void OnRebone()
	{
		m_dead = false;
	}
	
	public void HitStop(bool enable)
	{
		if (enable)
			m_pauseFlag |= PAUSE_ATTACK;
		else
			m_pauseFlag &= ~PAUSE_ATTACK;
	}
	
	
}

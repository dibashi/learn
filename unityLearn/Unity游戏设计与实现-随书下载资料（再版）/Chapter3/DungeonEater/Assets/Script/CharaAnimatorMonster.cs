using UnityEngine;
using System.Collections;

public class CharaAnimatorMonster : CharaAnimatorBase {

	
	// Update is called once per frame
	void	Awake()
	{
		m_dead      = false;
		m_pauseFlag = PAUSE_NONE;

		m_move     = GetComponent<GridMove>();
		m_animator = GetComponent<Animator>();
		if(m_animator == null) {

			Debug.LogError("Can't find Animator component.");
		}
	}
	public void		Update () 
	{
		if ((m_pauseFlag & PAUSE_GAME) != 0)
			return;
		// 旋转
		Quaternion targetRotation = Quaternion.LookRotation(m_move.GetDirection());
		if ((m_pauseFlag & PAUSE_ATTACK) == 0) {
			float t = 1.0f - Mathf.Pow(0.75f,Time.deltaTime*30.0f);
			transform.localRotation = MathUtil.Slerp(transform.localRotation,targetRotation,t);
		}
	}

	public void OnRestart()
	{
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;

		m_animator.Play("idle");
	}
	public void OnGameStart()
	{
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;

		m_animator.Play("idle");
	}
	public void OnStageStart()
	{
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;

		m_animator.Play("idle");
	}

	public void OnDead()
	{
		m_dead = true;

		m_animator.SetTrigger("begin_dead");
	}
	
	public void OnRebone()
	{
		m_dead = false;
		m_animator.SetTrigger("begin_idle");
	}
	
	public void HitStop(bool enable)
	{
		if (enable)
			m_pauseFlag |= PAUSE_ATTACK;
		else
			m_pauseFlag &= ~PAUSE_ATTACK;
	}
}

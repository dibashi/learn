using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	public GameCtrl 		m_gameCtrl;			// 游戏
	public GameObject		m_sword;			// 玩家的剑
	public GameObject		m_scoreBorad;		// 显示得分的对象
	private AudioChannels	m_audio;			// 音频
	public AudioClip 		m_swordAttackSE;	// 攻击音效
	public GameObject 		SWORD_ATTACK_OBJ;	// 攻击范围对象
	
	private bool 		m_equiped = false;  // 装备了宝剑
	private Transform 	m_target;  // 攻击对象
	
	// 得分
	private const int POINT = 500;
	private const int COMBO_BONUS = 200;

	private int 	m_combo = 0;
	
	protected Animator	m_animator;

	// 初始化
	void	Awake()
	{
		m_animator = GetComponent<Animator>();

		if(m_animator == null) {

			Debug.LogError("Can't find Animator component.");
		}
	}

	void 	Start()
	{
		m_equiped = false;
		m_sword.GetComponent<Renderer>().enabled = false;

		m_audio = FindObjectOfType(typeof(AudioChannels)) as AudioChannels;
		m_combo = 0;
	}
	
	// 关卡开始时
	void OnStageStart()
	{
		m_equiped = false;
		m_sword.GetComponent<Renderer>().enabled = false;
	}
	
	// 拾起宝剑
	void OnGetSword()
	{
		if (!m_equiped) {
			m_sword.GetComponent<Renderer>().enabled = true;
			m_equiped = true;
			m_animator.SetTrigger("begin_idle_sword");
		} else {
			int point = POINT + COMBO_BONUS * m_combo;

			Hud.get().CreateScoreBoard(this.transform.position, point);
			Hud.get().AddScore(point);
			m_combo++;
		}
	}
	
	void Remove()  
	{
		m_sword.GetComponent<Renderer>().enabled = false;
		m_equiped = false;

		// 为了同步层上的动画（防止左右手的摆动不匹配）
		// 重置状态（不执行补间）
		m_animator.Play("idle", 0);
		m_animator.Play("idle", 1);

		m_combo = 0;
	}

	
	// 自动攻击
	public void AutoAttack(Transform other)
	{
		if (m_equiped) {
			m_target = other;
			StartCoroutine("SwordAutoAttack");
		}
	}
	
	// 能够攻击吗？
	public bool CanAutoAttack()
	{
		if (m_equiped)
			return true;
		else
			return false;
	}
		
	
	IEnumerator SwordAutoAttack()
	{
		m_gameCtrl.OnAttack();

		// 回转
		Vector3		target_pos = m_target.transform.position;
		target_pos.y = transform.position.y;
		transform.LookAt(target_pos);
		yield return null;

		// 攻击
		m_animator.SetTrigger("begin_attack");
		yield return new WaitForSeconds(0.3f);

		m_audio.PlayOneShot(m_swordAttackSE,1.0f,0.0f);		
		yield return new WaitForSeconds(0.2f);

		Vector3 projectilePos;
		projectilePos = transform.position + transform.forward * 0.5f;
		Instantiate(SWORD_ATTACK_OBJ,projectilePos,Quaternion.identity);
		yield return null;

		// 回到原来的方向
		Remove();
		m_gameCtrl.OnEndAttack();
	}
}

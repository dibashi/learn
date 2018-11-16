using UnityEngine;
using System.Collections;

public class MagicHand : MonoBehaviour {

	public GameObject		model;

	public DraggableObject	target;

	protected Animator		animator;

	public Vector3 		m_magicHandOffset = Vector3.zero;

	protected RaycastHit	raycast_hit;

	// -------------------------------------------------------- //
		
	public enum STEP {
		
		NONE = -1,

		HIDE = 0,
		PICKING,
		RELEASE,

		NUM,
	};
	public Step<STEP>			step = new Step<STEP>(STEP.NONE);

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
		this.animator = this.GetComponentInChildren<Animator>();

		this.model.SetActive(false);
	}

	void	Start()
	{
		this.step.set_next(STEP.HIDE);
	}
	
	void	Update()
	{
		float	delta_time = Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 检测是否需要迁移到下一状态

		switch(this.step.do_transition()) {

			case STEP.HIDE:
			{
				do {

					// 如果正在处理事件则忽略
					if(EventManager.get().isExecutingEvents()) {
						break;
					}

					if(!Input.GetMouseButton(0)) {
						break;
					}

					// 检测鼠标位置落在哪个游戏对象上
					Ray		ray = Camera.main.ScreenPointToRay(Input.mousePosition);

					if(!Physics.Raycast(ray, out this.raycast_hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Draggable"))) {
						break;
					}

					if(this.raycast_hit.rigidbody == null) {
						break;
					}

					// 获取DraggableObject组件
					this.target = this.raycast_hit.rigidbody.gameObject.GetComponent<DraggableObject>();

					if(this.target == null) {
						break;
					}

					this.step.set_next(STEP.PICKING);

				} while(false);
			}
			break;

			case STEP.PICKING:
			{
				if(!Input.GetMouseButton(0)) {
					this.step.set_next(STEP.RELEASE);
				}
			}
			break;

			case STEP.RELEASE:
			{
				if(this.animator.GetCurrentAnimatorStateInfo(0).IsName("release")) {
					this.step.set_next(STEP.HIDE);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 状态发生迁移时的初始化

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.HIDE:
				{
					this.model.SetActive(false);
				}
				break;

				case STEP.PICKING:
				{
					this.model.SetActive(true);

					// 显示magicHand・移动相应的对象・播放拾起的动画
					Vector3		position = m_magicHandOffset + this.target.transform.position + this.target.getYTop()*Vector3.up;

					this.transform.position = position;
					this.animator.SetTrigger("pick");

					// 播放拖动开始时的音效
					this.GetComponent<AudioSource>().Play();

					// 通知拖动开始
					this.target.onDragBegin(this.raycast_hit);
				}
				break;

				case STEP.RELEASE:
				{
					this.animator.SetTrigger("to_release");

					// 通知拖动结束
					this.target.onDragEnd();
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 各个状态的执行处理

		switch(this.step.do_execution(delta_time)) {

			case STEP.PICKING:
			{
				// 移动magicHand
				Vector3		position = m_magicHandOffset + this.target.transform.position + this.target.getYTop()*Vector3.up;

				this.transform.position = position;

				this.target.onDragUpdate();
			}
			break;
		}
	}
}

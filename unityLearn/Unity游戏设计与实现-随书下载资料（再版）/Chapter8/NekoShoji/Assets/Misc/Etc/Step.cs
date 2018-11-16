using UnityEngine;
using System.Collections;

// 对执行步骤进行管理
public class Step<T> where T : struct {

	// none 的初值应当为 "NONE(-1)" 
	public Step(T none)
	{
		this.none = none;

		if(this.none.ToString() != "NONE") {

			Debug.Log(typeof(T).ToString() + ": none must be NONE.");
		}

		init();
	}

	public T	get_none()
	{
		return(this.none);
	}

	public void	init()
	{
		this.previous = this.none;
		this.current  = this.none;
		this.next     = this.none;

		this.previous_time = -1.0f;
		this.time = 0.0f;
		this.count = 0;

		this.status.is_changed = false;

		this.delay.delay = -1.0f;
		this.delay.next  = this.none;
	}

	public void	release()
	{
		this.init();
	}

	// 设置下一步骤
	public void	set_next(T step)
	{
		this.next = step;
	}
	// 获取下一步骤
	public T	get_next()
	{
		return(this.next);
	}

	public bool	is_has_next()
	{
		return(!this.next.Equals(this.none));
	}

	// 等待delay[sec]后迁移到下一状态
	public void	set_next_delay(T step, float delay)
	{
		this.next = this.none;

		this.delay.delay = delay;
		this.delay.next  = step;
	}

	// 获取现在的步骤
	public T	get_current()
	{
		return(this.current);
	}
	// 获取上一步骤
	public T	get_previous()
	{
		return(this.previous);
	}

	// 是否正在发生步骤迁移？
	public bool	is_changed()
	{
		return(this.status.is_changed);
	}

	// [sec] 获取该步骤内的流逝时间
	public float	get_time()
	{
		return(this.time);
	}

	// [sec] 获取上一次执行时的结果时间
	public float	get_previous_time()
	{
		return(this.previous_time);
	}

	// 迁移判断
	public T	do_transition()
	{
#if true
		return(this.do_transition_internal());
#else
		T	step;

		step = this.current;

		return(step);
#endif
	}

	// 迁移判断（仅限内部迁移）
	public T	do_transition_internal()
	{
		T	step;

		if(!this.delay.next.Equals(this.none)) {

			step = this.none;

			if(this.delay.delay <= 0.0f) {

				this.next = this.delay.next;
				this.delay.delay = -1.0f;
				this.delay.next  = this.none;
			}

		} else {

			if(this.next.Equals(this.none)) {
	
				step = this.current;
	
			} else {
	
				// 如果已经决定迁移了（收到来自外部的请求）则不执行操作
	
				step = this.none;
			}
		}

		return(step);
	}

	// 开始
	public T		do_initialize()
	{
		T	step;

		if(!this.next.Equals(this.none)) {

			step = this.next;

			this.previous = this.current;
			this.current  = this.next;
			this.next     = this.none;
			this.time     = -1.0f;
			this.count    = 0;

			this.status.is_changed = true;

		} else {

			// 没有启动项（未发生迁移）
			//
			step = this.none;

			this.status.is_changed = false;
		}

		return(step);
	}

	// 执行
	public T		do_execution(float passage_time)
	{
		T	step;

		if(this.delay.delay >= 0.0f) {

			this.delay.delay -= passage_time;

			step = this.none;

		} else {

			if(!this.current.Equals(this.none)) {
	
				step = this.current;
	
			} else {
	
				step = this.none;
			}
	
			this.count++;
	
			this.previous_time = this.time;
	
			if(this.time < 0.0f) {
	
				this.time = 0.0f;
	
			} else {
	
				this.time += passage_time;
			}
		}

		return(step);
	}

	// 暂停（没有步骤了）时使用
	public void		sleep()
	{
		this.current = this.none;
	}

	// 是否跨越了时间区间？
	public bool		is_acrossing_time(float time)
	{
		bool	ret = (this.previous_time < time && time <= this.time);

		return(ret);
	}

	public bool		is_acrossing_cycle(float cycle)
	{
		bool	ret = (Mathf.Ceil(this.previous_time/cycle) < Mathf.Ceil(this.time/cycle));

		return(ret);
	}

	// ---------------------------------------------------------------- //

	protected	T		previous;
	protected	T		current;
	protected	T		next;

	protected	T		none;

	protected 	float		time;					// STEP 改变后经过的时间
	protected 	float		previous_time;			// 上次执行do_execution()的时间
	protected 	int			count;

	protected struct Status {

		public	bool		is_changed;
	};
	protected Status	status;

	protected struct Delay {

		public float		delay;
		public T			next;
	};
	protected Delay	delay;
};

// 使用方法
#if false

		// ---------------------------------------------------------------- //
		// 检测是否可以迁移到下一个状态

		switch(this.step.do_transition()) {

			case STEP.IDLE:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 状态迁移时的初始化

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.STAND:
				{
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 各个状态的执行处理

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.STAND:
			{
			}
			break;
		}

#endif


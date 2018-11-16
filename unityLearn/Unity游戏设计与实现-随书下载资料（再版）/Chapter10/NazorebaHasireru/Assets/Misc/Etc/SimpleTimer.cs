using UnityEngine;
using System.Collections;

public class SimpleTimer {

	public float	current  =  0.0f;
	public float	previous = -1.0f;


	public void		update(float delta_time)
	{
		this.previous = this.current;
		this.current += delta_time;
	}

	public bool		is_accross_local_time(float repeat_time, float local_time)
	{
		bool	ret = false;
		float	time0 = Mathf.Repeat(this.previous, repeat_time);
		float	time1 = Mathf.Repeat(this.current,  repeat_time);

		ret = (time0 < local_time && local_time <= time1);

		return(ret);
	}

}

using UnityEngine;
using System.Collections;

// 刹车声
public class CarSoundSqueal : MonoBehaviour {

	public float	squal_speed_min = 0.1f;			// 刹车声刚开始时，横滑的速度
	public float	squal_speed_max = 1.0f;			// 刹车声最大时，横滑的速度

	public    AudioClip		audio_clip = null;
	protected AudioSource 	audio_source = null;

	protected CarControl	car_control = null;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
		this.car_control = this.gameObject.GetComponent<CarControl>();
	}

	void 	Start()
	{
		this.audio_source = this.gameObject.AddComponent<AudioSource>();

		this.audio_source.loop        = true;
		this.audio_source.playOnAwake = true;
		this.audio_source.clip        = this.audio_clip;
		this.audio_source.volume      = 0.0f;
	}
	
	void 	Update()
	{
		float	squal_max = 0.0f;

		foreach(var wheel in this.car_control.wheels.wheels) {

			float		squeal = 0.0f;

			WheelCollider	wc = wheel.collider;
			WheelHit		wh;

			if(!wc.GetGroundHit(out wh)) {

				continue;
			}

			wheel.wheelVelo   = GetComponent<Rigidbody>().GetPointVelocity(wh.point);
			wheel.groundSpeed = wheel.tire_model.transform.parent.InverseTransformDirection(wheel.wheelVelo);

			float	skidGroundSpeed = Mathf.InverseLerp(this.squal_speed_min, this.squal_speed_max, Mathf.Abs(wheel.groundSpeed.x));

			if(!(skidGroundSpeed > 0.0f /*|| handbrakeSkidding > 0.0f*/)) {

				continue;
			}
			squeal = Mathf.Clamp01(skidGroundSpeed);

			squal_max = Mathf.Max(squeal, squal_max);
		}

		this.set_squeal(squal_max);
	}


	// ================================================================ //

	public void		set_squeal(float squel)
	{
		float		next_volume = squel;
				
		float		diff = next_volume - this.audio_source.volume;
			
		diff = Mathf.Clamp(diff, -0.1f, 0.1f);
	
		next_volume = Mathf.Clamp01(this.audio_source.volume + diff);
		
		if(next_volume <= 0.0f) {

			if(this.audio_source.volume > 0.0f) {

				this.audio_source.Stop();
			}

		} else {

			if(this.audio_source.volume <= 0.0f) {

				this.audio_source.Play();
			}
		}

		this.audio_source.volume = next_volume;
	}
}

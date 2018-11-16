using UnityEngine;
using System.Collections;

public class CarSoundControl : MonoBehaviour {

	public AudioClip	audio_clip_engine;			// 引擎声音（循环）
	public AudioClip	audio_clip_hit_wall;		// 碰撞墙壁声音
	public AudioClip	audio_clip_landing;			// 落地声音（跃起后）

	public AudioSource	audio_engine;

	protected bool	is_contact_wall = false;		// 是否和墙壁发生碰撞？
	protected float	wall_hit_timer = 0.0f;			// 撞墙后的计时器
	protected float	hit_speed_wall = 0.0f;			// 撞墙后的速度

	protected bool	is_landing = false;				// 是否着陆？
	protected float	landing_timer = 0.0f;			// 着陆后的计时器
	protected float	landing_speed = 0.0f;			// 着陆时的速度

	// ================================================================ //
	// 继承MonoBehaviour

	void 	Start()
	{

		this.audio_engine = this.gameObject.AddComponent<AudioSource>();

		this.audio_engine.clip = this.audio_clip_engine;
		this.audio_engine.loop = true;
		this.audio_engine.Play();
	}
	
	void 	Update()
	{
	
		// 根据速度来调整声音频率
	
		float		rate;
		float		pitch;
	
		float		speed0 = 0.0f;
		float		speed1 = 60.0f;
	
		float		pitch0 = 1.0f;
		float		pitch1 = 2.0f;
	
		rate = Mathf.InverseLerp(speed0, speed1, this.GetComponent<Rigidbody>().velocity.magnitude);
		rate = Mathf.Clamp01(rate);
	
		pitch = Mathf.Lerp(pitch0, pitch1, rate);
	
		this.audio_engine.pitch = pitch;

		//

		// 控制撞墙的声音
		this.wall_hit_control();

		// 控制着陆的声音
		this.landing_control();

		//

		this.is_contact_wall = false;
		this.is_landing = false;
	}

	// ================================================================ //

	// 控制撞墙的声音
	private void	wall_hit_control()
	{
		if(this.wall_hit_timer > 0.0f) {

			this.wall_hit_timer -= Time.deltaTime;

		} else {

			if(this.is_contact_wall) {
	
				float		speed0 = 1.0f;
				float		speed1 = 10.0f;
				float		rate;
				float		volume;
	
				rate = Mathf.InverseLerp(speed0, speed1, this.hit_speed_wall);
				rate = Mathf.Clamp01(rate);
	
				volume = Mathf.Lerp(0.1f, 1.0f, rate);
	
				this.GetComponent<AudioSource>().volume = volume;
				this.GetComponent<AudioSource>().PlayOneShot(this.audio_clip_hit_wall);

				this.wall_hit_timer = 1.0f;

				//Debug.Log("speed " + this.hit_speed_wall.ToString() + ":vol " + volume);

			} else {

				this.wall_hit_timer = 0.0f;
			}
		}

	}

	private static float	LANDING_SPEED_MIN = 1.0f;			// 着陆声最小时的车速
	private static float	LANDING_SPEED_MAX = 5.0f;			// 着陆声最大时的车速

	// 控制着陆的声音
	private void	landing_control()
	{
		if(this.landing_timer > 0.0f) {

			this.landing_timer -= Time.deltaTime;

		} else {

			bool	is_play_sound = false;

			do {

				if(!this.is_landing) {

					break;
				}
				if(this.landing_speed < LANDING_SPEED_MIN) {

					break;
				}

				is_play_sound = true;

			} while(false);

			if(is_play_sound) {

				float		speed0 = LANDING_SPEED_MIN;
				float		speed1 = LANDING_SPEED_MAX;
				float		rate;
				float		volume;
	
				rate = Mathf.InverseLerp(speed0, speed1, this.landing_speed);
				rate = Mathf.Clamp01(rate);
	
				volume = Mathf.Lerp(0.5f, 1.0f, rate);
	
				this.GetComponent<AudioSource>().volume = volume;
				this.GetComponent<AudioSource>().PlayOneShot(this.audio_clip_landing);

				this.landing_timer = 1.0f;

				//Debug.Log("speed " + this.landing_speed.ToString() + ":vol " + volume);

			} else {

				this.landing_timer = 0.0f;
			}
		}

	}

	// 如果仅调用OnCollisionEnter() 会丢失比较多信息，还需要调用OnCollisionStay()
	void 	OnCollisionEnter(Collision other)
	{
		this.collision_common(other);
	}
	void 	OnCollisionStay(Collision other)
	{
		this.collision_common(other);
	}

	private void	collision_common(Collision other)
	{
		foreach(var contact in other.contacts) {

			float	d = Vector3.Dot(contact.normal, Vector3.up);

			// 如果发生碰撞的多边形法线接近水平，则视作墙壁
			//
			if(Mathf.Cos(80.0f*Mathf.Deg2Rad) > Mathf.Abs(d)) {

				this.is_contact_wall = true;

				// this.rigidbody.velocity 是碰撞后的速度（？）
				// 不是碰撞前的速度
				// 使用other.relativeVelocity

				this.hit_speed_wall = Vector3.Dot(contact.normal, other.relativeVelocity);

				if(this.hit_speed_wall < 0.0f) {

					this.hit_speed_wall = 0.0f;
				}
			}

			// 如果发生碰撞的多边形法线接近于垂直，视作地面
			//
			if(Mathf.Cos(10.0f*Mathf.Deg2Rad) < Mathf.Abs(d)) {

				this.is_landing = true;

				this.landing_speed = Vector3.Dot(contact.normal, other.relativeVelocity);

				if(this.landing_speed < 0.0f) {

					this.landing_speed = 0.0f;
				}
			}
		}

	}
}

using UnityEngine;
using System.Collections;

public class OniEmitterControl : MonoBehaviour {

	public GameObject[]	oni_prefab;

	// SE.
	public AudioClip	EmitSound = null;		// 从远处飞来的声音（咻～）、
	public AudioClip	HitSound = null;		// 怪物掉在怪物山上时的声音

	// 最后生成的怪物
	private GameObject	last_created_oni = null;

	private const float	collision_radius = 0.25f;

	// 生成的怪物数量（剩余）
	// 实际的数值根据结果会发生变化
	public int		oni_num = 2;

	public bool		is_enable_hit_sound = true;

	// -------------------------------------------------------------------------------- //

	void Start()
	{
		this.GetComponent<AudioSource>().PlayOneShot(this.EmitSound);
	}

	void 	Update()
	{

		do {

			if(this.oni_num <= 0) {

				break;
			}

			// 等待最后生成的怪物也彻底离开
			// （在同一位置重叠生成的话，会因为碰撞器大小被弹开）
			if(this.last_created_oni != null) {

				if(Vector3.Distance(this.transform.position, last_created_oni.transform.position) <= OniEmitterControl.collision_radius*2.0f) {

					break;
				}
			}

			Vector3	initial_position = this.transform.position;

			initial_position.y += Random.Range(-0.5f, 0.5f);
			initial_position.z += Random.Range(-0.5f, 0.5f);

			// 旋转（加上随机数控制能看见就行）
			Quaternion	initial_rotation;

			initial_rotation = Quaternion.identity;
			initial_rotation *= Quaternion.AngleAxis(this.oni_num*50.0f, Vector3.forward);
			initial_rotation *= Quaternion.AngleAxis(this.oni_num*30.0f, Vector3.right);

			GameObject oni = Instantiate(this.oni_prefab[this.oni_num%2], initial_position, initial_rotation) as GameObject;	

			//

			oni.GetComponent<Rigidbody>().velocity        = Vector3.down*1.0f;
			oni.GetComponent<Rigidbody>().angularVelocity = initial_rotation*Vector3.forward*5.0f*(this.oni_num%3);

			oni.GetComponent<OniStillBodyControl>().emitter_control = this;

			//

			this.last_created_oni = oni;

			this.oni_num--;

		} while(false);

	}

	// 播放怪物落在怪物山上时的音效
	//
	// 在短间隔内多次播放音效可能影响效果，调整其按照一定的间隔播放
	//
	public void	PlayHitSound()
	{
		if(this.is_enable_hit_sound) {

			bool	to_play = true;
	
			if(this.GetComponent<AudioSource>().isPlaying) {
	
				if(this.GetComponent<AudioSource>().time < this.GetComponent<AudioSource>().clip.length*0.75f) {
	
					to_play = false;
				}
			}
	
			if(to_play) {
	
				this.GetComponent<AudioSource>().clip = this.HitSound;
				this.GetComponent<AudioSource>().Play();
			}
		}
	}

}

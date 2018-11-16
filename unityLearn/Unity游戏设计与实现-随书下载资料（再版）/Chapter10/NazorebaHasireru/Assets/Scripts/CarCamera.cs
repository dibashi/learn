using UnityEngine;
using System.Collections;

public class CarCamera : MonoBehaviour
{
	public Transform	target = null;

	public float		height          = 1.0f;
	public float		positionDamping = 30.0f;
	public float		velocityDamping = 30.0f;
	public float		distance        = 4.0f;

	public LayerMask	ignoreLayers = -1;

	// ---------------------------------------------------------------- //

	private RaycastHit	hit = new RaycastHit();

	private Vector3		prev_velocity = Vector3.zero;
	private LayerMask	raycastLayers = -1;
	
	private Vector3		current_velocity = Vector3.zero;

	protected const float	USE_DIR_START_SPEED = 1.0f;
	protected const float	USE_DIR_END_SPEED   = 0.01f;
	
	// ================================================================ //
	// 继承于MonoBehaviour

	void 	Start()
	{
		raycastLayers = ~ignoreLayers;
	}

	void 	FixedUpdate()
	{
		Vector3		target_velocity;

		target_velocity = target.root.GetComponent<Rigidbody>().velocity;

		// 由于速度大小接近0时方向会飘忽不定
		// 所以这里使用车的朝向替代
		if(target_velocity.magnitude <= USE_DIR_START_SPEED) {

			float	rate = Mathf.InverseLerp(USE_DIR_START_SPEED, USE_DIR_END_SPEED, target_velocity.magnitude);

			rate = Mathf.Clamp01(rate);
			rate = Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, rate);
			rate = Mathf.Sin(rate);
			rate = Mathf.InverseLerp(-1.0f, 1.0f, rate);

			if(Vector3.Dot(target_velocity, this.target.forward) >= 0.0f) {
	
				target_velocity = Vector3.Lerp(target_velocity.normalized, this.target.forward, rate);

			} else {

				target_velocity = Vector3.Lerp(target_velocity.normalized, -this.target.forward, rate);
			}
		}

		target_velocity.Normalize();

		// 当赛车行驶时，防止摄像机跑到赛车的前方
		//
		if(Vector3.Dot(target_velocity, this.target.forward) < 0.0f) {

			target_velocity = -target_velocity;
		}

		if(Vector3.Angle(target_velocity, this.target.forward) > 90.0f) {

			target_velocity = this.prev_velocity;
		}

		this.current_velocity   = Vector3.Lerp(this.prev_velocity, target_velocity, this.velocityDamping*Time.deltaTime);
		this.current_velocity.y = 0;
		this.prev_velocity = this.current_velocity;

		this.current_velocity = this.current_velocity.normalized;

	}
	
	void 	LateUpdate()
	{
		this.calcPosture();
	}

	// ================================================================ //

	public void	calcPosture()
	{
		float	speed_factor = Mathf.Clamp01(target.root.GetComponent<Rigidbody>().velocity.magnitude/70.0f);

		// 路径
		GetComponent<Camera>().fieldOfView = Mathf.Lerp(55.0f, 72.0f, speed_factor);

		// 视点-注视点的距离
		float		distance = Mathf.Lerp(7.5f, 6.5f, speed_factor);

		Vector3		interest = this.target.position + Vector3.up*height;
		Vector3		eye      = interest - (this.current_velocity*distance);

		eye.y = interest.y + 2.0f;
		
		Vector3		eye_vector_reverse = eye - interest;

		if(Physics.Raycast(interest, eye_vector_reverse, out hit, distance, raycastLayers)) {

			eye = hit.point;
		}

		transform.position = eye;
		transform.LookAt(interest);
	}

	// 重置（赛车 生成后立刻调用）
	public void	reset()
	{
		// 生成赛车后 rigidbody.velocity 值为0,因此使用 rotation 
		// 来求出摄像机的方向
		//
		this.current_velocity   = this.target.TransformDirection(Vector3.forward);
		this.current_velocity.y = 0.0f;

		this.prev_velocity = this.current_velocity;
	}


	public void	setEnable(bool sw)
	{
		this.enabled = sw;
	}
}

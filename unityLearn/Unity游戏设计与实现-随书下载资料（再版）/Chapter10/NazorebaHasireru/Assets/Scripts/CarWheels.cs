using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarWheels {
	
	public class Wheel {
	
		public WheelCollider	collider;							// 碰撞器
	
		public GameObject		tire_model;							// 轮胎的模型
	
		public bool				is_front = false;		// 前轮？
		public bool				is_steer = false;		// 转向轮？
		public bool				is_drive = false;		// 驱动轮？
	
		public WheelFrictionCurve	fc_forward;
		public WheelFrictionCurve	fc_sideways;
	
		public Vector3			wheelVelo   = Vector3.zero;
		public Vector3			groundSpeed = Vector3.zero;
	}
	public List<Wheel>	wheels = new List<Wheel>();

	// ---------------------------------------------------------------- //

	protected CarControl	car;

	public float	side_slip_rate = 0.0f;

	protected const float	TIRE_LOCAL_Y_MAX = 0.0f;		// 轮胎模型的本地Y坐标的最大值（避免从车体飞出）

	// ================================================================ //

	public CarWheels(CarControl car)
	{
		this.car = car;
	}

	// 设置轮胎（全部）
	public void	setupWheels()
	{
		foreach(GameObject wheel in this.car.front_wheels) {

			this.wheels.Add(this.setup_wheel(wheel, true));
		}
		
		foreach(GameObject wheel in this.car.rear_wheels) {

			this.wheels.Add(this.setup_wheel(wheel, false));
		}
		
	}

	public void		execute()
	{
		// 将参数复制到所有轮上
		// （调试时调整用）
		foreach(Wheel wc in this.wheels) {

			JointSpring		suspension_spring = wc.collider.suspensionSpring;
		
			suspension_spring.spring = this.car.suspension_spring;
			suspension_spring.damper = this.car.suspension_damper;

			wc.collider.suspensionSpring = suspension_spring;

			wc.collider.mass             = this.car.wheel_mass;
			wc.collider.wheelDampingRate = this.car.wheel_damping_rate;
		}
	}

	// 将引擎和刹车的力量反映到碰撞器上
	public void		applyEngineAndBrakePower()
	{
		float	engine = this.car.engine_power;

		if(this.car.input.is_reverse) {

			engine *= -1.0f;
		}


		foreach(Wheel wheel in this.wheels) {

			if(wheel.is_drive) {

				wheel.collider.motorTorque = engine;

			} else {

				wheel.collider.motorTorque = 0.0f;
			}

			if(wheel.is_front) {

				wheel.collider.brakeTorque = this.car.hand_brake_power;

			} else {

				wheel.collider.brakeTorque = this.car.brake_power;
			}
		}
	}

	// 转向力反映到碰撞器上
	public void		applySteering()
	{
		float	angle = this.car.input.steer*30.0f;

		foreach(Wheel wheel in this.wheels) {

			if(!wheel.is_steer) {

				continue;
			}

			if(wheel.is_front) {

				wheel.collider.steerAngle =  angle;

			} else {

				wheel.collider.steerAngle = -angle;
			}
		}
	}

	// 控制轮胎的摩擦
	public void		updateFriction()
	{
		this.side_slip_rate = Mathf.Pow(this.car.relative_velocity.x, 2.0f)*0.1f;

		float	extremum_rate  = Mathf.Lerp(1.0f, 0.9f, this.side_slip_rate);
		float	asymptote_rate = Mathf.Lerp(1.0f, 0.9f, this.side_slip_rate);

		foreach(Wheel w in this.wheels) {

			WheelFrictionCurve	fc_sideways = w.fc_sideways;

			if(!w.is_front) {
	
				fc_sideways.extremumValue  *= extremum_rate;
				fc_sideways.asymptoteValue *= asymptote_rate;
			}

			w.collider.sidewaysFriction = fc_sideways;
		}
	}

	// 轮胎模型
	public void		tireModelControl()
	{
		foreach(var wheel in this.wheels) {

			WheelCollider	wc = wheel.collider;

			Vector3		position;
			Quaternion	rotation;

			wc.GetWorldPose(out position, out rotation);

			wheel.tire_model.transform.position = position;
			wheel.tire_model.transform.rotation = rotation;

			// 为了防止轮胎从车体飞出，控制它不超过最上方的值
			Vector3		local_position = wheel.tire_model.transform.localPosition;

			if(local_position.y > TIRE_LOCAL_Y_MAX) {

				local_position.y = TIRE_LOCAL_Y_MAX;
				wheel.tire_model.transform.localPosition = local_position;
			}
		}
	}


	// ---------------------------------------------------------------- //

	// 设置轮胎
	protected Wheel		setup_wheel(GameObject wheel_go, bool is_front)
	{
		WheelCollider	wc = wheel_go.GetComponentInChildren<WheelCollider>();

		// 为了保证轮胎碰撞器和轮胎模型位置保持一致， 设置它们等于悬挂系统的根位置
		wc.transform.position += Vector3.up*(wc.suspensionDistance - wc.suspensionSpring.targetPosition);

		Wheel	wheel = new Wheel(); 

		wheel.collider   = wc;
		wheel.tire_model = wheel_go.transform.FindChild("tire_model").gameObject;

		wheel.is_front =  is_front;
		wheel.is_steer =  is_front;
		wheel.is_drive = !is_front;

		wheel.fc_forward  = wc.forwardFriction;
		wheel.fc_sideways = wc.sidewaysFriction;

		return(wheel);
	}
}
using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private bool	is_landed;				// 已着陆？
	public float	JumpHeight = 4.0f;		// 跳跃的高度

	// ---------------------------------------------------------------- //

	void Start ()
	{
		this.is_landed = false;
	}

	void Update ()
	{
		// 着陆后的处理……
		if(this.is_landed) {

			// 点击鼠标右键……
			if(Input.GetMouseButtonDown(0)) {

				this.is_landed = false;
	
				// 通过跳跃的高度，算出初始速度
				float	y_speed = Mathf.Sqrt(2.0f*Mathf.Abs(Physics.gravity.y)*this.JumpHeight);

				this.GetComponent<Rigidbody>().velocity = Vector3.up*y_speed;
			}
		}
	}

	// 需要注意如果未和参数代表的对象发生碰撞则此函数不会被调用
	void OnCollisionEnter(Collision collision)
	{
		// Debug.Log 的使用方法
		// ど所有的对象作为参 Debug.Log 的参数时都将调用 “ToString()” 方法
		// float 等类型也将执行 ToString() 
		Debug.Log(collision.gameObject.ToString());

		// 如果没有执行这一步，和小球碰撞后 this.is_landed 将变为 true 
		if(collision.gameObject.tag == "Floor") {

			this.is_landed = true;
		}
	}

	// 已着陆？
	public bool	isLanded()
	{
		return(this.is_landed);
	}
}

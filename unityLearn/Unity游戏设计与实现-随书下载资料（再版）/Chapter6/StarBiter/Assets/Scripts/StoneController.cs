using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// StoneController
//  - 控制岩石“M12_asteroid”的运动
//  - 使用方法
//    - 配置添加了这个脚本的岩石对象
//  - 运动规则
//    - 采用随机速度往随机方向旋转
// ---------------------------------------------------------------------------
public class StoneController : MonoBehaviour {
	
	private float rotateSpeed = 0;			// 岩石的旋转速度
	
	void Start () 
	{
	
		// 设置岩石的随机方向
		transform.rotation = new Quaternion(
			Random.Range( 0, 360 ),
			Random.Range( 0, 360 ),
			Random.Range( 0, 360 ),
			Random.Range( 0, 360 ));
		
		// 设置岩石的随机旋转速度
		rotateSpeed = Random.Range( 0.01f, 3f );
		
		// 设置岩石为攻击对象
		SendMessage( "SetIsAttack", true );
	}
	
	void Update ()
	{
	
		// 使岩石旋转
		transform.Rotate( new Vector3( 0, rotateSpeed, 0 ) );
		
	}
}

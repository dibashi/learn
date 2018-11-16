using UnityEngine;
using System.Collections;

// 扩充方法
namespace MathExtension {

	static class Vector {

		public static Vector3	XZ(this Vector3 v, float x, float z)
		{
			return(new Vector3(x, v.y, z));
		}

		// Vector3.Y()
		// 设置Y
		public static Vector3	Y(this Vector3 v, float y)
		{
			return(new Vector3(v.x, y, v.z));
		}

		// Vector3.xz()
		// 通过xz创建Vector2
		public static Vector2	xz(this Vector3 v)
		{
			return(new Vector2(v.x, v.z));
		}

		// Vector3.xy()
		// 通过xy创建Vector2
		public static Vector2	xy(this Vector3 v)
		{
			return(new Vector2(v.x, v.y));
		}

		public static Vector3	to_vector3(this Vector2 v, float z = 0.0f)
		{
			return(new Vector3(v.x, v.y, z));
		}
	};
};

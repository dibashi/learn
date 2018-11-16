using UnityEngine;
using System.Collections;

public class MathUtil  {
	static public Quaternion Slerp(Quaternion q1, Quaternion q2,float t) {
		float d = Quaternion.Dot(q1,q2);
		if (d < 0) {
			q2.x *= -1.0f;
			q2.y *= -1.0f;
			q2.z *= -1.0f;
			q2.w *= -1.0f;
		}
		return Quaternion.Slerp(q1,q2,t);
	}
}

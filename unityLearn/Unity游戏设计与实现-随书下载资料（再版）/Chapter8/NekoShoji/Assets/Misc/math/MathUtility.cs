using UnityEngine;
using System.Collections;


public class MathUtility {
	
	// 计算两点间的距离（仅计算XZ分量）
	public static float		calcDistanceXZ(Vector3 from, Vector3 to)
	{
		Vector3		v = to - from;
		
		v.y = 0.0f;
		
		return(v.magnitude);
	}
	// 计算从from到to的向量的Y轴角度
	public static float		calcDirection(Vector3 from, Vector3 to)
	{
		Vector3		v = to - from;
		
		float	dir  = Mathf.Atan2(v.x, v.z)*Mathf.Rad2Deg;
		
		dir = MathUtility.snormDegree(dir);
		
		return(dir);
	}
	
	// 使degree的值落在 -180.0f ~ 180.0f区间内
	public static float		snormDegree(float degree)
	{
		if(degree > 180.0f) {
			
			degree -= 360.0f;
			
		} else if(degree < -180.0f) {
			
			degree += 360.0f;
		}
		
		return(degree);
	}
	
	// 使degree的值落在 0.0f ~ 360.0f区间内
	public static float		unormDegree(float degree)
	{
		if(degree > 360.0f) {
			
			degree -= 360.0f;
			
		} else if(degree < 0.0f) {
			
			degree += 360.0f;
		}
		
		return(degree);
	}
	
	public static float		remap(float a0, float a1, float x, float b0, float b1)
	{
		return(Mathf.Lerp(b0, b1, Mathf.InverseLerp(a0, a1, x)));
	}

	// 计算两条直线上连接距离最短的点
	public static bool calcBridgeOfTwoLines(out Vector3[] bridge, out float[] param, Vector3[] line0, Vector3[] line1)
	{
		float 		s,t;
		bool		ret;
		Vector3		v0, v1, p0_p1;
		float		a, b, c, d, e;
		
		bridge = new Vector3[2];
		param  = new float[2];
		
		v0    = line0[1] - line0[0];
		v1    = line1[1] - line1[0];
		p0_p1 = line0[0] - line1[0];
		
		a = Vector3.Dot(v0, v0);			// a = |v0|*|v0|
		b = Vector3.Dot(v1, v1);			// b = |v1|*|v1|
		c = Vector3.Dot(v1, v0);			// c = v0・v1
		d = Vector3.Dot(v0, p0_p1);			// d = v0・(p0 - p1)
		e = Vector3.Dot(v1, p0_p1);			// e = v1・(p0 - p1)
		
		
		ret = false;
		
		s = (b*d - c*e)/(c*c - a*b);
		t = (c*d - a*e)/(c*c - a*b);
		
		if(!float.IsInfinity(s) && !float.IsInfinity(t)) {
			
			// Lerp は 0 ～ 1 にクランプされちゃう.			
			//bridge[0] = Vector3.Lerp(line0[0], line0[1], s);
			//bridge[1] = Vector3.Lerp(line1[0], line1[1], t);
			bridge[0] = (1.0f - s)*line0[0] + s*line0[1];
			bridge[1] = (1.0f - t)*line1[0] + t*line1[1];
			
			ret = true;
		}
		
		param[0] = s;
		param[1] = t;
		
		return(ret);
	}
}

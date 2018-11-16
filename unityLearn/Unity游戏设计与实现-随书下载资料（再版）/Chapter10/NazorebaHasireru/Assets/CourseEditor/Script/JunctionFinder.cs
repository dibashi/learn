using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JunctionFinder {

	public struct Junction {

		public int		i0;
		public int		i1;
	};

	public List<Vector3>	positions;

	public Junction	junction;
	public bool		is_finded = false;

	public List<Junction>	junctions = new List<Junction>();

	// ------------------------------------------------------------------------------------------------ //

	public void	create()
	{
	}

	// 查找线的交叉点
	public void	findJunction()
	{
		Vector3	xp;
		float	t0, t1;

		this.junctions.Clear();

		this.is_finded = false;

		for(int i = 0;i < this.positions.Count - 2;i++) {

			// 区间（控制点之间部分）1的线段
			Vector3	st0  = this.positions[i];
			Vector3 dir0 = this.positions[i + 1] - this.positions[i];

			for(int j = i + 2;j < this.positions.Count - 1;j++) {

				// 区间2的线段
				Vector3	st1 = this.positions[j];
				Vector3 dir1 = this.positions[j + 1] - this.positions[j];

				// 探测线段1和线段2是否发生交叉

				if(!this.calcIntersectionVectorAndVector(out xp, out t0, out t1, st0, dir0, st1, dir1)) {

					continue;
				}

				// 交叉的部分在线段范围外（不在起点和终点之间）则无效
				if(t0 < 0.0f || 1.0f < t0) {

					continue;
				}
				if(t1 < 0.0f || 1.0f < t1) {

					continue;
				}

				// 发生交叉了！

				this.junction.i0 = i;
				this.junction.i1 = j;

				this.junctions.Add(this.junction);

				this.is_finded = true;
			}
		}
	}

	// ------------------------------------------------------------------------------------------------ //

	private enum ELEMENT {

		X = 0,
		Y,
		Z
	}

	// 求出线段间的交点
	public bool calcIntersectionVectorAndVector(out Vector3 dst, out float t0, out float t1, Vector3 st0, Vector3 dir0, Vector3 st1,Vector3 dir1)
	{
		Vector3				p0, p1, v0, v1;
		Vector3				v0xv1;
		Vector3				xp0, xp1;
		bool				ret;
		ELEMENT				element;
	
		v0 = dir0;
		v1 = dir1;
		p0 = st0;
		p1 = st1;
	
		v0xv1 = Vector3.Cross(v0, v1);
	
		element = selectMaxAbsoluteElement(v0xv1);

		xp0 = Vector3.zero;
		t0 = 0.0f;
		t1 = 0.0f;

		do {

			ret = false;
	
			switch(element) {

				default:	
				case ELEMENT.X:
				{
					t0 = (p0.y - p1.y)*v1.z - (p0.z - p1.z)*v1.y;
					t0 = -t0/v0xv1.x;
				}
				break;
	
				case ELEMENT.Y:
				{
					t0 = (p0.z - p1.z)*v1.x - (p0.x - p1.x)*v1.z;
					t0 = -t0/v0xv1.y;
				}
				break;
	
				case ELEMENT.Z:
				{
					t0 = (p0.x - p1.x)*v1.y - (p0.y - p1.y)*v1.x;
					t0 = -t0/v0xv1.z;
				}
				break;
			}

			// 如果是nan 则结束
			if(float.IsNaN(t0)) {

				break;
			}
	
			//
	
			xp0 = p0 + v0*t0;
	
			// v0 和 v1接近平行时本来应该出现错误，通过计算可以求出。
			// 因此，和通过 t0/t1 求出的交点进行比较，如果值不一致则认为错误
	
			element = selectMaxAbsoluteElement(v1);

			switch(element) {

				default:	
				case ELEMENT.X:
				{
					t1 = (p0.x + t0*v0.x - p1.x)/v1.x;
				}
				break;
	
				case ELEMENT.Y:
				{
					t1 = (p0.y + t0*v0.y - p1.y)/v1.y;
				}
				break;
	
				case ELEMENT.Z:
				{
					t1 = (p0.z + t0*v0.z - p1.z)/v1.z;
				}
				break;
			}
	
			// 如果是nan 则结束
			if(float.IsNaN(t1)) {

				break;
			}

			xp1 = p1 + v1*t1;
	
			//
	
			float	dist = Vector3.Distance(xp0, xp1);
	
			if(dist > (float)(1.0e-4)) {
	
				break;
			}
	
			//
	
			ret = true;
	
		} while(false);
	
		dst = xp0;

		return(ret);
	}

	private ELEMENT	selectMaxAbsoluteElement(Vector3 v)
	{
		ELEMENT				sel;
	
		if(Mathf.Abs(v.x) > Mathf.Abs(v.y)) {
	
			if(Mathf.Abs(v.z) > Mathf.Abs(v.x)) {
	
				sel = ELEMENT.Z;
	
			} else {
	
				sel = ELEMENT.X;
			}
	
		} else {
	
			if(Mathf.Abs(v.z) > Mathf.Abs(v.y)) {
	
				sel = ELEMENT.Z;
	
			} else {
	
				sel = ELEMENT.Y;
			}
		}
	
		return(sel);
	}


}

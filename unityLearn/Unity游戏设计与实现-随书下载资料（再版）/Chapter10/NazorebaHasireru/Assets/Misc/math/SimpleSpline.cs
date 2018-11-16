﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace SimpleSpline {

	// 控制点
	public class ControlVertex {
	
		public int			index;
		public Vector3		position;
		public Vector3		tangent;
		public float		tension;
	};

	// 跟踪曲线
	public class Tracer {

		public Curve			curve = null;

		public ControlVertex	cv = new ControlVertex();
		public bool				is_ended = false;
		public float			t = -1.0f;
		public float			distance = 0.0f;

		public void		attach(Curve curve)
		{
			this.curve = curve;
			this.restart();
		}
		public void		restart()
		{
			this.is_ended = false;
			this.t        = -1.0f;
			this.distance = 0.0f;
			this.cv       = new ControlVertex();
		}


		public ControlVertex	getCurrent()
		{
			return(this.cv);
		}

		// 指定现在的位置（指定该出到开始点的距离）
		public void		setCurrentByDistance(float distance)
		{
			this.proceedToDistance(distance);
			this.is_ended = false;
		}

		// 从现在位置开始处理（指定参数）
		public void		proceed(float dt)
		{
			if(!this.is_ended) {

				if(this.t < 0.0f) {

					this.t = 0.0f;

				} else {

					this.t += dt;
				}

				if(this.t >= this.curve.getEnd()) {

					this.t = this.curve.getEnd();
					this.is_ended = true;
				}

				this.cv = this.curve.calcVertex(this.t);
			}
		}

		// 从现在位置开始处理（指定到开始地点的距离）
		public void		proceedToDistance(float dist)
		{
			if(this.distance < 0.0f) {

				this.distance = 0.0f;
				this.t        = 0.0f;
			}

			this.proceedByDistance(dist - this.distance);

			this.distance = dist;
		}

		// 从现在位置开始处理（指定到现在位置的距离）
		public void		proceedByDistance(float dist)
		{
			do {

				if(this.is_ended) {

					break;
				}

				ControlVertex	cv0;

				if(this.t < 0.0f) {

					this.t = 0.0f;
				}

				cv0 = this.curve.calcVertex(this.t);
				this.cv = cv0;

				//

				float		dt  = dist/this.curve.calcTotalDistance()*this.curve.getEnd();
				float		sdt = dt >= 0.0f ? 1.0f : -1.0f;
							dt  = Mathf.Abs(dt);
				float		dt0 = dt;
				float		t0  = this.t;

				float		advanced  = 0.0f;
				float		advanced0 = advanced;
				int			i;

				// 最大循环次数
				int			max_times = Mathf.CeilToInt(this.curve.getEnd()/dt);

				max_times = max_times >= 1 ? max_times : 1;

				for(i = 0;i < max_times;i++) {

					t0 = this.t;
					this.t += sdt*dt;

					if(this.t >= this.curve.getEnd() && sdt >= 0.0f) {
	
						this.t = this.curve.getEnd();

						if(dt < dt0) {

							this.is_ended = true;
						}

						dt = Mathf.Max(0.0f, this.t - t0);

					} else if(this.t < 0.0f && sdt < 0.0f) {

						this.t = 0.0f;

						if(dt < dt0) {

							this.is_ended = true;
						}

						dt = Mathf.Max(0.0f, Mathf.Abs(this.t - t0));
					}

					this.cv = this.curve.calcVertex(this.t);

					advanced0 = advanced;
					advanced += Vector3.Distance(cv0.position, this.cv.position);

					if(this.is_ended) {

						break;
					}

					if(advanced >= Mathf.Abs(dist)) {

						// 如果遇到超过的情况，
						// 将前进的距离减半，再从原来的地方重新算过

						dt *= 0.5f;

						if(dt < dt0/100.0f) {

							break;
						}

						this.t = t0;
						advanced = advanced0;

					} else {

						cv0 = this.cv;
					}
				}

			} while(false);
		}

		public bool		isEnded()
		{
			return(this.is_ended);
		}
	};

	// 曲线
	public class Curve {
	
		public List<ControlVertex>	cvs = new List<ControlVertex>();

		public void		appendCV(Vector3 position, Vector3 slope)
		{
			ControlVertex	cv = new ControlVertex();

			cv.index    = this.cvs.Count;
			cv.position = position;
			cv.tension  = slope.magnitude;
			cv.tangent  = slope.normalized;

			this.cvs.Add(cv);
		}

		public float	getEnd()
		{
			float	end = (float)(this.cvs.Count - 1);

			return(end);
		}
	
		// 求出曲线上的点
		public ControlVertex	calcVertex(float t)
		{
			ControlVertex	cv = null;
	
			int		segment_index = Mathf.FloorToInt(t);
	
			if(segment_index < 0) {
	
				cv = this.cvs[0];
	
			} else if(this.cvs.Count - 1 <= segment_index) {
	
				cv = this.cvs[this.cvs.Count - 1];
	
			} else {
	
				float[]		pos_k = new float[4];
				float[]		tan_k  = new float[4];
		
				ControlVertex	cv0 = this.cvs[segment_index];
				ControlVertex	cv1 = this.cvs[segment_index + 1];
		
				float		local_param = t - (float)segment_index;
		
				Curve.calc_konst(pos_k, tan_k, local_param);
		
				cv = Curve.lerp(cv0, cv1, pos_k, tan_k);
			}
	
			return(cv);
		}

		// 计算曲线的总长度
		public float	calcTotalDistance()
		{
			float		distance = 0.0f;
	
			ControlVertex	cv_prev = this.calcVertex(0.0f);
	
			float	t     = 0.0f;
			float	dt    = 0.1f;
			float	t_max = (float)(this.cvs.Count - 1);
	
			int		safe_count = Mathf.CeilToInt(t_max/dt) + 1;
	
			for(int i = 0;i < safe_count;i++) {
	
				ControlVertex	cv_current = this.calcVertex(t);
	
				distance += Vector3.Distance(cv_prev.position, cv_current.position);
	
				if(t >= t_max) {
	
					break;
				}
	
				t += dt;
	
				t = Mathf.Min(t, t_max);

				cv_prev = cv_current;
			}
	
			return(distance);
		}
	
		// ================================================================ //
	
		// 对区间进行补间
		public  static ControlVertex	lerp(ControlVertex cv0, ControlVertex cv1, float[] pos_k, float[] tan_k)
		{
			ControlVertex	dest = new ControlVertex();
	
			dest.position = cv0.position*pos_k[0] + cv1.position*pos_k[1] + cv0.tangent*cv0.tension*pos_k[2] + cv1.tangent*cv1.tension*pos_k[3];
			dest.tangent  = cv0.position*tan_k[0] + cv1.position*tan_k[1] + cv0.tangent*cv0.tension*tan_k[2] + cv1.tangent*cv1.tension*tan_k[3];
	
			return(dest);
		}
	
		// 求出样条曲线的系数
		public static void calc_konst(float[] dest_pos_k, float[] dest_tan_k, float t)
		{
			dest_pos_k[0] =  2.0f*t*t*t - 3.0f*t*t     + 1.0f;
			dest_pos_k[1] = -2.0f*t*t*t + 3.0f*t*t;
			dest_pos_k[2] =       t*t*t - 2.0f*t*t + t;
			dest_pos_k[3] =       t*t*t -      t*t;
		
			dest_tan_k[0] =  6.0f*t*t - 6.0f*t;
			dest_tan_k[1] = -6.0f*t*t + 6.0f*t;
			dest_tan_k[2] =  3.0f*t*t - 4.0f*t + 1.0f;
			dest_tan_k[3] =  3.0f*t*t - 2.0f*t;
		}
	};

}; // namespace SimpleSpline 

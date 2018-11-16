using UnityEngine;
using System.Collections;


public class ipCell {

#if true
	protected	float	min;
	protected	float	max;
	protected	float	current;

	private  ipCell() {}

	private void	init()
	{
		this.min = 0.0f;
		this.max = 1.0f;
		this.current = 0.0f;
	}


	public ipCell	setInput(float current)
	{
		this.current = current;

		return(this);
	}

	public float	getCurrent()
	{
		return(this.current);
	}

	// 返回０~ max 的值（超出范围后则会重新来过）
	public ipCell	repeat(float max)
	{
		this.current = Mathf.Repeat(this.current, max);

		this.min = 0.0f;
		this.max = max;

		return(this);
	}

	// 返回min ~ max 之间的值
	public ipCell	clamp(float min, float max)
	{
		this.current = Mathf.Clamp(this.current, min, max);

		this.min = min;
		this.max = max;

		return(this);
	}

	// 返回０~1.0 之间的值
	public ipCell	normalize()
	{
		this.current = Mathf.InverseLerp(this.min, this.max, this.current);

		this.min = 0.0f;
		this.max = 1.0f;

		return(this);
	}

	// 补间函数
	public ipCell	lerp(float min, float max)
	{
		this.current = Mathf.Lerp(min, max, this.current);

		this.min = min;
		this.max = max;

		return(this);
	}

	// 反向补间
	public ipCell	ilerp(float min, float max)
	{
		this.current = Mathf.InverseLerp(min, max, this.current);

		this.min = 0.0f;
		this.max = 1.0f;

		return(this);
	}

	// 幂乘
	public ipCell	pow(float x)
	{
		this.current = Mathf.Pow(this.current, x);
		this.min = Mathf.Pow(this.min, x);
		this.max = Mathf.Pow(this.max, x);

		return(this);
	}

	// 在不改变current的位置（比率）的前提下，改变范围
	public ipCell	remap(float min, float max)
	{
		this.normalize();
		this.lerp(min, max);

		return(this);
	}

	// 返回０~ pi之间的值
	public ipCell	uradian()
	{
		this.lerp(0.0f, Mathf.PI);

		return(this);
	}

	// 正弦函数
	public ipCell	sin()
	{
		this.current = Mathf.Sin(this.current);

		this.max =  1.0f;
		this.min = -1.0f;

		return(this);
	}

	// 乘法运算
	public ipCell	scale(float s)
	{
		this.current *= s;
		this.max     *= s;
		this.min     *= s;

		return(this);
	}

	// 按quant的整数倍量化
	public ipCell	quantize(float quant)
	{
		this.current = Mathf.Floor(this.current/quant)*quant;

		return(this);
	}

	// ================================================================ //

	private	static ipCell	instance = null;

	public static ipCell	get()
	{
		if(ipCell.instance == null) {

			ipCell.instance = new ipCell();
		}

		return(ipCell.instance);
	}

#else
	public:

		 Atom() {}
		~Atom() {}

	public:
		ipModule&	clamp(float min, float max)
		{
			this->min = min;
			this->max = max;

			this->current = MathPrimary::minmax(this->min, this->current, this->max);

			return(*this);
		}
		ipModule&	delay(float delay)
		{
			this->current = std::max<float>(this->min, this->current - delay);

			return(*this);
		}

		ipModule&	remap(float new_min, float new_max)
		{
			// min = 0.0f, max = 1.0f, current = 0.5 ÌÆ«A
			//
			// remap(0.0f, 2.0f) Æ·éÆ
			//
			// min = 0.0f, max = 2.0f, current = 1.0 ÉÈèÜ·B

			this->current = MathPrimary::rate(this->min, this->max, this->current);

			this->max = new_max;
			this->min = new_min;

			this->current = MathPrimary::lerp(this->min, this->max, this->current);

			return(*this);
		}
		ipModule&	sin90(void)
		{
			this->remap(0.0f, 90.0f);

			this->current = MathPrimary::sinfDegree(this->current);

			this->max =  1.0f;
			this->min =  0.0f;

			return(*this);
		}
		ipModule&	square(void)
		{
			this->current = MathPrimary::square(this->current);

			return(*this);
		}
		ipModule&	sqrt(void)
		{
			this->current = sqrtf(this->current);

			return(*this);
		}
#endif
}


using UnityEngine;
using System.Collections;

[System.Serializable]
public class GenerateParameter 
{
    public RectXZ	posXZ = new RectXZ(0.0f, 0.0f, 1800.0f, 1800.0f);

    public bool		fill = false;   	// true : posXZ内全部作为对象范围.
                                		// flase: posXZ的外围一周作为对象范围
    public int		limitNum  = 1;		// 允许存在的数量
    public float	delayTime = 1.0f;	// 生成前的延迟
    public bool		endless   = true;	// 限制数量减少时是否自动添加

}

[System.Serializable]
public class RectXZ {

	public float	x, z;
	public float	width;
	public float	depth;

	public RectXZ(float x, float z, float width, float depth)
	{
		this.x = x;
		this.z = z;
		this.width = width;
		this.depth = depth;
	}

	public float getXMin()
	{
		return(this.x - this.width/2.0f);
	}
	public float getXMax()
	{
		return(this.x + this.width/2.0f);
	}
	public float getZMin()
	{
		return(this.z - this.depth/2.0f);
	}
	public float getZMax()
	{
		return(this.z + this.depth/2.0f);
	}
}

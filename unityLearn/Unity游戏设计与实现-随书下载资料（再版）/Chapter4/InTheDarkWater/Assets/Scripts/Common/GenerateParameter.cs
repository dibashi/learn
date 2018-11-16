using UnityEngine;

/// <summary>
/// 生成时的参数
/// </summary>
[System.Serializable]
public class GenerateParameter 
{
    public Rect		posXZ = new Rect(-900.0f, -900.0f, 1800.0f, 1800.0f);
    public bool		fill = false;		// true: posXZ内全部作为对象范围
										// flase: posXZ的外围一周作为对象范围
    public int		limitNum = 1;		// 允许存在的数量
    public float	delayTime = 1.0f;	// 生成前的延迟
    public bool		endless = true;		// 限制数量减少时是否自动添加

}

using UnityEngine;
using System.Collections;

// 对鼠标光标位置做平滑处理
public class MousePositionSmoother {

	// 历史数量
	private static int	HISTORY_NUM = 5;

	// 当前记录的历史数
	private int			current_num = 0;

	// 历史（位置的过往记录）
	private Vector3[]	history;

	// ------------------------------------------------------------------------ //

	public void	create()
	{
		this.history = new Vector3[HISTORY_NUM];
	}

	// 添加鼠标得位置（返回平滑处理后的位置）
	public Vector3	append(Vector3 current_position)
	{
		// 将最新的位置追加到历史中

		this.history[this.current_num%HISTORY_NUM] = current_position;

		this.current_num++;

		// 将过去的几帧位置进行平均化，平滑处理

		int	smooth_num = Mathf.Min(HISTORY_NUM, this.current_num);

		Vector3	smooth_position = Vector3.zero;

		for(int i = 0;i < smooth_num;i++) {

			smooth_position += this.history[i];
		}

		smooth_position /= (float)smooth_num;

		return(smooth_position);
	}

	// 重置（清空历史）
	public void	reset()
	{
		this.current_num = 0;
	}
}

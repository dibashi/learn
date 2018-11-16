using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LineDrawerControl : MonoBehaviour {

	enum STEP {

		NONE = -1,

		IDLE = 0,		// 待机
		DRAWING,		// 描绘直线（拖动过程中）
		DRAWED,			// 结束描绘

		NUM,
	};
	
	private STEP	step      = STEP.NONE;
	private STEP	next_step = STEP.NONE;

	public List<Vector3>	positions;

	public ToolControl	root = null;

	private MousePositionSmoother	smoother;

	private Vector3		previous_mouse_position;		// 鼠标的上一个位置
	private bool		is_play_drawing_sound;			// 画线时是否播放音效？
	private float		sound_to_stop_timer = 0.0f;		// 用于在画线时音效停止判断的计时器


	// ------------------------------------------------------------------------ //

	void	Start()	
	{
		this.positions = new List<Vector3>();

		this.smoother = new MousePositionSmoother();
		this.smoother.create();

		this.previous_mouse_position = Vector3.zero;
		this.is_play_drawing_sound = false;
	}

	void	Update()
	{
		// 状态迁移检测
		if(this.next_step == STEP.NONE) {
	
			switch(this.step) {
	
				case STEP.NONE:
				{
					this.next_step = STEP.IDLE;
				}
				break;
	
				case STEP.IDLE:
				{
					if(Input.GetMouseButton(0)) {
	
						this.next_step = STEP.DRAWING;
					}
				}
				break;
	
				case STEP.DRAWING:
				{
					if(!Input.GetMouseButton(0)) {
	
						if(this.positions.Count >= 2) {
	
							this.next_step = STEP.DRAWED;
	
						} else {
	
							this.next_step = STEP.IDLE;
						}

						this.GetComponent<AudioSource>().Stop();
						this.is_play_drawing_sound = false;
					}
				}
				break;
			}
		}

		// 状态迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.IDLE:
				{
					// 删除上次生成的对象

					this.positions.Clear();

					this.update_line_renderer();

					this.smoother.reset();
				}
				break;

				case STEP.DRAWING:
				{
					this.smoother.reset();

					this.previous_mouse_position = Input.mousePosition;
					this.is_play_drawing_sound = false;
				}
				break;
			}

			this.step = this.next_step;

			this.next_step = STEP.NONE;
		}

		// 各个状态的处理

		switch(this.step) {

			case STEP.DRAWING:
			{
				this.execute_step_drawing();
			}
			break;

			case STEP.DRAWED:
			{
				for(int i = 0;i < this.positions.Count - 1;i++) {

					Debug.DrawLine(this.positions[i], this.positions[i + 1], Color.red, 0.0f, false);
				}
			}
			break;
		}
	}
	
	private void	execute_step_drawing()
	{
		Vector3	mouse_position = Input.mousePosition;

		// 对光标位置做平滑处理
		mouse_position = this.smoother.append(mouse_position);

		Vector3		position;

		// 对鼠标光标位置进行逆透视变换
		if(this.root.unproject_mouse_position(out position, mouse_position)) {

			this.execute_step_drawing_sub(mouse_position, position);
		}
	}

	private void	execute_step_drawing_sub(Vector3 mouse_position, Vector3 position_3d)
	{
		// 顶点的间隔（＝道路多边形纵向长度）
		float	append_distance = RoadCreator.PolygonSize.z;

		int	append_num = 0;

		while(true) {

			bool	is_append_position;

			// 往线条上追加顶点，执行检测

			is_append_position = false;

			if(this.positions.Count == 0) {

				// 第一个无条件追加

				is_append_position = true;

			} else {

				// 添加和上次被添加的顶点距离一定间隔的点
				float	l = Vector3.Distance(this.positions[this.positions.Count - 1], position_3d);

				if(l > append_distance) {

					is_append_position = true;
				}
			}

			if(!is_append_position) {

				break;
			}

			//

			if(this.positions.Count == 0) {

				this.positions.Add(position_3d);

			} else {

				// ・在连接“上一次追加的顶点”和“鼠标光标位置”的直线上
				// ・追加和“上一次追加的顶点”距离为“append_distance”的顶点

				Vector3	distance = position_3d - this.positions[this.positions.Count - 1];

				distance *= append_distance/distance.magnitude;

				this.positions.Add(this.positions[this.positions.Count - 1] + distance);
			}

			//

			append_num++;
		}

		if(append_num > 0) {

			// 重新生成LineRender
			this.update_line_renderer();
		}

		// 控制画线时的SE

		this.drawing_sound_control(mouse_position);
	}

	// 删除直线
	public void		clear()
	{
		this.next_step = STEP.IDLE;

		this.Update();
	}

	// 是否画线？
	public bool		isLineDrawed()
	{
		bool	is_drawed = (this.step == STEP.DRAWED);

		return(is_drawed);
	}

	// 显示／不显示
	public void		setVisible(bool visible)
	{
		this.set_line_render_visible(visible);
	}

	// 读取文件
	public void		loadFromFile(BinaryReader Reader)
	{
		this.positions.Clear();

       	int		count = Reader.ReadInt32();
		
		for(int i = 0;i < count;i++) {

			Vector3		p = Vector3.zero;

			p.x = (float)Reader.ReadDouble();
			p.y = (float)Reader.ReadDouble();
			p.z = (float)Reader.ReadDouble();

			this.positions.Add(p);
		}

		// 重新生成LineRender
		this.update_line_renderer();

		this.next_step = STEP.DRAWED;

		this.Update();
	}

	public void		saveToFile(BinaryWriter Writer)
	{
       	Writer.Write((int)this.positions.Count);

		for(int i = 0;i < this.positions.Count;i++) {

			Writer.Write((double)this.positions[i].x);
			Writer.Write((double)this.positions[i].y);
			Writer.Write((double)this.positions[i].z);
		}
	}

	// ---------------------------------------------------------------- //

	// 显示／不显示 直线
	private void	set_line_render_visible(bool visible)
	{
		this.GetComponent<LineRenderer>().enabled = visible;
	}

	// 重新生成LineRender
	private void	update_line_renderer()
	{
		this.GetComponent<LineRenderer>().SetVertexCount(this.positions.Count);

		for(int i = 0;i < this.positions.Count;i++) {

			this.GetComponent<LineRenderer>().SetPosition(i, this.positions[i]);
		}
	}
	

	private float	DRAW_SE_VOLUME_MIN = 0.3f;
	private float	DRAW_SE_VOLUME_MAX = 1.0f;

	private float	DRAW_SE_PITCH_MIN = 0.75f;
	private float	DRAW_SE_PITCH_MAX = 1.5f;

	// 控制画线时的SE
	private void	drawing_sound_control(Vector3 mouse_position)
	{
		float	distance = Vector3.Distance(mouse_position, this.previous_mouse_position)/Time.deltaTime;

		// 鼠标静止时间超过这个值，则停止画线的SE
		// 如果不这样处理将会出现杂音
		const float		stop_time = 0.3f;

		if(this.is_play_drawing_sound) {

			if(distance < 60.0f) {

				// 鼠标的移动量变小

				this.sound_to_stop_timer += Time.deltaTime;

				if(this.sound_to_stop_timer > stop_time) {

					this.GetComponent<AudioSource>().Stop();
					this.is_play_drawing_sound = false;
					this.sound_to_stop_timer = 0.0f;
				}

			} else {

				this.sound_to_stop_timer = 0.0f;

			}

		} else {

			if(distance >= 60.0f) {

				this.GetComponent<AudioSource>().Play();
				this.is_play_drawing_sound = true;
				this.sound_to_stop_timer = 0.0f;
			}
		}

		// 通过画线的速度改变音调和音量

		if(this.is_play_drawing_sound) {

			float	speed_rate;

			speed_rate = Mathf.InverseLerp(60.0f, 500.0f, distance);

			speed_rate = Mathf.Clamp01(speed_rate);

			speed_rate = Mathf.Round(speed_rate*3.0f)/3.0f;

			// 音量

			float		next_volume = Mathf.Lerp(DRAW_SE_VOLUME_MIN, DRAW_SE_VOLUME_MAX, speed_rate);
			float		current_volume = this.GetComponent<AudioSource>().volume;

			float		diff = next_volume - current_volume;

			if(diff > 0.1f) {

				diff = 0.1f;

			} else if(diff < -0.05f) {

				diff = -0.05f;
			}

			next_volume = current_volume + diff;

			this.GetComponent<AudioSource>().volume = next_volume;

			// 音调

			float		next_pitch = Mathf.Lerp(DRAW_SE_PITCH_MIN, DRAW_SE_PITCH_MAX, speed_rate);
			float		current_pitch = this.GetComponent<AudioSource>().pitch;

			float		pitch_diff = next_pitch - current_pitch;

			if(pitch_diff > 0.1f) {

				pitch_diff = 0.1f;

			} else if(pitch_diff < -0.1f) {

				pitch_diff = -0.1f;
			}

			next_pitch = current_pitch + pitch_diff;

			this.GetComponent<AudioSource>().pitch = next_pitch;

		}

		this.previous_mouse_position = mouse_position;
	}

	// ---------------------------------------------------------------- //

}
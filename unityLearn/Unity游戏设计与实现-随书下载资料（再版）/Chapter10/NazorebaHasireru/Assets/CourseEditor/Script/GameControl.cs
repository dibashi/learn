using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

	public ToolControl		tool_control = null;
	public AudioClip		goalAudioClip = null;

	private RaycastHit	hit;
	private bool		is_hitted = false;
	private int			current_block_index = 0;
	

	private int			goal_poly_index = -1;
	private int			current_poly_index = -1;

	private bool		is_goaled = false;				//!< 是否得分？

	private AudioSource	goal_audio = null;

	// ------------------------------------------------------------------------ //

	// Use this for initialization
	void Start () {

		this.goal_audio = this.gameObject.AddComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
	
		RoadCreator	road_creator = this.tool_control.road_creator;

		// -------------------------------------------------------------------------------------------- //
		// 查找当前赛车位于哪个方块上
		// （方块＝将跑道按照前进方向进行分割后的产物）

		GameObject	car_object = this.tool_control.car_object;

		if(car_object != null) {


			// 检测赛车所在的方块

			// 向赛车的底部发射Ray，和道路发生碰撞

			Vector3		start = car_object.transform.position;
			Vector3		end   = start + Vector3.down*10.0f;

			is_hitted = Physics.Linecast(start, end, out hit, (1 << LayerMask.NameToLayer("Road Coli")));

			if(is_hitted) {

				this.current_block_index = road_creator.getRoadBlockIndexByName(hit.collider.name);

				if(this.current_block_index != -1) {

					// 只有针对赛车所在的前后方块碰撞才会有效
					//
					for(int i = 0;i < road_creator.road_mesh.Length;i++) {
	
						if(this.current_block_index - 1 <= i && i <= this.current_block_index + 1) {
	
							road_creator.setEnableToBlock(i, true);
	
						} else {
	
							road_creator.setEnableToBlock(i, false);
						}
					}

					//

					current_poly_index = hit.triangleIndex/2;
				}
			}
			
			// 是否得分？判断
			if(!this.is_goaled) {

				do {

					if(road_creator.road_mesh == null) {

						break;
					}

					if(this.current_block_index < road_creator.road_mesh.Length - 1) {

						break;
					}


					if(this.current_poly_index < this.goal_poly_index - 1) {

						break;
					}


					this.is_goaled = true;
					this.goal_audio.PlayOneShot(this.goalAudioClip);

				} while(false);
			}
		}
	}
#if false
	void	OnGUI()
	{

		RoadCreator	road_creator = this.tool_control.road_creator;

		if(road_creator.road_mesh != null) {

			GUI.Label(new Rect(100, 100, 100, 20), this.current_block_index.ToString() + " " + road_creator.road_mesh.Length);

			GUI.Label(new Rect(100, 120, 100, 20), this.current_poly_index + "/" + this.goal_poly_index.ToString());
		}
	}
#endif

	// 开始测试运行
	public void	startTestRun()
	{
		// 播放环境音效
		this.GetComponent<AudioSource>().Play();

		RoadCreator	road_creator = this.tool_control.road_creator;

		if(road_creator.split_points.Length > 2) {

			int		s = road_creator.split_points[road_creator.split_points.Length - 1 - 1];
			int		e = road_creator.split_points[road_creator.split_points.Length - 1];

			this.goal_poly_index = e - s;
		}
	}

	// 结束运行测试
	public void stopTestRun()
	{
		// 停止播放环境音效
		this.GetComponent<AudioSource>().Stop();
	}

	// 清空生成物时被调用
	public void	onClearOutput()
	{
		this.is_goaled = false;
	}
}

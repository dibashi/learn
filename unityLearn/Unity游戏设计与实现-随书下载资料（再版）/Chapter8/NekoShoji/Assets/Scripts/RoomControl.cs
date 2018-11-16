using UnityEngine;
using System.Collections;

public class RoomControl : MonoBehaviour {

	public GameObject	roomPrefab = null;
	public GameObject	shojiPrefab = null;

	private FloorControl[]	rooms;

	// 摄像机
	private GameObject main_camera = null;

	public static float	MODEL_LENGTH   = 15.0f;
	public static float	MODEL_Z_OFFSET = 0.0f;

	public static float	RESTART_Z_OFFSET = 5.0f;		//重新开始时的位置偏移

	public static int	MODEL_NUM = 3;

	private int		start_model_index = 0;				// 最靠前的模型的索引

	private LevelControl	level_control;
	private	ShojiControl	shoji_control;				// 窗户（循环使用一个）
	private	SceneControl	scene_control;

	private int		room_count = 0;						// 进入的房间数量
	private	bool	is_closed = false;					// 窗户是否关闭？（每次进入下一间屋子时重置）

	// ---------------------------------------------------------------- //
	
	// Sound
	public AudioClip CLOSE_SOUND = null;
	public AudioClip CLOSE_END_SOUND = null;
	
	// ================================================================ //
	// 继承于MonoBehaviour 

	void 	Start()
	{
		this.rooms = new FloorControl[MODEL_NUM];

		for(int i = 0;i < 3;i++) {
	
			this.rooms[i] = (Instantiate(this.roomPrefab) as GameObject).GetComponent<FloorControl>();

			this.rooms[i].transform.position = new Vector3(0.0f, 0.0f, MODEL_Z_OFFSET + (float)i*MODEL_LENGTH);
		}

		this.start_model_index = 0;

		this.rooms[(this.start_model_index + 0)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 1)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 2)%MODEL_NUM].setClose();

		this.shoji_control = (Instantiate(this.shojiPrefab) as GameObject).GetComponent<ShojiControl>();

		this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);

		//

		// 查找摄像机的实例
		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.scene_control = SceneControl.get();

		this.level_control = LevelControl.get();
	}

	void 	Update()
	{
		FloorControl	room = this.rooms[this.start_model_index];

		// 最后面的模型到达摄像机后面时，使其往里移动
		//
		if(room.transform.position.z + MODEL_LENGTH < this.main_camera.transform.position.z) {

			// 使最后的模型往里移动

			Vector3		new_position = room.transform.position;

			new_position.z += MODEL_LENGTH*MODEL_NUM;

			room.transform.position = new_position;

			//

			this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(null);

			// 增加“位于最前方的模型的索引”
			//
			this.start_model_index = (this.start_model_index + 1)%MODEL_NUM;


			// 最前面的房间　→　将窗户附加在其上，设置为打开的状态

			if(this.scene_control.step.get_current() == SceneControl.STEP.GAME) {

				this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);
				this.rooms[(this.start_model_index + 0)%MODEL_NUM].setOpen();
				
			} else {

				this.shoji_control.gameObject.SetActive(false);
			}

			// 第二个房间　→　开始打开拉门

			this.rooms[(this.start_model_index + 1)%MODEL_NUM].beginOpen();

			// 第三个房间　→　拉门的状态为关闭

			this.rooms[(this.start_model_index + 2)%MODEL_NUM].setClose();

			// 将多次穿过的格子眼设置为铁板

			foreach(var paper_control in this.shoji_control.papers) {

				if(this.level_control.getChangeSteelCount() > 0) {

					if(paper_control.through_count >= this.level_control.getChangeSteelCount()) {

						paper_control.beginSteel();
					}
				}
			}

			//

			this.room_count++;
			this.is_closed = false;
		}

		// 摄像机靠近后，关闭
		//
		// 虽然事实上应该看到玩家，但由于重复处理房间模型时看到摄像机的关系，这里设置为摄像机

		float	close_distance = MODEL_LENGTH - this.level_control.getCloseDistance();

		if(room.transform.position.z + close_distance < this.main_camera.transform.position.z) {

			do {

				if(this.is_closed) {

					break;
				}

				// 开始后设置为不关闭
				if(this.room_count < 1) {

					break;
				}

				// 关闭窗户
	
				if(this.scene_control.step.get_current() == SceneControl.STEP.GAME) {
	
					FloorControl.CLOSING_PATTERN_TYPE	type;
					bool								is_flip;
					FloorControl.ClosingPatternParam	param;

					this.level_control.getClosingPattern(out type, out is_flip, out param);

					this.rooms[(this.start_model_index + 0)%MODEL_NUM].setClosingPatternType(type, is_flip, param);
					this.rooms[(this.start_model_index + 0)%MODEL_NUM].beginCloseShoji();
				}

				this.is_closed = true;

			} while(false);
		}

	#if false
		// 调试功能
		// 通过全键盘的数字键，查看窗户出现的模式

		for(int i = (int)KeyCode.Alpha1;i <= (int)KeyCode.Alpha9;i++) {

			if(Input.GetKeyDown((KeyCode)i)) {

				FloorControl.CLOSING_PATTERN_TYPE	type = (FloorControl.CLOSING_PATTERN_TYPE)(i - (int)KeyCode.Alpha1);

				bool	is_flip = Input.GetKey(KeyCode.RightShift);

				this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);
				this.rooms[(this.start_model_index + 0)%MODEL_NUM].setClosingPatternType(type, is_flip);
				this.rooms[(this.start_model_index + 0)%MODEL_NUM].beginCloseShoji();
			}
		}
	#endif
	}

	// ================================================================ //

	public void	onRestart()
	{
		this.room_count = 0;
		this.is_closed = false;

		this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);
		this.rooms[(this.start_model_index + 0)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 1)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 2)%MODEL_NUM].setClose();
	}

	// 失败后获取其重新开始的位置
	public Vector3	getRestartPosition()
	{
		Vector3	position;

		position = this.rooms[this.start_model_index].transform.position;

		position.z += RESTART_Z_OFFSET;

		return(position);
	}

	// 获取剩余的纸张数量
	public int	getPaperNum()
	{
		return(this.shoji_control.getPaperNum());
	}

	// ================================================================ //
	//																	//
	// ================================================================ //

	protected static	RoomControl instance = null;

	public static RoomControl	get()
	{
		if(RoomControl.instance == null) {

			GameObject		go = GameObject.Find("RoomControl");

			if(go != null) {

				RoomControl.instance = go.GetComponent<RoomControl>();

			} else {

				Debug.LogError("Can't find game object \"RoomControl\".");
			}
		}

		return(RoomControl.instance);
	}
}

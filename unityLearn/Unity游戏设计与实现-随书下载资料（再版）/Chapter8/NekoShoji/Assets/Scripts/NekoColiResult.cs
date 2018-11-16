using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NekoColiResult  {

	public NekoControl	neko = null;

	public static float	THROUGH_GAP_LIMIT = 0.4f;			//!< 从格子的中心距离偏移超过该值，则认定为窗框
	public static float UNLOCK_DISTANCE = 3.0f;				//!< 超过此距离将解除引导

	// ---------------------------------------------------------------- //

	// 描述窗户边框的数据结构
	//
	public struct ShojiHitInfo {

		public bool			is_enable;

		public ShojiControl	shoji_control;

		public ShojiControl.HoleIndex	hole_index;
	};

	public ShojiHitInfo		shoji_hit_info;
	public ShojiHitInfo		shoji_hit_info_first;

	// 描述窗户格子眼的数据结构
	//
	public struct HoleHitInfo {

		public SyoujiPaperControl	paper_control;
	};

	public List<HoleHitInfo>	hole_hit_infos;

	// 描述拉门，窗户边框的数据结构
	//
	public struct ObstacleHitInfo {

		public bool			is_enable;

		public GameObject	go;
		public bool			is_steel;
	};

	public ObstacleHitInfo	obstacle_hit_info;

	// 施加引导时的窗户格子
	//
	public struct LockTarget {

		public bool						enable;

		public ShojiControl.HoleIndex	hole_index;

		public Vector3					position;
	};

	public LockTarget	lock_target;

	public bool			is_steel = false;

	// ---------------------------------------------------------------- //

	public void create()
	{
		this.hole_hit_infos = new List<HoleHitInfo>(4);

		this.shoji_hit_info.is_enable       = false;
		this.shoji_hit_info_first.is_enable = false;

		this.obstacle_hit_info.is_enable = false;

		this.lock_target.enable = false;
	}

	// 检测前一帧的碰撞结果

	public void	resolveCollision()
	{
		// 穿过“格子眼”再前进一定距离后，解除引导
		if(this.lock_target.enable) {

			if(this.neko.transform.position.z > this.lock_target.position.z + UNLOCK_DISTANCE) {

				this.lock_target.enable = false;
			}
		}

		if(!this.lock_target.enable) {

			this.resolve_collision_sub();
		}
	}

	private void	resolve_collision_sub()
	{
		bool	is_collied_obstacle = false;

		this.is_steel = false;

		// 一开始检测是否和拉门／铁板发生了碰撞
		//
		// 如何和拉门／铁板发生碰撞仍可从窗户穿过，不认定为失败

		if(this.obstacle_hit_info.is_enable) {

			is_collied_obstacle = true;

			this.is_steel = this.obstacle_hit_info.is_steel;
		}

		//

		if(this.shoji_hit_info.is_enable) {

			// 是否撞到窗框？

			ShojiControl			shoji_control = this.shoji_hit_info.shoji_control;
			ShojiControl.HoleIndex	hole_index    = this.shoji_hit_info.hole_index;

			if(shoji_control.isValidHoleIndex(hole_index)) {

				SyoujiPaperControl		paper_control = shoji_control.papers[hole_index.x, hole_index.y];

				if(paper_control.isSteel()) {

					// 格子眼中是铁板

					is_collied_obstacle = true;

					this.is_steel = true;

				} else  {

					// 格子眼中是“纸”和“撞破的纸”

					// 回到“格子眼”时目标的位置
					Vector3	position = NekoColiResult.get_hole_homing_position(shoji_control, hole_index);

					//
	
					Vector3	diff = this.neko.transform.position - position;
	
					if(Mathf.Abs(diff.x) < THROUGH_GAP_LIMIT && Mathf.Abs(diff.y) < THROUGH_GAP_LIMIT) {
	
						// 从距离格子眼中心近到一定程度位置处穿过，认定为从格子眼中穿过
						// （复位）

						is_collied_obstacle = false;

						this.lock_target.enable     = true;
						this.lock_target.hole_index = hole_index;
						this.lock_target.position   = position;


						// 向“格子眼”模型通知玩家碰撞事件
						paper_control.onPlayerCollided();

					} else {

						// 距离格子的中心较远的情况下，认为和格子发生了碰撞

						is_collied_obstacle = true;
					}
				}

			} else {

				// 和窗户的格子眼之外区域碰撞

				is_collied_obstacle = true;
			}

		} else {

			// 未和窗框发生碰撞时，不可能出现同时和两个以上“格子眼”碰撞的情况
			// （因为和两个“格子眼”碰撞时，会和这两者中间的窗框碰撞）
			// 所以，未和窗框发生碰撞时，只需要探测 this.hole_hit_infos[0] 就够了
			if(this.hole_hit_infos.Count > 0) {

				// 只和“格子眼”发生了碰撞

				HoleHitInfo			hole_hit_info = this.hole_hit_infos[0];
				SyoujiPaperControl	paper_control = hole_hit_info.paper_control;
				ShojiControl		shoji_control = paper_control.shoji_control;

				paper_control.onPlayerCollided();

				// 穿过“格子眼”
				// （复位）

				// 锁定（设置引导的目标位置）

				// 算出“格子眼”的中心

				ShojiControl.HoleIndex	hole_index = paper_control.hole_index;

				Vector3	position = NekoColiResult.get_hole_homing_position(shoji_control, hole_index);

				this.lock_target.enable = true;
				this.lock_target.hole_index = hole_index;
				this.lock_target.position   = position;
			}
		}


		if(is_collied_obstacle) {

			// 和障碍物（窗户边框，拉门）发生了碰撞

			if(this.neko.step != NekoControl.STEP.MISS) {

				this.neko.beginMissAction(this.is_steel);
			}
		}

	}


	// 回到“格子眼”时的目标位置
	private static Vector3	get_hole_homing_position(ShojiControl shoji_control, ShojiControl.HoleIndex hole_index)
	{
		Vector3		position;
	
		position = shoji_control.getHoleWorldPosition(hole_index.x, hole_index.y);
	
		// 碰撞的中心到对象原点的偏移
		// 让碰撞的中心穿过“格子眼”的中心
		position += -NekoControl.COLLISION_OFFSET;

		return(position);
	}

}

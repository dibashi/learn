using UnityEngine;
using System.Collections;

public class EnemyStatusBoss : EnemyStatus {
	
	public bool isBoss = false;							// 该对象是BOSS（=true）
	public int bossGuardLimit = 0;						// 如果Life值大于该数则对攻击进行防御
	private bool canGuard = false;						// 允许防御攻击
	
	private GameObject boss;							// Boss的实例
	
	public override void StartSub()
	{
		// BOSS用
		if ( isBoss )
		{
			// 跟随BOSS
			SetIsFollowingLeader( true );
			
			// 获取BOSS的实例
			boss = GameObject.FindGameObjectWithTag("Boss");
			
			// 确认是否允许防御
			if ( CanGuard() )
			{
				canGuard = true;
			}
		}
	}
	
	public override void UpdateSub()
	{
		// BOSS用.
		if ( isBoss && canGuard )
		{
			if ( !CanGuard() )
			{
				canGuard = false;
				
				// 使碰撞有效
				EnableCollider();
			}
		}
	}
	
	public override void DestroyEnemySub()
	{
		// BOSS时的情况
		if ( isBoss )
		{
			if ( transform.parent )
			{
				transform.parent.SendMessage( "SetLockonBonus", lockonBonus );
				transform.parent.SendMessage( "SetIsBreak", true );
			}
		}
	}
	
	private bool CanGuard()
	{
		if ( !isBoss )
		{
			return false;
		}
		
		int bossLife = boss.GetComponent<EnemyStatus>().GetLife();

		if ( bossLife >= bossGuardLimit )
		{
			return true;
		}
		return false;
	}
	
	protected override void DestroyEnemyEx()
	{
		// 不做任何处理
	}
	
	private void EnableCollider()
	{
		// 使碰撞有效（子对象的Collider全部有效）
		Transform[] children = this.GetComponentsInChildren<Transform>();
  		foreach ( Transform child in children )
		{
			if ( child.tag == "Enemy" )
			{
				if ( child.GetComponent<SphereCollider>() )
				{
					// 设置collider为有效
					child.GetComponent<SphereCollider>().enabled = true;
				}
			}
		}
	}
}

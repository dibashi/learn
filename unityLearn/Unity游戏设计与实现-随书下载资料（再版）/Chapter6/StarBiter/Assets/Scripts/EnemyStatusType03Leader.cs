using UnityEngine;
using System.Collections;

public class EnemyStatusType03Leader : EnemyStatus {
	
	public bool isType03Leader = false;			// 自身为TYPE 03 Leader（=true）
	
	public override void StartSub()
	{
		
		// Type03Leader用
		if ( isType03Leader )
		{
			// 跟随队长
			SetIsFollowingLeader( true );
		}
	}
	
	public override void DestroyEnemySub()
	{
		// Type03Leader用
		if ( isType03Leader )
		{
			if ( transform.parent )
			{
				transform.parent.SendMessage( "SetLockonBonus", lockonBonus );
				transform.parent.SendMessage( "SetIsBreak", true );
			}
		}
	}
	
}

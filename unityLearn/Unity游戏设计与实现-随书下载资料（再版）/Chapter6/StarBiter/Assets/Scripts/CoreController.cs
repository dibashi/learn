using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// BOSS Core 被销毁时显示消息
// ----------------------------------------------------------------------------
public class CoreController : MonoBehaviour {
	
	private PrintMessage printMessage;		// SubScreen的消息区域
	
	void Start () {
		
		// 获取PrintMessage的实例
		printMessage = Navi.get().GetPrintMessage();			
	}
	
	// ------------------------------------------------------------------------
	// BOSS Core 被销毁时的处理
	// ------------------------------------------------------------------------
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if (
				this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
				this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
			{
				printMessage.SetMessage(" ");
				printMessage.SetMessage("DEFEATED SPIDER-TYPE.");
				printMessage.SetMessage("MISSION ACCOMPLISHED.");
				printMessage.SetMessage(" ");
			}
		}
	}
	
}

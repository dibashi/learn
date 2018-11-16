using UnityEngine;
using System.Collections;

public class EngineUnitController : MonoBehaviour {
	
	private PrintMessage printMessage;		// SubScreen的消息区域
	
	void Start () 
	{
		//获取PrintMessage的实例
		printMessage = Navi.get().GetPrintMessage();	
	}
	
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if (
				this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
				this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
			{
				printMessage.SetMessage(" ");
				printMessage.SetMessage("DESTROYED DEFENSIVE BULKHEAD.");
				printMessage.SetMessage(" ");
			}
		}
	}
}

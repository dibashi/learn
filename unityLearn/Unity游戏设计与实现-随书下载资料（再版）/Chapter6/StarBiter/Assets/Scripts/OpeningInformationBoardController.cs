using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 开场画面中显示的消息从画面外由下往上滚动
// ----------------------------------------------------------------------------
public class OpeningInformationBoardController : MonoBehaviour {
	
	public float		scrollSpeed   = 130.0f;				// 标题滚动的速度
	public float		startPosition = -520.0f;			// 滚动的开始位置
	
	private float 		stopPositionY;						// 标题的停止位置

	public GameObject	uiInformationBoard;

	// ================================================================ //
	
	void Start () 
	{
		// 获取消息的实例
		stopPositionY = uiInformationBoard.GetComponent<RectTransform>().localPosition.y;
		
		// 获取滚动的停止位置
		Vector3 tmpPosition =  uiInformationBoard.GetComponent<RectTransform>().localPosition;

		uiInformationBoard.GetComponent<RectTransform>().localPosition = new Vector3( tmpPosition.x, startPosition, 0 );
	}
	
	void Update () 
	{
		// 滚动到停止位置
		Vector3 position = uiInformationBoard.GetComponent<RectTransform>().localPosition;
		if ( position.y < stopPositionY )
		{
			position.y += scrollSpeed * Time.deltaTime;
			uiInformationBoard.GetComponent<RectTransform>().localPosition = new Vector3( position.x, position.y, 0 );
		}

	}
}

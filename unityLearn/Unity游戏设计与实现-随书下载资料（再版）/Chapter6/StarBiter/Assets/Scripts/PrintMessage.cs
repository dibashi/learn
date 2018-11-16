using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// PrintMessage
//  - 控制sub screen显示的消息
//  - 使用方法
//    - 将想要显示的消息作为参数传递给SetMessage
//  - 工作流程
//    - 通过SetMessage传递的消息按照顺序逐个进行处理
//    - 一点一点将消息显示到sub screen
//    - 超过指定的行数后将从首行开始消除
// ----------------------------------------------------------------------------
public class PrintMessage : MonoBehaviour {
	
	private ArrayList	messages   = new ArrayList();
	private bool		isPrinting = false;
	private const int	MAX_ROW_COUNT = 6;
	
	private const int		ADDITION_NUM = 1;		// 一度に表示する文字数.
	private const string 	CURSOR_STR = "_";		// カーソルの文字.

	public UnityEngine.UI.Text	uiLineText;

	// ================================================================ //

	void Start () {

        // 向sub screen添加换行（为了让第一条消息从最下面开始显示）
        uiLineText.text = "\n\n\n\n\n\n";		
		// 游戏开始时显示消息
		SetMessage("STAND BY ALERT.");
		SetMessage("ENEMY FLEETS ARE APPROACHING.");
		SetMessage(" ");
		
	}
	
	void Update () {
	
		// 确实是否存在需要显示的消息
		if ( messages.Count > 0 )
		{
			// 如果正在向sub screen显示消息则不会对新到达的消息进行处理
			if ( !isPrinting )
			{
				// 调用消息显示处理
				isPrinting = true;
				string tmp = messages[0] as string;
				messages.RemoveAt(0); 
				StartCoroutine( "PlayMessage", tmp );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 设置消息（先进先出）
	// ------------------------------------------------------------------------
	public void SetMessage( string message )
	{
		messages.Add( message );
	}
	
	// ------------------------------------------------------------------------
	// 逐步将消息显示到sub screen
	// ------------------------------------------------------------------------
	IEnumerator PlayMessage( string message )
	{
		char[] charactors = new char[256];
			
		// 删除超出存储空间的文字
		if ( message.Length > 255 )
		{
			message = message.Substring(0, 254);
		}
		
		// 将每个文字分割开
		charactors = message.ToCharArray();
		
		// 获取显示的文字
		string subScreenText = uiLineText.text;

		subScreenText += "\n";

		// 一次显示的文字
		// 固定值 ＋ 队列中存在的行数
		// 如果队列中存储了很多待显示的消息，
		// 则加快显示的速度
		int	additionNum = ADDITION_NUM + messages.Count;

		for(int i = 0;i < charactors.Length;i += additionNum)
		{
			// 暂时删除末尾的光标
			if(subScreenText.EndsWith(CURSOR_STR)) {

				subScreenText = subScreenText.Remove(subScreenText.Length - 1);
			}


			// 按照一次additionNum个将缓冲区中的文字添加
			for(int j = 0;j < additionNum;j++) {

				if(i + j >= charactors.Length) {

					break;
				}

				subScreenText += charactors[i + j];
			}


			// 追加光标
			subScreenText += CURSOR_STR;

			// 删除超出画面外的行

			string[] lines = subScreenText.Split("\n"[0]);

			if(lines.Length > MAX_ROW_COUNT) {

				subScreenText = "";

				// 从后面追加 MAX_ROW_COUNT 行
				for(int j = lines.Length - MAX_ROW_COUNT;j < lines.Length;j++) {

					subScreenText += lines[j];

					if(j < lines.Length - 1) {

						subScreenText += "\n";
					}
				}
			}
			
			uiLineText.text = subScreenText;			
			// 等待
			yield return new WaitForSeconds( 0.001f );
		}
		
		// 全文显示结束后，隐藏光标
		if(subScreenText.EndsWith(CURSOR_STR)) {

			subScreenText = subScreenText.Remove(subScreenText.Length - 1);

			uiLineText.text = subScreenText;
		}
			
		// 结束显示处理
		isPrinting = false;
	}

}

using UnityEngine;
using System.Collections;

/// <summary>
/// 用于调试
/// </summary>
public class DebugStageAdapter : MonoBehaviour
{
    [SerializeField]
    private Rect rectArea = new Rect(0, 460, 640, 100);
    [SerializeField]
    private Rect rectS1 = new Rect(0, 0, 100, 20);
    [SerializeField]
    private Rect rectS2 = new Rect(100, 0, 100, 20);
    [SerializeField]
    private Rect rectS3 = new Rect(200, 0, 100, 20);

	void Start()
	{
		if(dbwin.root().getWindow("调试") == null) {
			
			var		window = dbwin.root().createWindow("调试");
			
			window.createButton("摧毁敌人")
				.setOnPress(() =>
					{
						GameObject[]	gos = GameObject.FindGameObjectsWithTag("Enemy");

						foreach(var go in gos) {

							Note	note = go.GetComponentInChildren<Note>();

							note.SendMessage("OnHit");
						}
					});
		}
	}

    void OnGUI()
    {
        /*GUILayout.BeginArea(rectArea);
        if (GUI.Button(rectS1, "Load Stage1"))
        {
            SendMessage("OnFieldLoad", StageAdapter.Type.Stage1);
        }
        if (GUI.Button(rectS2, "Load Stage2"))
        {
            SendMessage("OnFieldLoad", StageAdapter.Type.Stage2);
        }
        if (GUI.Button(rectS3, "Load Stage3"))
        {
            SendMessage("OnFieldLoad", StageAdapter.Type.Stage3);
        }
        GUILayout.EndArea();*/
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// 用于调试。通过按钮触发SceneSelector
/// </summary>
public class DebugSceneSelector : MonoBehaviour
{
    [SerializeField]
    private Rect rectArea = new Rect(0, 460, 640, 100);
    [SerializeField]
    private Rect rectT = new Rect(0, 0, 100, 20);
    [SerializeField]
    private Rect rectS = new Rect(100, 0, 100, 20);

    void OnGUI()
    {
        GUILayout.BeginArea(rectArea);
        if (GUI.Button(rectT, "Load Title"))
        {
            SendMessage("OnStartTitle");
        }
        if (GUI.Button(rectS, "Load Stage"))
        {
            SendMessage("OnNextStage");
        }
        GUILayout.EndArea();
    }
}

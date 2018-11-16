using UnityEngine;
using System.Collections;

/// <summary>
/// 用于调试Field
/// </summary>
public class DebugField : MonoBehaviour
{
    [SerializeField]
    private Rect rectArea = new Rect(0, 420, 640, 100);
    [SerializeField]
    private Rect rectS1 = new Rect(0, 0, 100, 20);
    [SerializeField]
    private Rect rectS2 = new Rect(100, 0, 100, 20);

    void OnGUI()
    {
        GUILayout.BeginArea(rectArea);
        if (GUI.Button(rectS1, "OnGameStart"))
        {
            BroadcastMessage("OnGameStart");
        }
        if (GUI.Button(rectS2, "OnGameOver"))
        {
            BroadcastMessage("OnGameOver");
        }
        GUILayout.EndArea();
    }
}

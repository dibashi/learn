using UnityEngine;
using System.Collections;

/// <summary>
/// 显示的部分
/// </summary>
public class GUIDisplay : MonoBehaviour {

    private string itemHitCapsule   = "Capsule Hits: ";
    private string itemHitCube      = "Cube Hits   : ";

    private int capsuleNum = 0;
    private int cubeNum = 0;

    // Use this for initialization
	void Start () {
	
	}

    void OnHitItem(  string itemName )
    {
        if (itemName.Contains("TestCube") )
        {
            cubeNum++;
        }
        else if (itemName.Contains("TestCapsule"))
        {
            capsuleNum++;
        }
    }

    void OnGUI()
    {
//        GUI.Label(new Rect(10, 10, 200, 20), enemyHit + enemyNum);
        GUI.Label(new Rect(10, 30, 200, 50), itemHitCube + cubeNum);
        GUI.Label(new Rect(10, 50, 200, 70), itemHitCapsule + capsuleNum);
    }
    
}

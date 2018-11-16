using UnityEngine;
using System.Collections;

// 开始菜单的GUI类
public class StartupMenuGUI : MonoBehaviour {

	void	Start()
	{
	}
	void 	Update()
	{
	}

	public void	onStartButtonPressed()
	{
		GameObject.Find("PhaseManager").GetComponent<PhaseManager>().SetPhase("OnBeginInstruction");
	}
	public void	onDevelopmentButtonPressed()
	{
		GameObject.Find("PhaseManager").GetComponent<PhaseManager>().SetPhase("DevelopmentMode");
	}

}

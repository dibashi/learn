using UnityEngine;
using System.Collections;

public class TitleControl : MonoBehaviour {

	protected string	pressed_button = "";

	// ================================================================ //
	// 继承于MonoBehaviour 

	void	Start()
	{	
	}
	
	void	Update()
	{
		if(this.pressed_button != "") {

			switch(this.pressed_button) {

				case "easy":
				{
					GlobalParam.GetInstance().SetMode(0);
					UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
				}
				break;

				case "normal":
				{
					GlobalParam.GetInstance().SetMode(1);
					UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
				}
				break;

				case "hard":
				{
					GlobalParam.GetInstance().SetMode(2);
					UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
				}
				break;

				default:
					break;
			}
		}
	}

	// ================================================================ //

	public void	onButtonPressed(string button_name)
	{
		this.pressed_button = button_name;
	}
	
}

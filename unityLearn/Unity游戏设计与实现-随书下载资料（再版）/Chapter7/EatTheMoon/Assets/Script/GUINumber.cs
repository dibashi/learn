using UnityEngine;
using System.Collections;

public class GUINumber : MonoBehaviour {

	public UnityEngine.Sprite[]		numSprites;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
	}

	void	Start()
	{
	
	}

	// ================================================================ //

	public void		setNumber(int number)
	{
		this.GetComponent<UnityEngine.UI.Image>().sprite = this.numSprites[number];	
	}
}

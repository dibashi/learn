using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Navi : MonoBehaviour {

	public GameObject	uiCanvas;
	public GameObject	uiEndImage;

	public GameObject	uiFaceIconPrefab = null;

	protected List<FaceIcon>	face_icons = new List<FaceIcon>();

	public static Vector2	face_icon_base_position;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
	}

	void	Start()
	{
		Navi.face_icon_base_position.x = -Screen.width/2.0f  + FaceIcon.WIDTH/2.0f  + 10.0f;
		Navi.face_icon_base_position.y = -Screen.height/2.0f + FaceIcon.HEIGHT/2.0f + 30.0f;

		for(int i = SceneControl.LIFE_COUNT - 1;i >= 0;i--) {
		
			FaceIcon	face_icon = this.create_face_icon();

			Vector2		position = Navi.face_icon_base_position + new Vector2(0.0f, 25.0f*i);

			face_icon.setPosition(position);

			this.face_icons.Add(face_icon);
		}
	}
	
	void	Update()
	{
		do {

			if(this.face_icons.Count <= 0) {

				break;
			}
			if(!this.face_icons[0].isVanished()) {

				break;
			}

			GameObject.Destroy(this.face_icons[0].gameObject);
			this.face_icons.RemoveAt(0);

		} while(false);
	}

	// ================================================================ //

	// 设置连击
	public void		setCombo(SceneControl.COMBO combo)
	{
		if(this.face_icons.Count > 0) {
	
			this.face_icons[0].setCombo(combo);
		}
	}

	// 显示“结束”提示
	public void		dispEnd()
	{
		this.uiEndImage.SetActive(true);
	}

	// 是否正在显示“结束”提示？
	public bool		isDispEnd()
	{
		return(this.uiEndImage.activeSelf);
	}

	// ================================================================ //

	// 创建用于表示残余机会次数的图标（猫脸）
	protected FaceIcon	create_face_icon()
	{
		FaceIcon	icon = GameObject.Instantiate(this.uiFaceIconPrefab).GetComponent<FaceIcon>();

		icon.GetComponent<RectTransform>().SetParent(this.uiCanvas.GetComponent<RectTransform>());
		icon.GetComponent<RectTransform>().SetSiblingIndex(0);

		return(icon);
	}

	// ================================================================ //
	//																	//
	// ================================================================ //

	protected static	Navi instance = null;

	public static Navi	get()
	{
		if(instance == null) {

			instance  = GameObject.Find("GameCanvas").GetComponent<Navi>();
		}

		return(instance);
	}
}
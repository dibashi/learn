using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 画面淡入操作
//  - 使alpha(透明度) 按照 1(透明度0%) -> 0(透明度100%) 变换来实现淡入效果
// ----------------------------------------------------------------------------
public class FadeIn : MonoBehaviour {
	
	protected float		alphaRate = 1f;				// 透明度
	protected Color		textureColor;				// 纹理颜色信息
	
	public UnityEngine.UI.Image		uiImage;

	void Start () {
	
		// 执行了FadeOut的状态
		this.textureColor    = this.uiImage.color;
		this.textureColor.a  = this.alphaRate;
		this.uiImage.color   = this.textureColor;
		this.uiImage.enabled = true;
	}

	void Update () {
	
		// 透明度是否已达到100%	
		if ( this.alphaRate > 0 ) {

			// FadeIn.
			this.alphaRate -= 0.007f;
			this.textureColor.a = this.alphaRate;
			this.uiImage.color = this.textureColor;
		}

	}
}

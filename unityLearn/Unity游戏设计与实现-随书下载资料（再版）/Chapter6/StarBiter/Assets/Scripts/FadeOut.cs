using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 画面淡出操作
//  - 使alpha(透明度) 按照 0(透明度100%) -> 1(透明度0%) 变换来实现淡出效果
// ----------------------------------------------------------------------------
public class FadeOut : MonoBehaviour 
{
	public UnityEngine.UI.Image		uiImage;

	protected float		alphaRate = 0f;						// 透明度
	protected Color		textureColor;						// 纹理颜色信息

	protected string 	openingSceneName = "opening";		// 开场场景名称

	protected bool 		isEnabled = false;

	void Start () 
	{
		// 执行了FadeOut后的状态
		this.textureColor    = this.uiImage.color;
		this.textureColor.a  = this.alphaRate;
		this.uiImage.color   = this.textureColor;
		this.uiImage.enabled = true;
	}

	void Update () 
	{
		if( this.isEnabled ) {

			// 透明度是否达到100%？
			if( this.alphaRate < 1.0f ) {

				// FadeOut.
				this.alphaRate += 0.007f;
				this.textureColor.a = this.alphaRate;
				this.uiImage.color  = this.textureColor;

			} else {

				// 载入游戏场景
				UnityEngine.SceneManagement.SceneManager.LoadScene( openingSceneName );
			}
		}
	}
	
	public void SetEnable()
	{
		StartCoroutine( WaitAndEnable( 8f ) );
	}
	
	IEnumerator WaitAndEnable( float waitForSeconds )
	{
		// 等待指定的时间
		yield return new WaitForSeconds( waitForSeconds );
		
		this.isEnabled = true;
	}
}

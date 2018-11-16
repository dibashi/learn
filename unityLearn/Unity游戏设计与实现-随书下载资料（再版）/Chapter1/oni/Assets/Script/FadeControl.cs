using UnityEngine;
using System.Collections;

public class FadeControl : MonoBehaviour
{
    private float	timer;				//当前的时间
	private float	fadeTime;			// 淡入淡出需要的时间
	private	Color	colorStart;			// 淡入淡出开始时的颜色
	private	Color	colorTarget;		// 淡入淡出结束时的颜色

	public UnityEngine.UI.Image		ui_image;
	
 	// ================================================================ //

	void Awake()
    {
		this.timer			= 0.0f;
		this.fadeTime		= 0.0f;
		this.colorStart		= new Color( 0.0f, 0.0f, 0.0f, 0.0f );
		this.colorTarget	= new Color( 0.0f, 0.0f, 0.0f, 0.0f );
    }
    
 	void	Update()
	{
		if(this.timer < this.fadeTime) {

			float	rate;
			
			rate = this.timer/this.fadeTime;

			if(float.IsNaN(rate)) {

				rate = 1.0f;
			}

			rate = Mathf.Sin(rate*Mathf.PI/2.0f);

			Color	color = Color.Lerp(this.colorStart, this.colorTarget, rate);

			this.ui_image.color = color;		
		}

		this.timer += Time.deltaTime;
	}
    
    //void OnApplicationQuit()
   // {
    //    Destroy(texture);
    //}
	
	public void fade( float time, Color start, Color target )
	{
		this.ui_image.gameObject.SetActive(true);

		this.fadeTime		= time;
		this.timer			= 0.0f;
		this.colorStart		= start;
		this.colorTarget	= target;
	}
	
	public bool isActive()
	{
		return ( this.timer > this.fadeTime ) ? false : true;
	}

	// ================================================================ //
	// 对象实例

	private	static FadeControl	instance = null;

	public static FadeControl	get()
	{
		if(FadeControl.instance == null) {

			GameObject		go = GameObject.Find("FadeControl");

			if(go != null) {

				FadeControl.instance = go.GetComponent<FadeControl>();

			} else {

				Debug.LogError("Can't find game object \"FadeControl\".");
			}
		}

		return(FadeControl.instance);
	}
}
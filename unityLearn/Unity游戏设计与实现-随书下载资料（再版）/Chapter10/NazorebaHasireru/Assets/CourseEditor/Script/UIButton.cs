using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	public bool		current = false;
	public bool		trigger_on = false;
	public bool		is_repeat_button = false;

	public float	pushed_timer = 0.0f;		// 按下的时间长度

	protected bool	req_trigger_on  = false;
	protected bool	req_trigger_off = false;
	protected int	pushed_counter = 0;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Start()
	{
	}
	
	void 	Update()
	{
		if(this.req_trigger_on) {

			this.trigger_on = true;
			this.current    = true;

			this.pushed_timer = 0.0f;
			this.pushed_counter = 0;

			this.req_trigger_on = false;
		}

		if(this.req_trigger_off) {

			this.current = false;

			this.req_trigger_off = false;
		}

		if(this.current) {

			if(this.pushed_counter > 0) {

				this.trigger_on = false;
			}

			this.pushed_counter++;
			this.pushed_timer += Time.deltaTime;
		}
	}

	// ================================================================ //

	public void		setPosition(float x, float y)
	{
		this.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0.0f);
	}

	public void		setScale(float x, float y)
	{
		this.GetComponent<RectTransform>().localScale = new Vector3(x, y, 0.0f);
	}

	public void		setText(string text)
	{
		this.GetComponentInChildren<UnityEngine.UI.Text>().text = text;
	}

	// ================================================================ //

	public void OnPointerDown(PointerEventData eventData) 
	{
		this.req_trigger_on = true;
	}

	public void	OnPointerUp(PointerEventData eventData) 
	{
		this.req_trigger_off = true;
	}

}

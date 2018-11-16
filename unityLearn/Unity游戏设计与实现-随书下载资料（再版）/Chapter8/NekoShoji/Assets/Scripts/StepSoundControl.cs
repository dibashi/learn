using UnityEngine;
using System.Collections;

public class StepSoundControl : MonoBehaviour {

	public AudioClip 	stepSE1 = null;
	public AudioClip 	stepSE2 = null;

	public int			step_sound_sel = 0;

	// ================================================================ //
	// 继承于MonoBehaviour 

	void 	Start()
	{
	}

	void	Update()
	{
	}

	// ================================================================ //

	// Animator的事件
	public void 	PlayStepSound()
	{
		if(this.step_sound_sel == 0) {

			this.GetComponent<AudioSource>().PlayOneShot(this.stepSE1);

		} else {

			this.GetComponent<AudioSource>().PlayOneShot(this.stepSE2);
		}

		this.step_sound_sel = (this.step_sound_sel + 1)%2;
	}
}

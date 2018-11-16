using UnityEngine;
using System.Collections;

public class VanishEffectControl : MonoBehaviour {

	public	GameObject	eff_blue;		// CYAN = 0,
	public	GameObject	eff_yellow;		// YELLOW,
	public	GameObject	eff_orange;		// ORANGE,
	public	GameObject	eff_purple;		// MAGENTA,
	public	GameObject	eff_green;		// GREEN,
										// PINK,

	// ---------------------------------------------------------------- //

	void 	Start()
	{
	
	}
	
	void	Update()
	{
	
	}

	public void	createEffect(StackBlock block)
	{
		GameObject	fx_prefab = null;

		switch(block.color_type) {

			case Block.COLOR_TYPE.CYAN:		fx_prefab = eff_blue;	break;
			case Block.COLOR_TYPE.YELLOW:	fx_prefab = eff_yellow;	break;
			case Block.COLOR_TYPE.ORANGE:	fx_prefab = eff_orange;	break;
			case Block.COLOR_TYPE.MAGENTA:	fx_prefab = eff_purple;	break;
			case Block.COLOR_TYPE.GREEN:	fx_prefab = eff_green;	break;
		}

		if(fx_prefab != null) {

			GameObject 	go = Instantiate(fx_prefab) as GameObject;

			go.AddComponent<FruitEffect>();

			go.transform.position = block.transform.position + Vector3.back*1.0f;
		}
	}
}

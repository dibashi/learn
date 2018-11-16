using UnityEngine;
using System.Collections;

public class ItemSwordScr : MonoBehaviour {
	public float ROT_SPEED = 360;
	public GameObject GET_EFFECT;
	public AudioClip GET_SE;
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(0,ROT_SPEED*Time.deltaTime,0);
	}
	
	public void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerController>() != null) {
			other.SendMessage("OnGetSword",0);
			transform.localScale = new Vector3(0,0,0);
			AudioChannels audioChannels = FindObjectOfType(typeof(AudioChannels)) as AudioChannels;
			if (audioChannels != null)
				audioChannels.PlayOneShot(GET_SE,1,0);
			Object geteffect = Instantiate(GET_EFFECT,transform.position,Quaternion.identity);
			Destroy(geteffect,1.0f);
			Destroy(gameObject);
		}
	}
}

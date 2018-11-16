using UnityEngine;
using System.Collections;

public class Note : MonoBehaviour {
    [SerializeField]
    private float interval = 1.0f;
    [SerializeField]
    private float offset = 0.0f;
    [SerializeField]
    private bool visible = true;
    [SerializeField]
    private bool valid   = true;

    private float counter;
    private float param;
    private Color baseColor;

    private void OnHitItem()
    {
        valid = false;
        // 使用Stop时声音可能会突然停顿，因此稍作处理令声音淡出
        //audio.Stop();

        // 开始特效（只有1个）
        ParticleSystem particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        if (particleSystem) {
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			if(player != null) {

				this.transform.parent = player.transform;
				this.transform.localPosition = Vector3.forward*1.0f;
			}
            particleSystem.Play();
        }

        StartCoroutine("Fadeout", 1.0f);
    }

    private IEnumerator Fadeout(float duration)
    {
        // 淡出处理
        float currentTime = 0.0f;
        float waitTime = 0.02f;
        float firstVol = GetComponent<AudioSource>().volume;
        while (duration > currentTime)
        {
            GetComponent<AudioSource>().volume = Mathf.Lerp( firstVol, 0.0f, currentTime/duration );
            yield return new WaitForSeconds(waitTime);
            currentTime += waitTime;
        }

        // 特效完全结束后销毁对象
        ParticleSystem particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        while (particleSystem.isPlaying)
        {
            yield return new WaitForSeconds(waitTime);
        }

        GameObject parent = GameObject.Find("/NotesObject");
        if(parent) parent.SendMessage("OnDestroyObject", gameObject);
    }

    private void Clock(float step)
    {
        if (valid)
        {
            counter += step;
            if (counter >= interval)
            {
                GetComponent<AudioSource>().Play();
                param = 1.0f;
                counter = 0.0f;
            }
        }
    }

    // Use this for initialization
	void Start () 
    {
        counter = offset;
        GetComponent<Renderer>().enabled = visible;
        baseColor = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b);
        param = 1.0f;
    }
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        if (valid)
        {
            Clock(Time.deltaTime);
            if (visible)
            {
                param *= Mathf.Exp(-3.0f * Time.deltaTime);
                //	        transform.localScale = Vector3.one * (1.0f + param * 0.5f);
                Color color = new Color(Mathf.Abs(baseColor.r - param), baseColor.g, baseColor.b);
                GetComponent<Renderer>().material.color = color;
            }
        }
	}

}

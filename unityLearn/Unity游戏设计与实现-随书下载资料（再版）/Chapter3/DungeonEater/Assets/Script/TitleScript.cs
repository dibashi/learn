using UnityEngine;
using System.Collections;

public class TitleScript : MonoBehaviour {
	public Texture m_titleTex;
	public float m_go_adv = 10.0f;
	private bool m_PlayButtonPushed = false;
	
	// Update is called once per frame
	void Update () {
		m_go_adv -= Time.deltaTime;
		if (m_go_adv < 0.0f && !m_PlayButtonPushed) {
			GlobalParam.GetInstance().SetMode(true);
			UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
		}
	}

	public void		OnPlayButtonPressed()
	{
		m_PlayButtonPushed = true;
		GetComponent<AudioSource>().Play();
		GlobalParam.GetInstance().SetMode(false);
		StartCoroutine(StartGame());
	}

	IEnumerator StartGame()
	{
		yield return new WaitForSeconds(1.0f);
		UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
	}
}

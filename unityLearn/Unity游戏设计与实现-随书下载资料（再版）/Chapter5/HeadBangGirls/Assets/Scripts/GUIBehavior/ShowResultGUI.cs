using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using System.Collections;
//带有快速定位功能的开发模式GUI类
public class ShowResultGUI : MonoBehaviour
{
	public enum RESULT {

		NONE = -1,

		EXCELLENT = 0,
		GOOD,
		BAD,

		NUM,
	};

	public string[] comments_EXCELLENT;
	public string[] comments_GOOD;
	public string[] comments_BAD;

	public UnityEngine.UI.Text	uiScoreText;
	public UnityEngine.UI.Text	uiCommentText;

	protected ScoringManager	m_scoringManager;

	// ================================================================ //

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void BeginVisualization(RESULT result)
	{
		m_scoringManager = GameObject.Find("ScoringManager").GetComponent<ScoringManager>();

		string[]	comments;

		switch(result) {
			default:
			case RESULT.EXCELLENT:	comments = this.comments_EXCELLENT;		break;
			case RESULT.GOOD:		comments = this.comments_GOOD;			break;
			case RESULT.BAD:		comments = this.comments_BAD;			break;
		}

		this.uiCommentText.text = "";
		foreach(var comment in comments) {

			this.uiCommentText.text += comment;
			this.uiCommentText.text += "\n";
		}

		this.uiScoreText.text = m_scoringManager.score.ToString();
	}

	public void	onReturnButtonPressed()
	{
		GameObject.Find("PhaseManager").GetComponent<PhaseManager>().SetPhase("Restart");
	}

}

using UnityEngine;
using System.Collections;

/// <summary>
/// 项目的碰撞检测
/// </summary>
public class ItemCollider : MonoBehaviour
{
    private bool 	isFinished;

    void Start()
    {
        isFinished = false;
        GetComponent<Collider>().isTrigger = true;  // 设置好触发器
    }

	void	Update()
	{
		// 自爆
		if(Input.GetKeyDown(KeyCode.D)) {

			this.on_player_hit();
		}
	}

    void OnTriggerEnter(Collider collider)
    {
        if (isFinished) return; 	// 因为只希望发生一次碰撞所以用它来监测
                                	// 如果isTrigge=false则会多次执行碰撞

        GameObject obj = collider.gameObject;

        if (obj.tag.Equals("Player"))   // 判断是否是玩家
        {
			this.on_player_hit();
        }
    }

	protected void	on_player_hit()
	{
        isFinished = true;

        GameObject ui = GameObject.Find("/UI");
        if (ui) ui.SendMessage("OnHitItem", gameObject.name);

        Note note = GetComponent<Note>();
        if (note) note.SendMessage("OnHitItem");
	}
}

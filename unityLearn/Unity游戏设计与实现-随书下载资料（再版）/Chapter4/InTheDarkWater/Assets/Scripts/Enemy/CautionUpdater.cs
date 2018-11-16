using UnityEngine;
using System.Collections;

/// <summary>
/// 用于一直显示最新的最大值Caution的脚本
/// </summary>
public class CautionUpdater : MonoBehaviour
{
    [SerializeField] // debug
    private int instantiatedCount = 0;

    private GameObject ui = null;
    private GameObject maxCautionEnemy = null;

    void Start()
    {
    }

    void OnGameStart()
    {
        // 因为场景是分开的，为了保险起见在OnGameStart中连接
        ui = GameObject.Find("/UI");
    }

    void OnInstantiatedChild(GameObject target)
    {
        instantiatedCount++;
        //EnemyCaution enemyCaution = target.GetComponent<EnemyCaution>();
        //float waitTime = 0.0f;
        //if (instantiatedCount>0) waitTime = maxWaitTime / (float)instantiatedCount;
        //enemyCaution.SetCountUp(waitTime);

        // 通常而言值肯定为0,为保险起见执行Update
        DisplayValue(target, GetCautionValue(target));
    }

    void OnDestroyChild(GameObject target)
    {
        if (target.Equals(maxCautionEnemy))
        {
            maxCautionEnemy = null;
            if(ui)ui.BroadcastMessage("OnUpdateCaution", 0, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void DisplayValue(GameObject updateEnemy, int newValue)
    {
        //Debug.Log(updateEnemy.name + ".cation=" + newValue);
        int maxValue = 0;
        if (!updateEnemy.Equals(maxCautionEnemy))
        {
            // 如果值不相同则和持有现在Max值的敌人当前值作比较
            maxValue = GetCautionValue(maxCautionEnemy);
            if (newValue > maxValue)
            {
                maxValue = newValue;
                maxCautionEnemy = updateEnemy;
            }
        }
        else
        {
            // 如果一致则直接更新
            maxValue = newValue;
        }
        // 传递最大值用于显示
        if(ui)ui.BroadcastMessage("OnUpdateCaution", maxValue, SendMessageOptions.DontRequireReceiver);
    }

    private int GetCautionValue(GameObject enemyObj)
    {
        if(enemyObj == null ) return 0;
        EnemyCaution enemyCauiton = enemyObj.GetComponent<EnemyCaution>();
        if (enemyCauiton) return enemyCauiton.Value();
        return 0;
    }
}

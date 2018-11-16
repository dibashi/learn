using UnityEngine;
using System.Collections;

/// <summary>
/// 主动声纳
/// </summary>
public class ActiveSonar : MonoBehaviour {

    [SerializeField]
    private float delayTime = 0.2f;

    private float maxRadius = 300.0f;

    private GameObject player = null;
    private RandomGenerator enemy = null;
    private RandomGenerator item  = null;
    private TorpedoManager torpedo = null;
    private SonarEffect effect = null;

	void Start () 
    {
        effect = GetComponent<SonarEffect>();

        player = GameObject.Find("/Field/Player");
        GameObject enemyObj = GameObject.Find("/Field/Enemies");
        if (enemyObj) enemy = enemyObj.GetComponent<RandomGenerator>();
        GameObject itemObj = GameObject.Find("/Field/Items");
        if (itemObj) item = itemObj.GetComponent<RandomGenerator>();
        GameObject torpedoObj = GameObject.Find("/Field/Torpedoes");
        if (torpedoObj) torpedo = torpedoObj.GetComponent<TorpedoManager>();

        GameObject sonarCameraObj = GameObject.Find("/Field/Player/SonarCamera");
        if (sonarCameraObj) maxRadius = sonarCameraObj.GetComponent<SphereCollider>().radius;

        StartCoroutine("Delay");
	}

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(delayTime);

        // 简易版本的探索
        float effectDist = Mathf.Lerp(0.0f, maxRadius, effect.Value());
        //Debug.Log("ActiveSonar="+effectDist + ":" + Time.time);
        if (enemy)
        {
            //Debug.Log(effectDist + "- Enemy Search :" + enemy.SonarChildren().Count);
            Search(enemy.SonarChildren(), effectDist);
        }
        if (item)
        {
            //Debug.Log(effectDist + "- Item Search :" + item.SonarChildren().Count);
            Search(item.SonarChildren(), effectDist);
        }
        if (torpedo)
        {
            //Debug.Log(effectDist + "- Torpedo Search :" + torpedo.SonarChildren().Count);
            Search(torpedo.SonarChildren(), effectDist);
        }

        StartCoroutine("Delay");
    }
	
    void Search(ArrayList array, float effectDist) 
    {
        if (array == null) return;

        int i = 0;
        while (i < array.Count)
        {
            GameObject target = array[i] as GameObject;
            if (target == null)
            {
                i++;
                continue;
            }

            float dist = Vector3.Distance(target.transform.position, player.transform.position);
            if (dist < effectDist)
            {
                // 如果在指定距离内则命中主动的声纳
                target.BroadcastMessage("OnActiveSonar");
                // 从声纳对象列表中剔除
                array.RemoveAt(i);
            }
            else i++;
        }
    }

    public void SetMaxRadius( float radius )
    {
        maxRadius = radius;
    }
}

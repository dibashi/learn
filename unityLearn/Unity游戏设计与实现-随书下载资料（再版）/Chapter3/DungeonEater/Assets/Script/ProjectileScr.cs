using UnityEngine;
using System.Collections;

public class ProjectileScr : MonoBehaviour {
	
	IEnumerator Start()
	{
		yield return new WaitForSeconds(0.1f);
		Destroy(gameObject);
	}
	void OnTriggerEnter(Collider other)
	{
		MonsterCtrl mo = other.GetComponent<MonsterCtrl>();
		if (mo != null) {
			mo.Damage();
		}
	}
}

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SpellCaster : NetworkBehaviour {
	
	public float fireballSpeed = 8.0F;
	public GameObject fireballPrefab;

	public GameObject CreateFire() {
		var fireball = (GameObject)Instantiate(
			fireballPrefab,
			transform.position,
			Quaternion.identity);
		fireball.transform.SetParent (transform);
		return fireball;
	}

//	void Update() {
//		for (int i=0; i<transform.childCount; i++) {
//			if (transform.GetChild(i).localScale.x < 0.5f) {
//				transform.GetChild(i).localScale *= 1.05f;
//			} else {
//				transform.GetChild(i).localScale = new Vector3(0.5f,0.5f,0.5f);
//			}
//		}
//	}

	public void FireFire()
	{
		foreach (Rigidbody childbody in transform.GetComponentsInChildren<Rigidbody>()) {
			childbody.useGravity = true;
			childbody.velocity = transform.forward * fireballSpeed * childbody.transform.localScale.x;
		}
		transform.DetachChildren ();
	}
}

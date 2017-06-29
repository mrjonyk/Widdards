using UnityEngine;
using System.Collections;

public class ChargeSpell : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		if (transform.parent != null) {
			if (transform.localScale.x < 0.5f) {
				transform.localScale *= 1.05f;
			} else {
				transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour {
	List<GameObject> gameObjectts;

	// Use this for initialization
	void Start () {
		
		foreach(GameObject ob in gameObjectts){
			//移動 
			//回転
			//状態変化
			//衝突検知
			//などなど
		}

	}
	
	void Update () {
		transform.Rotate(new Vector3(1.0f, 1.0f, 1.0f));
	}
}

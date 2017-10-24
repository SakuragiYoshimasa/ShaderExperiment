using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RockParticles {
	public class CameraController : MonoBehaviour {

		public float speed;
		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			this.transform.position += new Vector3 (0,0,speed);
		}
	}
}

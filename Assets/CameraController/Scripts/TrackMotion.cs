using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CameraController{
	[Serializable]
	public class TrackMotion {
		public GameObject target;

		public float radius = 10.0f;
		public float theta = 0f;
		public float thetaSpeed = 0f;
		public float phai = 0f;
		public float phaiSpeed = 0f;
		public bool isTracking = false;

		public Vector3 MakeTrackingMotionDiffP(){
			if(!isTracking) return new Vector3(0, 0, 0);
			theta += thetaSpeed * Time.deltaTime;
			phai += phaiSpeed * Time.deltaTime;
			Vector3 pos = target.transform.position + new Vector3(radius * Mathf.Sin(theta) * Mathf.Cos(phai), radius * Mathf.Sin(theta) * Mathf.Sin(phai), radius * Mathf.Cos(theta));
			return pos;
		}
	}
}

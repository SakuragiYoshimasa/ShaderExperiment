using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CameraController{
	[Serializable]
	public class Motion {
		public float speed = 10.0f;
		public float min = -10f, max = 10f;
		public bool ping = true;
		public bool pingpong = true;

		public float makeMotionDiff(float val){
			if(ping && val > max) ping = false;
			if(!ping &&  val < min) ping = true;
			
			float sign = ping || !pingpong ? 1.0f : -1.0f;
			return sign * speed * Time.deltaTime;
		}

		public Vector3 makeMotionDiff3D(Vector3 val){
			return new Vector3(0,0,0);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Basic Camera work and tracking motion referenced
//https://pixta.jp/channel/?p=15795

namespace CameraController {
	[RequireComponent(typeof(Camera)), DisallowMultipleComponent]

	public class CameraController : MonoBehaviour {
		
		Vector3 staticRotation,  dynamicPosition, trackPosition, trackRotation;
		
		#region Motions
		[SerializeField] Motion pan;
		public Motion Pan {
			get {
				if(pan == null) pan = new Motion();
				return pan;
			}
		}

		[SerializeField] Motion tilt;
		public Motion Tilt{
			get {
				if(tilt == null) tilt = new Motion();
				return tilt;
			}
		}

		[SerializeField] Motion zoom;
		public Motion Zoom {
			get {
				if(zoom == null) zoom = new Motion();
				return zoom;
			}
		}

		[SerializeField] Motion craneH;
		public Motion CraneH {
			get {
				if(craneH == null) craneH = new Motion();
				return craneH;
			}
		}

		[SerializeField] Motion craneV;
		public Motion CraneV {
			get {
				if(craneV == null) craneV = new Motion();
				return craneV;
			}
		}

		[SerializeField] TrackMotion track;
		public TrackMotion Track {
			get {
				if(track == null) track = new TrackMotion();
				return track;
			}
		}
		#endregion //Motions

		void Awake () {
			staticRotation = Vector3.zero;
			dynamicPosition = Vector3.zero;
			trackPosition = Vector3.zero;
			trackRotation = transform.rotation.eulerAngles;
		}
		
		void FixedUpdate () {

			staticRotation.x += Tilt.makeMotionDiff(staticRotation.x);
			staticRotation.y += Pan.makeMotionDiff(staticRotation.y);
			dynamicPosition.x += CraneH.makeMotionDiff(dynamicPosition.x);
			dynamicPosition.y += CraneV.makeMotionDiff(dynamicPosition.y);
			dynamicPosition.z += Zoom.makeMotionDiff(dynamicPosition.z);
			trackPosition = Track.MakeTrackingMotionDiffP();

			transform.position = dynamicPosition + trackPosition;
			if(Track.isTracking){
				transform.LookAt(Track.target.transform, Vector3.up);
				transform.rotation = Quaternion.Euler(trackRotation + staticRotation + transform.rotation.eulerAngles);
			}else{
				transform.rotation = Quaternion.Euler(trackRotation + staticRotation);
			}
		
			
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CameraController{
	[CustomEditor(typeof(CameraController))]
	public class CameraControllerInspector : Editor {
		public override void OnInspectorGUI(){
			CameraController cc = target as CameraController;

			EditorGUILayout.LabelField("Pan settings");
	
			cc.Pan.speed =  EditorGUILayout.Slider("speed", cc.Pan.speed, 0f, 20.0f);
			cc.Pan.pingpong = EditorGUILayout.Toggle("PingPong", cc.Pan.pingpong);
			cc.Pan.min = EditorGUILayout.FloatField("min", cc.Pan.min);
        	cc.Pan.max = EditorGUILayout.FloatField("max", cc.Pan.max);
        	EditorGUILayout.MinMaxSlider(ref cc.Pan.min, ref cc.Pan.max, -100.0f, 100.0f);

			EditorGUILayout.LabelField("Tilt settings");
			cc.Tilt.speed =  EditorGUILayout.Slider("speed", cc.Tilt.speed, 0f, 20.0f);
			cc.Tilt.pingpong = EditorGUILayout.Toggle("PingPong", cc.Tilt.pingpong);
			cc.Tilt.min = EditorGUILayout.FloatField("min", cc.Tilt.min);
        	cc.Tilt.max = EditorGUILayout.FloatField("max", cc.Tilt.max);
        	EditorGUILayout.MinMaxSlider(ref cc.Tilt.min, ref cc.Tilt.max, -100.0f, 100.0f);
			
			EditorGUILayout.LabelField("Zoom settings");
			cc.Zoom.speed =  EditorGUILayout.Slider("speed", cc.Zoom.speed, 0f, 20.0f);
			cc.Zoom.pingpong = EditorGUILayout.Toggle("PingPong", cc.Zoom.pingpong);
			cc.Zoom.min = EditorGUILayout.FloatField("min", cc.Zoom.min);
        	cc.Zoom.max = EditorGUILayout.FloatField("max", cc.Zoom.max);
        	EditorGUILayout.MinMaxSlider(ref cc.Zoom.min, ref cc.Zoom.max, -100.0f, 100.0f);

			EditorGUILayout.LabelField("Crane H settings");
			cc.CraneH.speed =  EditorGUILayout.Slider("speed", cc.CraneH.speed, 0f, 20.0f);
			cc.CraneH.pingpong = EditorGUILayout.Toggle("PingPong", cc.CraneH.pingpong);
			cc.CraneH.min = EditorGUILayout.FloatField("min", cc.CraneH.min);
        	cc.CraneH.max = EditorGUILayout.FloatField("max", cc.CraneH.max);
        	EditorGUILayout.MinMaxSlider(ref cc.CraneH.min, ref cc.CraneH.max, -100.0f, 100.0f);

			EditorGUILayout.LabelField("Crane V settings");
			cc.CraneV.speed =  EditorGUILayout.Slider("speed", cc.CraneV.speed, 0f, 20.0f);
			cc.CraneV.pingpong = EditorGUILayout.Toggle("PingPong", cc.CraneV.pingpong);
			cc.CraneV.min = EditorGUILayout.FloatField("min", cc.CraneV.min);
        	cc.CraneV.max = EditorGUILayout.FloatField("max", cc.CraneV.max);
        	EditorGUILayout.MinMaxSlider(ref cc.CraneV.min, ref cc.CraneV.max, -100.0f, 100.0f);

			EditorGUILayout.LabelField("Tracking setting");
			cc.Track.isTracking = EditorGUILayout.Toggle("Track", cc.Track.isTracking);
			cc.Track.target = EditorGUILayout.ObjectField("Target", cc.Track.target, typeof(GameObject), true) as GameObject;
			cc.Track.radius = EditorGUILayout.Slider("Radius", cc.Track.radius, 1.0f, 100.0f);
			cc.Track.theta = EditorGUILayout.FloatField("Theta", cc.Track.theta);
			cc.Track.thetaSpeed = EditorGUILayout.FloatField("ThetaSpeed", cc.Track.thetaSpeed);
			cc.Track.phai = EditorGUILayout.FloatField("Phai", cc.Track.phai);
			cc.Track.phaiSpeed = EditorGUILayout.FloatField("PhaiSpeed", cc.Track.phaiSpeed);
		}
	}
}
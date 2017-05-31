using System;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using Unity.Vfx.Cameras.Model;
using UnityEngine;
using UnityEditor;

namespace Unity.Vfx.Cameras.Editor
{
	[ExecuteInEditMode]
	[CustomEditor(typeof(PhysicalCamera))]
	public class PhysicalCameraEditor : UnityEditor.Editor
	{
		public bool m_ShowCamera = true;
		public bool m_showLens = true;
		public bool m_showBody = true;
		public bool m_showStereo = true;
		public bool m_showOctane = false;

		public override void OnInspectorGUI()
		{
			var physCamera = this.serializedObject;
			EditorGUI.BeginChangeCheck();
			physCamera.Update();

			// Start --------------------------------------------------------
			var camObj = serializedObject.targetObject as PhysicalCamera;
			var property = this.serializedObject.FindProperty(() => camObj.m_Mode);
			AddEnumPopup(property, "Mode", "Determines if the Physical Camera controls the attached camera or reads from it.", typeof(PhysicalCameraMode));

			property = this.serializedObject.FindProperty( ()=>camObj.m_AssociatedCameraObj );
			EditorGUILayout.PropertyField(property);

			// models
			var camModel = this.serializedObject.FindProperty("m_Model");
			var lensModel = camModel.FindPropertyRelative("m_Lens");
			var bodyModel = camModel.FindPropertyRelative("m_Body");
			var bodyStereo = camModel.FindPropertyRelative("m_Stereo");

			m_ShowCamera = EditorGUILayout.Foldout(m_ShowCamera, "Camera");
			if (m_ShowCamera)
				DrawCameraModel(camModel, lensModel);

			m_showLens = EditorGUILayout.Foldout(m_showLens, "Lens");
			if (m_showLens)
				DrawLensModel(lensModel);

			m_showBody = EditorGUILayout.Foldout(m_showBody, "Body");
			if (m_showBody)
				DrawBodyModel(bodyModel);

			m_showStereo = EditorGUILayout.Foldout(m_showStereo, "Stereoscopic");
			if (m_showStereo)
				DrawStereoModel(bodyStereo);

			// Done -------------------------------------------------------------
			physCamera.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}

		private void DrawCameraModel(SerializedProperty camModel, SerializedProperty lensModel)
		{
			var camObj = serializedObject.targetObject as PhysicalCamera;
			var camModelObj = camObj.Model;

			// Projection / stereo
			{
				var projProp = camModel.FindPropertyRelative( () => camModelObj.m_ProjectionMode);
				var stereoProp = camModel.FindPropertyRelative(() => camModelObj.m_StereoScopic);

				Rect ourRect = EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginProperty(ourRect, GUIContent.none, projProp);
				EditorGUI.BeginProperty(ourRect, GUIContent.none, stereoProp);

				int value = (stereoProp.boolValue ? 1 : 0) * 2 + projProp.intValue;
				string[] enumNamesList = new []
				{
					EProjectionMode.Perspective.ToString(),
					EProjectionMode.Ortographic.ToString(),
					"Stereo " + EProjectionMode.Perspective.ToString(),
					"Stereo " + EProjectionMode.Ortographic.ToString(),
				};
				var newValue = EditorGUILayout.Popup("Projection", value, enumNamesList);
				if (newValue != value)
				{
					projProp.intValue = newValue & 1;
					stereoProp.boolValue = (newValue >> 1) != 0;
				}
				EditorGUI.EndProperty();
				EditorGUI.EndProperty();
				EditorGUILayout.EndHorizontal();

			}

			// Fake property: FOV
			{
				EditorGUILayout.BeginHorizontal();
				GUIContent gc;
				if (camObj.Model.m_ProjectionMode < EProjectionMode.Ortographic)
					gc = new GUIContent("Vertical FOV", "Vertical angular field of view of the camera.");
				else
					gc = new GUIContent("Vertical FOV", "Vertical field of view of the camera.");
				var orgValue = camObj.Model.VerticalFOV;
				var newValue = EditorGUILayout.FloatField(gc, orgValue);
				newValue = camObj.Model.Rules.ClampVerticalFOV(newValue);
				if (orgValue != newValue)
				{
					var lens = camModelObj.Lens;
					var flenProp = lensModel.FindPropertyRelative( () => lens.m_FocalLength);
					camObj.Model.VerticalFOV = newValue;
					flenProp.floatValue = lens.m_FocalLength;
				}

				EditorGUILayout.EndHorizontal();
			}

			var property = camModel.FindPropertyRelative( () => camModelObj.m_NearClippingPlane );
			AddFloatProperty(property, "Near clipping plane", "Distance, from camera sensor, to the Near clipping plane.", (oldv, newv) =>
			{
				if (newv < 0.01f) newv = 0.01f;
				var far = camModel.FindPropertyRelative( () => camModelObj.m_FarClippingPlane );
				if (far.floatValue - 0.01f < newv)
					far.floatValue = newv + 0.01f;
				return newv;
			});

			property = camModel.FindPropertyRelative( () => camModelObj.m_FarClippingPlane );
			AddFloatProperty(property, "Far clipping plane", "Distance, from camera sensor, to the Far clipping plane.", (oldv, newv) => {
				if (newv < camModelObj.m_NearClippingPlane  +0.01f) return camModelObj.m_NearClippingPlane + 0.01f;
				else return newv;
			});

			property = camModel.FindPropertyRelative( () => camModelObj.m_AutoFocus );
			AddBoolProperty(property, "Autofocus", "Does the camera auto focus? [For future use]");

			GUI.enabled = camObj.Model.Body.m_HDR;
			property = camModel.FindPropertyRelative( () => camModelObj.m_Exposure );
			AddFloatSlider( property, "Exposure", "Used for tonal mapping of HDR images. [For future use]", null, 1f, -10f, 10f);
			GUI.enabled = true;
		}

		private void DrawBodyModel(SerializedProperty bodyModel)
		{
			var camObj = serializedObject.targetObject as PhysicalCamera;
			var bodyModelObj = camObj.Model.Body;

			var property = bodyModel.FindPropertyRelative( ()=> bodyModelObj.m_SensorWidth);
			AddFloatProperty(property, "Sensor width", "Width, in millimeters, of the camera sensor.", (o, n) => n < 0.001f ? 0.001f : n > 0.1f ? 0.1f : n, 1000f);

			property = bodyModel.FindPropertyRelative( () => bodyModelObj.m_SensorHeight );
			AddFloatProperty(property, "Sensor height", "Height, in millimeters, of the camera sensor.", (o, n) => n < 0.001f ? 0.001f : n > 0.1f ? 0.1f : n, 1000f);

			// Fake property: Aspect Ratio
			{
				EditorGUILayout.BeginHorizontal();
				var orgValue = camObj.Model.Body.AspectRatio;
				var newValue = EditorGUILayout.FloatField(new GUIContent("Aspect ratio", "Aspect ratio of sensor: width over height"), orgValue);

				if (newValue < camObj.Model.Rules.MaxAspectRatio(camObj.Model))
					newValue = camObj.Model.Rules.MaxAspectRatio(camObj.Model);
				if (newValue > 20f)
					newValue = 20f;
				if (orgValue != newValue)
					camObj.Model.Body.AspectRatio = newValue;
				EditorGUILayout.EndHorizontal();
			}

			property = bodyModel.FindPropertyRelative( () => bodyModelObj.m_ShutterAngle);
			AddIntSlider(property, "Shutter angle", "Defines the �shutter speed�, driving the postFX motion blur parameter. [For future use]", null, 15, 360);

			property = bodyModel.FindPropertyRelative( () => bodyModelObj.m_ISO);
			AddIntProperty(property, "ISO", "A measure of the sensitivity of the image sensor. [For future use]", (o, n) => n < 25 ? 25 : n > 25600 ? 25600 : n);

			property = bodyModel.FindPropertyRelative( () => bodyModelObj.m_HDR);
			AddBoolProperty(property, "HDR", "Toggles extended dynamic range of generated images.");

			property = bodyModel.FindPropertyRelative( () => bodyModelObj.m_LensShiftX);
			AddFloatSlider(property, "Lens Shift X", "A displacement of the render frustum parallel to the image plane. Useful for perspective control architectural rendering. [For future use]", null, 1f, -1f, 1f);

			property = bodyModel.FindPropertyRelative( () => bodyModelObj.m_LensShiftY );
			AddFloatSlider(property, "Lens Shift Y", "A displacement of the render frustum parallel to the image plane.  Useful for perspective control architectural rendering. [For future use]", null, 1f, -1f, 1f);

			GUI.enabled = camObj.Model.m_ProjectionMode < EProjectionMode.Ortographic;
			{
				property = bodyModel.FindPropertyRelative( () => bodyModelObj.m_PerspectiveCorrection);
				AddFloatSlider(property, "Perspective Correction", "This rotates the render frustum on the x-axis to compensate for perpective distortion. [For future use]", null, 1f, -1f, 1f);
			}
			GUI.enabled = true;

		}

		private void DrawLensModel(SerializedProperty lensModel)
		{
			var camObj = serializedObject.targetObject as PhysicalCamera;
			var lensModelObj = camObj.Model.Lens;

			GUI.enabled = camObj.Model.m_ProjectionMode == EProjectionMode.Perspective;

			var property = lensModel.FindPropertyRelative( ()=> lensModelObj.m_FocalLength);
			AddFloatProperty(property, "Focal length", "Focal length of the lens in millimeters.", (o, n) =>
			{
				if (n < 0.001f) n = 0.001f;
				return n;
			}, 1000f);

			GUI.enabled = true;

			property = lensModel.FindPropertyRelative( () => lensModelObj.m_FStop );
			AddFloatSlider(property, "f-Stop", "Ratio of the lens's focal length to the diameter of the lens's entrance pupil. [For future use]", null, 1f, 0.7f, 64f);

			DrawOctaneModel(lensModel);
		}

		private void DrawOctaneModel(SerializedProperty lensModel)
		{
			var camObj = serializedObject.targetObject as PhysicalCamera;
			var lensModelObj = camObj.Model.Lens;

			EditorGUI.indentLevel++;

			// Octane
			m_showOctane = EditorGUILayout.Foldout(m_showOctane, "Octane parameters");
			if (!m_showOctane)
			{
				EditorGUI.indentLevel--;
				return;
			}

			var property = lensModel.FindPropertyRelative(() => lensModelObj.m_FocalDepth);
			AddFloatProperty(property, "Focal depth", "See Octane documentation.");

			property = lensModel.FindPropertyRelative(() => lensModelObj.m_Aperture);
			AddFloatProperty(property, "Aperture", "See Octane documentation.");

			property = lensModel.FindPropertyRelative(() => lensModelObj.m_ApertureAspectRatio);
			AddFloatProperty(property, "Aperture aspect ratio", "See Octane documentation.");

			property = lensModel.FindPropertyRelative(() => lensModelObj.m_ApertureEdge);
			AddFloatProperty(property, "Aperture edge", "See Octane documentation.");

			property = lensModel.FindPropertyRelative(() => lensModelObj.m_Distortion);
			AddFloatProperty(property, "Distortion", "See Octane documentation.");

			EditorGUI.indentLevel--;
		}

		private void DrawStereoModel(SerializedProperty stereoModel)
		{
			var camObj = serializedObject.targetObject as PhysicalCamera;
			var stereoModelObj = camObj.Model.Stereo;

			GUI.enabled = camObj.Model.m_StereoScopic;
			var property = stereoModel.FindPropertyRelative( ()=> stereoModelObj.m_Mode);
			AddEnumPopup(property, "Mode", "", typeof(EStereoscopicMode));

			property = stereoModel.FindPropertyRelative( () => stereoModelObj.m_EyeDistance );
			AddFloatSlider(property, "Eye Distance", "Distance seperating the eyes. [For future use]", null, 1, 0f, float.MaxValue);

			property = stereoModel.FindPropertyRelative( () => stereoModelObj.m_SwapEyes );
			AddBoolProperty(property, "Swap eyes", "[For future use]");

			property = stereoModel.FindPropertyRelative( () => stereoModelObj.m_LeftFilter );
			EditorGUILayout.PropertyField(property);

			property = stereoModel.FindPropertyRelative( () => stereoModelObj.m_RightFilter );
			EditorGUILayout.PropertyField(property);

			GUI.enabled = true;
		}

		private delegate T OnValueChangedDelegate<T>(T oldValue, T newValue);

		void AddEnumPopup(SerializedProperty porperty, string text, string tooltip, Type typeOfEnum, OnValueChangedDelegate<int> onChange  =null)
		{
			Rect ourRect = EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginProperty(ourRect, GUIContent.none, porperty);

			int selectionFromInspector = porperty.intValue;
			string[] enumNamesList = System.Enum.GetNames(typeOfEnum);
			var actualSelected = EditorGUILayout.Popup(text, selectionFromInspector, enumNamesList);
			if (onChange != null && actualSelected != porperty.intValue)
				actualSelected = onChange(porperty.intValue, actualSelected);
			porperty.intValue = actualSelected;
			EditorGUI.EndProperty();
			EditorGUILayout.EndHorizontal();
		}

		void AddFloatProperty(SerializedProperty porperty, string text, string tooltip, OnValueChangedDelegate<float> onChange = null, float factor = 1f)
		{
			Rect ourRect = EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginProperty(ourRect, GUIContent.none, porperty);

			var orgValue = porperty.floatValue * factor;
			var newValue = EditorGUILayout.FloatField(new GUIContent(text, tooltip), orgValue) / factor;
			if (onChange != null && orgValue != newValue)
				newValue = onChange(orgValue, newValue);
			porperty.floatValue = newValue;

			EditorGUI.EndProperty();
			EditorGUILayout.EndHorizontal();
		}

		void AddIntProperty(SerializedProperty porperty, string text, string tooltip, OnValueChangedDelegate<int> onChange = null)
		{
			Rect ourRect = EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginProperty(ourRect, GUIContent.none, porperty);

			var orgValue = porperty.intValue;
			var newValue = EditorGUILayout.IntField(new GUIContent(text, tooltip), orgValue);
			if (onChange != null && orgValue != newValue)
				newValue = onChange(orgValue, newValue);
			porperty.intValue = newValue;

			EditorGUI.EndProperty();
			EditorGUILayout.EndHorizontal();
		}

		void AddFloatSlider(SerializedProperty porperty, string text, string tooltip, OnValueChangedDelegate<float> onChange = null, float factor = 1f, float min = 0, float max = float.MaxValue)
		{
			Rect ourRect = EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginProperty(ourRect, GUIContent.none, porperty);

			var orgValue = porperty.floatValue * factor;
			var newValue = EditorGUILayout.Slider(new GUIContent(text, tooltip), orgValue, min * factor, max * factor) / factor;
			if (onChange != null && orgValue != newValue)
				newValue = onChange(orgValue, newValue);
			porperty.floatValue = newValue;

			EditorGUI.EndProperty();
			EditorGUILayout.EndHorizontal();
		}

		void AddIntSlider(SerializedProperty porperty, string text, string tooltip, OnValueChangedDelegate<int> onChange = null, int min = 0, int max = int.MaxValue)
		{
			Rect ourRect = EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginProperty(ourRect, GUIContent.none, porperty);

			var orgValue = porperty.intValue;
			var newValue = EditorGUILayout.IntSlider(new GUIContent(text, tooltip), orgValue, min, max);
			if (onChange != null && orgValue != newValue)
				newValue = onChange(orgValue, newValue);
			porperty.intValue = newValue;

			EditorGUI.EndProperty();
			EditorGUILayout.EndHorizontal();
		}

		void AddBoolProperty(SerializedProperty porperty, string text, string tooltip, OnValueChangedDelegate<bool> onChange = null)
		{
			Rect ourRect = EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginProperty(ourRect, GUIContent.none, porperty);

			var orgValue = porperty.boolValue;
			var newValue = EditorGUILayout.Toggle(new GUIContent(text, tooltip), orgValue);
			if (onChange != null && orgValue != newValue)
				newValue = onChange(orgValue, newValue);
			porperty.boolValue = newValue;

			EditorGUI.EndProperty();
			EditorGUILayout.EndHorizontal();
		}

	}
}

using System;
using UnityEngine;

namespace Unity.Vfx.Cameras.Model
{
	public enum EProjectionMode
	{
		Perspective,
		Ortographic,
	}

	[System.Serializable]
	public class PhysicalCameraModel
	{
		private MathematicalModel m_Rules;

		public EProjectionMode m_ProjectionMode;
		public bool m_StereoScopic;
		public float m_NearClippingPlane;
		public float m_FarClippingPlane;
		public bool m_AutoFocus;
		public float m_Exposure;

		[SerializeField] private PhysicalCameraBodyModel m_Body;
		[SerializeField] private PhysicalCameraLensModel m_Lens;
		[SerializeField] private StereoPhysicalCameraModel m_Stereo;

		public PhysicalCameraBodyModel Body {
			get { return m_Body ?? (m_Body = new PhysicalCameraBodyModel()); }
		}
		public PhysicalCameraLensModel Lens
		{
			get { return m_Lens ?? (m_Lens = new PhysicalCameraLensModel()); }
		}
		public StereoPhysicalCameraModel Stereo
		{
			get { return m_Stereo?? (m_Stereo = new StereoPhysicalCameraModel()); }
		}

		public MathematicalModel Rules
		{
			get { return m_Rules ?? (m_Rules = new MathematicalModel()); } 
		}

		public float VerticalFOV
		{
			get { return Rules.VerticalFOV( this ); }
			set
			{
				Rules.ApplyVerticalFOV(value, this);
			}
		}

		public void SetDefaultValues()
		{
			m_ProjectionMode = EProjectionMode.Perspective;
			m_StereoScopic = false;
			m_NearClippingPlane = 0.03f;
			m_FarClippingPlane = 1000f;

			m_AutoFocus = false;
			m_Exposure = 0.0f;

			Body.SetupDefaultValues();
			Lens.SetupDefaultValues();
			Stereo.SetupDefaultValues();
		}

		public bool IsValid()
		{
			return m_Body.IsValid() && m_Lens.IsValid();
		}

		public PhysicalCameraModel()
		{
			SetDefaultValues();
		}
	}
}

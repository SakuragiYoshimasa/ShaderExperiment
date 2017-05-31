using UnityEngine;

namespace Unity.Vfx.Cameras.Model
{

	[System.Serializable]
	public class PhysicalCameraLensModel
	{
		public float m_FocalLength;
		public float m_FStop;
		public float m_Aperture; 
		public float m_ApertureAspectRatio;
		public float m_ApertureEdge;
		public float m_Distortion;
		public float m_FocalDepth;

		public void SetupDefaultValues()
		{
			m_FocalLength = 24/1000f;
			m_FStop = 2.8f;
			m_ApertureAspectRatio = 1f;
			m_ApertureEdge = 1f;
			m_Distortion = 0f;
		}

		public bool IsValid()
		{
			return m_FStop > 0 && m_FocalLength > 0;
		}

		public PhysicalCameraLensModel()
		{
			SetupDefaultValues();
		}
	}
}

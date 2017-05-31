using UnityEngine;

namespace Unity.Vfx.Cameras.Model
{

	public enum EStereoscopicMode
	{
		Offaxis,
	}

	[System.Serializable]
	public class StereoPhysicalCameraModel
	{
		public EStereoscopicMode m_Mode;
		public float m_EyeDistance;
		public bool m_SwapEyes;

		[Tooltip("Color tint used as filter for left eye. [For future use]")]
		public Color m_LeftFilter;

		[Tooltip("Color tint used as filter for right eye. [For future use]")]
		public Color m_RightFilter;

		public StereoPhysicalCameraModel()
		{
			SetupDefaultValues();
		}

		public void SetupDefaultValues()
		{
			
		}
	}
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExperimentUtilities {

	public class MaterialFuncs  {

		public static Material CreateMaterial(Shader shader){
			var material = new Material(shader);
			material.hideFlags = HideFlags.DontSave;
			return material;
		}
	}
}
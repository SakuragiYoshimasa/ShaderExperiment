using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Utility {
	public class MenuItems : Editor {

		[MenuItem("Utility/ExportGif")]
		public static void showTwitterPallet(){
			CaptureImagePalltet.ShowPallet ();


		}
	}
}
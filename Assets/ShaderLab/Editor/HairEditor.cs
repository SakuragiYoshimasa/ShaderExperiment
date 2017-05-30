using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace ShaderLab{
	public class HairEditor : Editor {

			[MenuItem("Assets/Create/Hair")]
			static void CreteHair()
			{
				// Make a proper path from the current selection.
				var path = AssetDatabase.GetAssetPath(Selection.activeObject);
				if (string.IsNullOrEmpty(path))
					path = "Assets";
				else if (Path.GetExtension(path) != "")
					path = path.Replace(Path.GetFileName(path), "");
				var assetPathName = AssetDatabase.GenerateUniqueAssetPath(path + "/Hair.asset");

				// Create a tube asset.
				var asset = ScriptableObject.CreateInstance<Hair>();
				AssetDatabase.CreateAsset(asset, assetPathName);
				AssetDatabase.AddObjectToAsset(asset.mesh, asset);

				// Build an initial mesh for the asset.
				asset.RebuildMesh();

				// Save the generated mesh asset.
				AssetDatabase.SaveAssets();

				// Tweak the selection.
				EditorUtility.FocusProjectWindow();
				Selection.activeObject = asset;
			}
	}
}
using UnityEngine;

namespace ShaderLab{
	public class MeshTest : MonoBehaviour {

		[SerializeField] SkinnedMeshRenderer renderer;
		[SerializeField] AnimatedSkinnedMesh asm;
		[SerializeField] Material _mat;
		Mesh mesh;

		void Update () {
			mesh = renderer.sharedMesh;
			_mat.SetTexture("_PositionTex", asm.CurrPositionBuffer);
			Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, _mat, 0);
		}
	}
}
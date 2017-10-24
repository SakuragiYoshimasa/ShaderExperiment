using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RockParticles {
	public class RockParticle : MonoBehaviour {

		//[SerializeField]
		//private List<Mesh> particleMeshes;

		[SerializeField]
		private List<GameObject> particles;

		[SerializeField]
		private List<Vector3> rotates;

		[SerializeField]
		private List<float> speeds;

		[SerializeField]
		private int rockNum = 1;

		[SerializeField]
		private Material mat;

		[SerializeField]
		private float scale = 2.0f;

		[SerializeField]
		private float speedOffset;
		[SerializeField]
		private float speedPow;
		[SerializeField]
		private float rotatePow;


		void Start () {
			GenerateParticle ();
		}

		void Update () {
			for(int i = 0; i < rockNum; i++){
				particles [i].transform.Rotate (rotates[i]);
				particles [i].transform.localPosition += new Vector3 (0, speeds[i] * Time.deltaTime, 0);
				if(particles [i].transform.localPosition.y > 150.0f){
					particles [i].transform.localPosition = new Vector3 (250.0f * UnityEngine.Random.value - 125.0f, -100.0f, 250.0f * UnityEngine.Random.value - 125.0f);
				}
			}
		}

		void GenerateParticle(){


			particles = new List<GameObject> (0);
			rotates = new List<Vector3> (0);
			speeds = new List<float> (0);

			for(int i = 0; i < rockNum; i++){
				GameObject go = new GameObject ();
				MeshRenderer renderer =  go.AddComponent<MeshRenderer> ();
				MeshFilter fileter = go.AddComponent<MeshFilter> ();

				Mesh mesh = new Mesh ();
				List<Vector3> vecs = new List<Vector3>(0);

				vecs.Add (new Vector3 ( UnityEngine.Random.value * scale * 2.0f - scale, 1.0f + UnityEngine.Random.value * scale * 2.0f - scale, 1.0f +  UnityEngine.Random.value * 6.0f));
				vecs.Add (new Vector3 (-scale * UnityEngine.Random.value, -scale * UnityEngine.Random.value));
				vecs.Add (new Vector3 (-scale * UnityEngine.Random.value,  scale * UnityEngine.Random.value));
				vecs.Add (new Vector3 ( scale * UnityEngine.Random.value,  scale * UnityEngine.Random.value));
				vecs.Add (new Vector3 ( scale * UnityEngine.Random.value, -scale * UnityEngine.Random.value));
				vecs.Add (new Vector3 ( UnityEngine.Random.value * scale * 2.0f - scale, 1.0f + UnityEngine.Random.value * scale * 2.0f - scale, -1.0f + UnityEngine.Random.value * -6.0f));

				int[] indices = new int[24]{
					0, 1, 2,
					0, 2, 3,
					0, 3, 4,
					0, 4, 1,
					5, 2, 1,
					5, 3, 2,
					5, 4, 3,
					5, 1, 4
				};

				List<Vector2> uvs = new List<Vector2>(0);
		
				uvs.Add (new Vector2(0, 0));
				uvs.Add (new Vector2(0, 1.0f));
				uvs.Add (new Vector2(1.0f, 1.0f));
				uvs.Add (new Vector2(2.0f, 1.0f));
				uvs.Add (new Vector2(3.0f, 1.0f));
				uvs.Add (new Vector2(0f, 2.0f));

				mesh.Clear ();
				mesh.SetVertices (vecs);
				mesh.SetIndices (indices, MeshTopology.Triangles, 0);
				mesh.SetUVs (0, uvs);

				fileter.sharedMesh = mesh;
				fileter.sharedMesh.name = "rockMesh";
				renderer.material = mat;

				mesh.RecalculateNormals ();
				mesh.RecalculateBounds ();

				go.name = "rock" + i.ToString();
				go.transform.parent = this.transform;
				go.transform.localPosition = new Vector3 (250.0f * UnityEngine.Random.value - 125.0f, 250.0f * UnityEngine.Random.value - 125.0f, 250.0f * UnityEngine.Random.value - 125.0f);
			
				particles.Add (go);
				speeds.Add (UnityEngine.Random.value * speedPow + speedOffset);
				rotates.Add (new Vector3(
					(UnityEngine.Random.value - 0.5f) * rotatePow,
					(UnityEngine.Random.value - 0.5f) * rotatePow,
					(UnityEngine.Random.value - 0.5f) * rotatePow
				));
			}
		}
	}
}
using System.Collections.Generic;
using UnityEngine;

namespace ShaderLab {

	public class Hair : ScriptableObject {
		[SerializeField] public int divisions = 4;
		[SerializeField] public int segments = 32;
		[SerializeField] public Mesh mesh;

		public void RebuildMesh(){
			List<Vector3> vertices = new List<Vector3>();

			for(int i = 0; i < segments + 1; i++){
				for(int j = 0; j < divisions + 1; j++){
					float phai = Mathf.PI * 2.0f * (float)j / (float)divisions;
					vertices.Add(new Vector3(phai, 0, i));
				}
			}

			List<int> indices = new List<int>();
			int refi = 0;

			 for (var i = 0; i < segments; i++){
                for (var j = 0; j < divisions; j++){
                    indices.Add(refi);
                    indices.Add(refi + 1);
                    indices.Add(refi + 1 + divisions);

                    indices.Add(refi + 1);
                    indices.Add(refi + 2 + divisions);
                    indices.Add(refi + 1 + divisions);

                    refi++;
                }
                refi++;
            }

            // Reset the mesh asset.
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            mesh.UploadMeshData(true);
		}

		void OnEnable(){
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.name = "Hair";
            }
        }
	}
}

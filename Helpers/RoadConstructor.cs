#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace unab
{
    [RequireComponent(typeof(LineRenderer))]
    public class RoadConstructor : MonoBehaviour
    {
        public float lineWidth = 0f;
        public int vertexCount = 0;
        public float radius = 0f;

        private LineRenderer m_lineRenderer;
        private Vector3 m_position;

        // Start is called before the first frame update
        void Start()
        {
            m_lineRenderer = GetComponent<LineRenderer>();
            m_lineRenderer.alignment = LineAlignment.TransformZ;
            m_lineRenderer.startWidth = lineWidth;
            m_lineRenderer.endWidth = lineWidth;

            m_position = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.N))
            {
                SaveMesh();
            }

            if (Input.GetKey(KeyCode.R))
            {
                DrawPolygon(vertexCount, radius, m_position);
            }
        }

        private void DrawPolygon(int vertex, float radius, Vector3 center)
        {
            if (m_lineRenderer.positionCount > 0) m_lineRenderer.positionCount = 0;

            m_lineRenderer.loop = true;
            m_lineRenderer.positionCount = vertex;

            float angle = 2 * Mathf.PI / vertex;

            for (int i = 0; i < vertex; i++)
            {
                Matrix4x4 rotationMatrix = new Matrix4x4(
                    new Vector4(Mathf.Cos(angle * i), Mathf.Sin(angle * i), 0, 0),
                    new Vector4(-Mathf.Sin(angle * i), -Mathf.Cos(angle * i), 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1)
                    );

                Vector3 initialRelativePosition = new Vector3(0, radius, 0);

                m_lineRenderer.SetPosition(i, center + rotationMatrix.MultiplyPoint(initialRelativePosition));
            }
        }

        private void SaveMesh()
        {
            var mesh = new Mesh();
            mesh.name = transform.name + "Mesh";
            m_lineRenderer.BakeMesh(mesh, true);

            var mf = GetComponent<MeshCollider>();
            mf.sharedMesh = mesh;

            transform.localEulerAngles = new Vector3(90f, 0, 0);

            if (mf)
            {
                var savePath = "Assets/LineMesh.asset";

                AssetDatabase.CreateAsset(mf.sharedMesh, savePath);
            }
        }
    }
}
#endif
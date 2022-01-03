using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace unab
{
    public class SensorColorController : MonoBehaviour
    {
        public Material material;

        public Color followedColor;
        public Color detectedColor;
        public float sensorValue = 0f;
        public bool isLineDetected = false;

        private RaycastHit m_hit;

        // Start is called before the first frame update
        void Start()
        {
            followedColor = material.color;
            followedColor.a = 1.0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out m_hit))
            {
                detectedColor = m_hit.collider.GetComponent<Renderer>().material.color;
                detectedColor.a = 1.0f;

                isLineDetected = detectedColor == followedColor;
            }
        }
    }
}
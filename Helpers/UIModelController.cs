using UnityEngine;
using UnityEngine.SceneManagement;

namespace unab
{
    public class UIModelController : MonoBehaviour
    {        
        public string nextScene;
        public float rotationSpeed = .25f;

        private Vector3 m_originalPosition;
        private Quaternion m_originalRotation;

        

        private void Start()
        {
            m_originalPosition = transform.localPosition;
            m_originalRotation = transform.localRotation;            
        }        

        private void Update()
        {
            if (Input.GetKey(KeyCode.R))
            {
                transform.localPosition = m_originalPosition;
                transform.localRotation = m_originalRotation;
            }

            if (Input.GetKey(KeyCode.C))
            {
                SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
            }
        }

        private void OnMouseDrag()
        {            
            float rotation_x =  Input.GetAxis("Mouse X") * rotationSpeed * Mathf.Rad2Deg;
            float rotation_y = Input.GetAxis("Mouse Y") * rotationSpeed * Mathf.Rad2Deg;

            transform.Rotate(Vector3.forward, -rotation_y);
            transform.Rotate(Vector3.right, -rotation_x);
        }       
    }
}
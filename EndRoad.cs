using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace unab
{
    public class EndRoad : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                other.gameObject.GetComponent<LineFollowerController>().Reset();
            }
        }
    }
}
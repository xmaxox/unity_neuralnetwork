using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace unab
{
    public class SplashScreenAnimation : MonoBehaviour
    {
        public Image logo;
        public float animation_time = 1.5f;
        public AnimationClip logo_animation;

        private Animator m_animator;

        // Start is called before the first frame update
        void Start()
        {
            m_animator = GetComponent<Animator>();

            StartCoroutine(Wait(animation_time));
        }

        IEnumerator Wait(float time)
        {
            yield return new WaitForSeconds(1f);

            m_animator.SetBool("isStarting", false);

            yield return new WaitForSeconds(time);

            SceneManager.LoadScene("intro_screen");
        }
    }
}
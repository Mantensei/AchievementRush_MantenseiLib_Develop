using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;

namespace MantenseiLib
{
    public class Tes01 : MonoBehaviour
    {
        [GetComponent]
        Animation2DRegisterer registerer;

        [SerializeField]
        GameObject TesTes;
        [SerializeField]
        GameObject TesTes2;

        private void Start()
        {
            Debug.Log(registerer);
            registerer?.Play();

            if(registerer != null)
            {
                registerer.Animator.transform.position = Random.Range(-3f, 3f) * Vector3.one;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log
                (
                    registerer.Play()
                );

                Instantiate(TesTes);
                Instantiate(TesTes2);

                SceneManager.LoadScene("TesTes");
            }
        }
    } 
}

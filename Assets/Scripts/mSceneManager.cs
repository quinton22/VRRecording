using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mSceneManager : MonoBehaviour
{
    [System.Serializable]
    private struct Activation
    {
        [SerializeField]
        GameObject obj;
        [SerializeField]
        bool setActive;
        public void Set()
        {
            obj.SetActive(setActive);
        }
        
    }

    [System.NonSerialized]
    public bool isARActive;
    [SerializeField]
    private Activation[] m_Activations;
    void Awake()
    {
        isARActive = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
        if (isARActive)
        {
            // load AR scene
            foreach (Activation activation in m_Activations)
            {
                activation.Set();
            }
        }
    }
}

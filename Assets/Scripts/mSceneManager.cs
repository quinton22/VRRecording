using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class mSceneManager : MonoBehaviour
{
    [System.Flags]
    private enum PlatformType
    {
        AR,
        Desktop,
        VR
    }

    [System.Serializable]
    private struct Activation
    {
        [SerializeField]
        GameObject obj;

        [SerializeField]
        [EnumFlagsAttribute]
        private PlatformType activePlatforms;

        public void Set(PlatformType currentPlatform)
        {
            obj.SetActive(ReturnSelectedElements().Contains((int) currentPlatform));
        }

        List<int> ReturnSelectedElements()
        {

            List<int> selectedElements = new List<int>();
            for (int i = 0; i < System.Enum.GetValues(typeof(PlatformType)).Length; i++)
            {
                int layer = 1 << i;
                if (((int)activePlatforms & layer) != 0)
                {
                    selectedElements.Add(i);
                }
            }

            return selectedElements;
        }
    }

    [System.NonSerialized]
    public bool isARActive;
    [System.NonSerialized]
    public bool isVRActive;
    [SerializeField]
    private Activation[] m_Activations;
    void Awake()
    {
        isARActive = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
        isVRActive = XRDevice.isPresent;

        PlatformType platformType = isARActive ? PlatformType.AR : (isVRActive ? PlatformType.VR : PlatformType.Desktop);

        Debug.Log($"Platform Type: {platformType.ToString()}");
        
        // activate correct objects
        foreach (Activation activation in m_Activations)
        {
            activation.Set(platformType);
        }
        
    }
}

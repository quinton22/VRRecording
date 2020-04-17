using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class RecordingController : MonoBehaviour
{

    public enum RecordMode {
        On,
        Off
    }

    public RecordMode m_RecordMode;
    //public List<GameObject> m_RecordedObjects;
    //public List<RecordedObjectController> m_RecordedObjectControllers;
    public string m_TextFileLocation;
    [SerializeField]
    private GameObject m_DisplayCanvas;
    private List<RecordedObjectController> m_RecordedObjectControllers;
    private Vector3 m_PosOffset;
    private Quaternion m_RotOffset;
    private Vector3 m_ScaleOffset;

    void Awake()
    {
        ResetRecordingController();

        if (m_RecordMode == RecordMode.Off && m_DisplayCanvas != null)
        {
            m_DisplayCanvas.transform.Find("RecordingImage").GetComponent<Image>().enabled = false;
        }
    }

    public void ResetRecordingController()
    {
        List<GameObject> m_RecordedObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("RecordedObject"));
        m_RecordedObjectControllers = m_RecordedObjects.Select((GameObject obj) => obj.GetComponent<RecordedObjectController>()).ToList();

        // set the m_RecordingController for every object
        foreach (RecordedObjectController roc in m_RecordedObjectControllers)
        {
            roc.SetRecordingController(this);

            roc.m_RecordMode = m_RecordMode;
            roc.textFileLocation = m_TextFileLocation;
        }
    }

    public void SetOffsets(Vector3 posOffset, Quaternion rotOffset, Vector3 scaleOffset)
    {
        m_PosOffset = posOffset;
        m_RotOffset = rotOffset;
        m_ScaleOffset = scaleOffset;        
    }

    public void SetChildOffsets()
    {
        foreach (RecordedObjectController roc in m_RecordedObjectControllers)
        {
            roc.SetOffsets(m_PosOffset, m_RotOffset, m_ScaleOffset);
        }
    }
}

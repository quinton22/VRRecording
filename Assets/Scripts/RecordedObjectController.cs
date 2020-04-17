using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

// This class will record an objects position, scale, and rotation
// It will also replay a recording depending on a switch
public class RecordedObjectController : MonoBehaviour
{
    public class DataWrapper
    {
        public float? m_TimeStep = null;
        public List<Vector3> m_PositionList = new List<Vector3>();
        public List<Vector3> m_ScaleList = new List<Vector3>();
        public List<Vector3> m_RotationList = new List<Vector3>();

        // produces a json string
        public string Serialize()
        {
            string str = "{";
            str += $"'TimeStep':{m_TimeStep}\n";
            str += $"'PositionList':{ListToString(m_PositionList)}\n";
            str += $"'ScaleList':{ListToString(m_ScaleList)}\n";
            str += $"'RotationList':{ListToString(m_RotationList)}";
            str += "}";
            
            return str;
        }

        // creates object from json
        public void Deserialize(string json)
        {
            json = json.Trim();
            json = json.Trim(new Char[]{'{', '}'});

            string[] jsonArray = json.Split('\n');

            foreach (string obj in jsonArray)
            {
                string value = obj.Split(':')[1];

                if (obj.Contains("TimeStep"))
                {
                    m_TimeStep = float.Parse(value);
                }
                else if (obj.Contains("Position"))
                {
                    m_PositionList = StringToList(value);
                }
                else if (obj.Contains("Scale"))
                {
                    m_ScaleList = StringToList(value);
                }
                else if (obj.Contains("Rotation"))
                {
                    m_RotationList = StringToList(value);
                }
            }
        }

        private string ListToString<T>(List<T> l)
        {
            return "[" + string.Join(";", l.Select(i => i.ToString())) + "]";
        }

        private List<Vector3> StringToList(string s)
        {
            List<Vector3> l = new List<Vector3>();

            s = s.Substring(1, s.Length - 2);
            string[] objArr = s.Split(';');
            foreach (string obj in objArr)
            {
                l.Add(StringToObject(obj));
            }

            return l;
        }

        private Vector3 StringToObject(string s)
        {
            int index = s.IndexOf("(");

            s = s.Substring(index + 1, s.Length - 2);
            float[] p = (new List<string>(s.Split(','))).Select(i => float.Parse(i.Trim())).ToArray();
            return new Vector3(p[0], p[1], p[2]);
        }
    }

    private bool m_HasRecordingControllerBeenSet = false;
    private RecordingController m_RecordingController;
    public DataWrapper m_DataWrapper = new DataWrapper();
    [HideInInspector]
    public RecordingController.RecordMode m_RecordMode;
    [HideInInspector]
    public string textFileLocation;
    public TextAsset m_TextFile;
    private bool isRecording;
    private int playbackIndex = 0;
    private Vector3 m_PosOffset;
    private Quaternion m_RotOffset;
    private Vector3 m_ScaleOffset;

    public void SetRecordingController(RecordingController value)
    {
        if (!m_HasRecordingControllerBeenSet)
        {
            m_RecordingController = value;
            m_HasRecordingControllerBeenSet = true;
        }
    }

    public void SetOffsets(Vector3 posOffset, Quaternion rotOffset, Vector3 scaleOffset)
    {
        m_PosOffset = posOffset;
        m_RotOffset = rotOffset;
        m_ScaleOffset = scaleOffset;
    }
    string GetFile()
    {
        if (m_TextFile == null)
        {
            return System.IO.File.ReadAllText(textFileLocation + gameObject.name + ".txt");
        }
        else
        {
            return m_TextFile.text;
        }
    }

    void Start()
    {
        isRecording = m_RecordMode == RecordingController.RecordMode.On;

        if (!isRecording)
        {
            Logger.Log("Start");

            Deserialize(GetFile());
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            // TODO: turn off collider on object
        }
    }

    void OnDestroy()
    {
        if (isRecording)
            System.IO.File.WriteAllText(textFileLocation + gameObject.name + ".txt", Serialize());
    }

    // This is better because it does not depend on frame rate... but (TODO) it may cause some perceived lag
    // NOTE: The user should change the timestep in ProjectSettings > Time to 1/90 for VR
    // TODO: this can be avoided by recording in FixedUpdate and playing back in Update but lerping based on the timestep
    void FixedUpdate()
    {
        if (isRecording)
        {
            Record();
        }
        else 
        {
            Playback();
        }
    }

    void Record()
    {
        if (m_DataWrapper.m_TimeStep == null)
        {
            m_DataWrapper.m_TimeStep = Time.deltaTime;
        }

        m_DataWrapper.m_PositionList.Add(transform.position);
        m_DataWrapper.m_ScaleList.Add(transform.localScale);
        m_DataWrapper.m_RotationList.Add(transform.rotation.eulerAngles);
    }

    void Playback()
    {
        if (playbackIndex >= m_DataWrapper.m_PositionList.Count) playbackIndex = 0;

        transform.position = m_DataWrapper.m_PositionList[playbackIndex] + m_PosOffset;
        transform.localScale = m_DataWrapper.m_ScaleList[playbackIndex];
        transform.rotation = Quaternion.Euler(m_DataWrapper.m_RotationList[playbackIndex] + m_RotOffset.eulerAngles);

        ++playbackIndex;
    }

    public string Serialize()
    {
       return m_DataWrapper.Serialize();
    }

    public void Deserialize(string json)
    {
        m_DataWrapper.Deserialize(json);
    }
}

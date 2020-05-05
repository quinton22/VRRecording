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
    public RecordingController.RecordingState m_RecordingState;
    [HideInInspector]
    public bool m_PlaybackInFixedUpdate;
    [HideInInspector]
    public bool m_RestartPlaybackOnFinish;
    [HideInInspector]
    public string textFileLocation;
    public TextAsset m_TextFile;
    private bool isRecording;
    private int playbackIndex = 0;
    private float elapsedTime = 0;
    private Vector3 m_PosOffset;
    private Quaternion m_RotOffset;
    private Vector3 m_ScaleOffset;
    private Vector3 previousPosition;
    private Vector3 previousScale;
    private Quaternion previousRotation;
    private bool isPreviousTransformSet = false;

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

            //if (m_PlaybackInFixedUpdate) // objects act weird if not kinematic
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            // TODO: turn off collider on object
        }
    }

    void OnDestroy()
    {
        // TODO: make sure we are writing every time
        if (isRecording)
            WriteToTextFile();
    }

    void WriteToTextFile()
    {
        System.IO.File.WriteAllText(textFileLocation + gameObject.name + ".txt", Serialize());
    }

    // This is better because it does not depend on frame rate... but (TODO) it may cause some perceived lag
    // NOTE: The user should change the timestep in ProjectSettings > Time to 1/90 for VR
    // TODO: this can be avoided by recording in FixedUpdate and playing back in Update but lerping based on the timestep
    void FixedUpdate()
    {
        if (isRecording)
        {
            // TODO: what is the proper functionality when pausing while recording?
            // should objects freeze? can the player interact with them? should they have gravity?
            if (m_RecordingState == RecordingController.RecordingState.Paused || m_RecordingState == RecordingController.RecordingState.Stopped)
            {

            }
            else
            {
                StopMaintainingTransform();
                Record();
            }
        }
        else if (m_PlaybackInFixedUpdate)
        {
            PlaybackAndMaintain(Playback);
        }
    }

    void Update()
    {
        if (!isRecording && !m_PlaybackInFixedUpdate) // playback mode && not fixedupdate
        {
           PlaybackAndMaintain(LerpPlayback);
        }
    }

    void MaintainTransform()
    {
        if (!isPreviousTransformSet)
        {
            previousPosition = transform.position;
            previousScale = transform.localScale;
            previousRotation = transform.rotation;
            isPreviousTransformSet = true;
        }

        transform.position = previousPosition;
        transform.localScale = previousScale;
        transform.rotation = previousRotation;
    }

    void StopMaintainingTransform()
    {
        if (isPreviousTransformSet) isPreviousTransformSet = false;
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

    void PlaybackAndMaintain(Action playbackFunction)
    {
        // maintain position if paused or stopped
        if (m_RecordingState == RecordingController.RecordingState.Paused || m_RecordingState == RecordingController.RecordingState.Stopped)
        {
            MaintainTransform();
        }
        else
        {
            StopMaintainingTransform();
            playbackFunction();
        }
    }

    void Playback()
    {
        if (playbackIndex >= m_DataWrapper.m_PositionList.Count) 
        {
            if (m_RestartPlaybackOnFinish)
                playbackIndex = 0; // start over
            else
                playbackIndex--; // play last frame
        }

        transform.position = m_DataWrapper.m_PositionList[playbackIndex] + m_PosOffset;
        transform.localScale = m_DataWrapper.m_ScaleList[playbackIndex];
        transform.rotation = Quaternion.Euler(m_DataWrapper.m_RotationList[playbackIndex] + m_RotOffset.eulerAngles);

        ++playbackIndex;
    }

    void LerpPlayback()
    {
        if (playbackIndex >= m_DataWrapper.m_PositionList.Count - 1)
        {
            if (m_RestartPlaybackOnFinish)
            {
                elapsedTime = 0;
                playbackIndex = 0; // start over
            }
            else
            {
                elapsedTime -= Time.deltaTime;
                playbackIndex--; // play last frame
            }
        }

        transform.position = Vector3.Lerp(
                m_DataWrapper.m_PositionList[playbackIndex],
                m_DataWrapper.m_PositionList[playbackIndex+1],
                elapsedTime-playbackIndex
            ) + m_PosOffset;
        transform.localScale = Vector3.Lerp(
                m_DataWrapper.m_ScaleList[playbackIndex],
                m_DataWrapper.m_ScaleList[playbackIndex+1],
                elapsedTime-playbackIndex
            );
        transform.rotation = Quaternion.Lerp(
                Quaternion.Euler(m_DataWrapper.m_RotationList[playbackIndex] + m_RotOffset.eulerAngles),
                Quaternion.Euler(m_DataWrapper.m_RotationList[playbackIndex+1] + m_RotOffset.eulerAngles),
                elapsedTime-playbackIndex
            );

        elapsedTime+=Time.deltaTime;
        playbackIndex=(int) Mathf.Floor(elapsedTime / (float) m_DataWrapper.m_TimeStep);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class RecordingController : MonoBehaviour
{

    public enum RecordMode {
        On,
        Off
    }

    public enum RecordingState {
        Playing,
        Paused,
        Stopped
    }

    public RecordMode m_RecordMode;
    public RecordingState m_RecordingStartState = RecordingState.Playing;
    [HideInInspector]
    public RecordingState m_RecordingState;
    //public List<GameObject> m_RecordedObjects;
    //public List<RecordedObjectController> m_RecordedObjectControllers;
    public string m_TextFileLocation;
    [SerializeField]
    private GameObject m_DisplayCanvas;
    [SerializeField, Tooltip("Action that toggles between playing and pausing the recording")]
    private SteamVR_Action_Boolean m_PlayPauseAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PlayPauseRecording");
    [SerializeField, Tooltip("Action that stops the recording")]
    private SteamVR_Action_Boolean m_StopAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("StopRecording");
    [SerializeField, Tooltip("Specify whether playback occurs in the FixedUpdate function or Update function")]
    private bool m_PlaybackInFixedUpdate = false;
    [SerializeField, Tooltip("Should playback restart when finished")]
    private bool m_RestartPlaybackOnFinish = true;
    private List<RecordedObjectController> m_RecordedObjectControllers;
    private Vector3 m_PosOffset;
    private Quaternion m_RotOffset;
    private Vector3 m_ScaleOffset;

    void Awake()
    {
        if (m_RecordingStartState == RecordingState.Stopped) m_RecordingStartState = RecordingState.Paused;
        m_RecordingState = m_RecordingStartState;

        ResetRecordingController();

        if (m_RecordMode == RecordMode.Off && m_DisplayCanvas != null)
        {
            m_DisplayCanvas.transform.Find("RecordingImage").GetComponent<Image>().enabled = false;
        }

        if (m_RecordMode == RecordMode.On)
        {
            m_PlayPauseAction.AddOnStateDownListener(PlayPauseAction, SteamVR_Input_Sources.Any);
            m_StopAction.AddOnStateDownListener(StopAction, SteamVR_Input_Sources.Any);
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
            roc.m_PlaybackInFixedUpdate = m_PlaybackInFixedUpdate;
            roc.m_RestartPlaybackOnFinish = m_RestartPlaybackOnFinish;
            roc.m_RecordingState = m_RecordingStartState;
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

    private void SetRecordingState(RecordingState rs)
    {
        m_RecordingState = rs;
        foreach (RecordedObjectController roc in m_RecordedObjectControllers)
        {
            roc.m_RecordingState = m_RecordingState;
        }
    }

    private void PlayPauseAction(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (m_RecordMode == RecordMode.On)
        {
            if (m_RecordingState == RecordingState.Playing)
                SetRecordingState(RecordingState.Paused);
            else if (m_RecordingState == RecordingState.Paused)
                SetRecordingState(RecordingState.Playing);
        }
        else
        {
            // even if stopped in playback, then we can still resume
            // basically 'stopped' while in playback mode is equivalent to 'paused'
            SetRecordingState(m_RecordingState == RecordingState.Playing ? RecordingState.Paused : RecordingState.Playing);
        }
        
    }

    private void StopAction(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        SetRecordingState(RecordingState.Stopped);
    }
}

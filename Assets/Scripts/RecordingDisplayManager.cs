using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordingDisplayManager : MonoBehaviour
{
    [SerializeField]
    private Camera m_Camera;
    [SerializeField]
    private bool m_AlwaysFaceUser = true;
    [SerializeField]
    private float m_CanvasOffset = .3f;
    [SerializeField]
    private Image m_RecordingImage;
    [SerializeField]
    private Image m_PlayingImage;
    [SerializeField]
    private Image m_PausedImage;
    [SerializeField]
    private Image m_StoppedImage;
    private Canvas m_Canvas;
    private RecordingController m_RecordingController;


    void Awake()
    {
        m_Canvas = GetComponentInChildren<Canvas>();

        m_RecordingController = FindObjectOfType<RecordingController>();
    }

    void Update()
    {
        if (m_AlwaysFaceUser)
            FaceUser();

        DisplayMode(); // TODO: maybe move to start function
        DisplayCurrentState();
    }

    void FaceUser()
    {
        m_Canvas.transform.LookAt(m_Camera.transform);
        m_Canvas.transform.localPosition = m_Canvas.transform.forward * m_CanvasOffset;
    }

    void DisplayMode()
    {
        // Displays red dot if recording, and nothing if not
        m_RecordingImage.enabled = m_RecordingController.m_RecordMode == RecordingController.RecordMode.On;
    }

    void DisplayCurrentState()
    {
        // Displays play, pause, or stop images
        m_PlayingImage.enabled = m_RecordingController.m_RecordingState == RecordingController.RecordingState.Playing;
        m_PausedImage.enabled = m_RecordingController.m_RecordingState == RecordingController.RecordingState.Paused;
        m_StoppedImage.enabled = m_RecordingController.m_RecordingState == RecordingController.RecordingState.Stopped;
    }
}

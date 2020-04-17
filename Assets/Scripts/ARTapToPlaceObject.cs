using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ARTapToPlaceObject : MonoBehaviour
{
    // private GraphicRaycaster m_GraphicRaycaster;
    private ARSessionOrigin m_AROrigin;
    private ARRaycastManager m_ARRaycastManager;
    private Pose m_PlacementPose;
    private bool isPlacementPoseValid = false;
    private bool shouldPlace = true;
    [SerializeField]
    private GameObject m_ObjectToPlace;
    public GameObject m_PlacementIndicator;
    [SerializeField]
    private GameObject m_Canvas;
    [SerializeField]
    private RecordingController m_RecordingController;
    void Awake()
    {
        m_AROrigin = GetComponent<ARSessionOrigin>();
        m_ARRaycastManager = GetComponent<ARRaycastManager>();
        // m_GraphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
    }

    void Update()
    {
        if (shouldPlace)
        {
            UpdatePlacementPose();
            UpdatePlacementIndicator();
        }
      
    }

    public void PlaceObject()
    {
        m_RecordingController.SetOffsets(m_PlacementPose.position, m_PlacementPose.rotation, new Vector3(1, 1, 1));
        GameObject newObj = Instantiate(m_ObjectToPlace, m_PlacementPose.position, m_PlacementPose.rotation);
        RemovePlacementIndicator();
        //Logger.Log("Object Placed");
    }

    void RemovePlacementIndicator()
    {
        m_PlacementIndicator.SetActive(false);
        shouldPlace = false;
        for (int i = 0; i < m_Canvas.transform.childCount; ++i)
        {
            GameObject child = m_Canvas.transform.GetChild(i).gameObject;
            string name = child.name.ToLower();
            if (name.Contains("slider") || name.Contains("button"))
            {
                child.SetActive(false);
            }
        }
    }

    void UpdatePlacementIndicator()
    {
        if (isPlacementPoseValid)
        {
            m_PlacementIndicator.SetActive(true);
            m_PlacementIndicator.transform.SetPositionAndRotation(m_PlacementPose.position, m_PlacementPose.rotation);
            //m_ARSessionOrigin.MakeContentAppearAt(content, hitPose.position, m_Rotation);
        }
        else
        {
            m_PlacementIndicator.SetActive(false);
        }
    }

    void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        m_ARRaycastManager.Raycast(screenCenter, hits, TrackableType.All);

        isPlacementPoseValid = hits.Count > 0;
        if (isPlacementPoseValid)
        {
            m_PlacementPose = hits[0].pose;

            Vector3 cameraForward = Camera.current.transform.forward;
            Vector3 cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            m_PlacementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
}

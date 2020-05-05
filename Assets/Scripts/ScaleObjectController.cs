using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ScaleObjectController : MonoBehaviour
{
    [SerializeField, Tooltip("Weight with which the change in scale will be applied")]
    private float m_ScaleFactor = 1;
    [SerializeField]
    private SteamVR_Action_Vector2 m_ScaleObjectAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("ScaleObject");
    private Hand[] hands;
    void Awake()
    {
        m_ScaleObjectAction.onAxis += UpdateScaleMultiplier;
        hands = GetComponentsInChildren<Hand>();
    }

    private void UpdateScaleMultiplier(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        // only care about up and down
        float y = axis.y;
        
        foreach (Hand hand in hands)
        {
            if (hand.AttachedObjects.Count > 0)
            {
                foreach (Hand.AttachedObject obj in hand.AttachedObjects)
                {
                    obj.interactable.transform.localScale += new Vector3(1, 1, 1) * y * m_ScaleFactor;
                }
            }
        }
    }
}

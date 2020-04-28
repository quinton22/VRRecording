using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerController : MonoBehaviour
{
    public SteamVR_Action_Vector2 input;
    public float speed = 1;
    private Player player;
    public float CurrentDistance; // current  distance of HMD from character
    [SerializeField]
    private GameObject CameraRig;
    [SerializeField]
    private GameObject LeftHand;
    [SerializeField]
    private GameObject RightHand;
    [SerializeField]
    private bool UseGravity;
    void Start()
    {
        player = gameObject.GetComponent<Player>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //PreCharacterMovement();
        Vector3 direction = player.hmdTransform.TransformDirection(input.axis.x, 0, input.axis.y);
        transform.position += speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up) - ((UseGravity ? 9.81f : 0.0f) * Vector3.up * Time.deltaTime);
    }

    void PreCharacterMovement()
    {
        var CameraOffset = CameraRig.transform.localPosition;
        CameraOffset.y = 0;

        if (CameraOffset.magnitude > 0.001f)
        {
            CameraRig.transform.position -= CameraOffset;
            LeftHand.transform.position -= CameraOffset;
            RightHand.transform.position -= CameraOffset;

        }


        // var PlayerPos = transform.position;
        // var CameraPos = CameraRig.transform.position;
        // var LocalCameraPos = CameraRig.transform.localPosition;

        // Debug.Log($"CameraRig pos: {CameraPos}");
        // Debug.Log($"LocalCamera pos: {LocalCameraPos}");
        // Debug.Log($"this pos: {PlayerPos}");

        // var delta = CameraPos - PlayerPos;
        // delta.y = 0;
        // if (delta.magnitude > 0)
        // {
        //     characterController.Move(delta);
        // }

        // First, determine if the lateral movement will collide with the scene geometry.
        // var oldCameraPos = CameraRig.transform.position;
        // var wpos = CameraRig.transform.position;
        // var delta = wpos - transform.position;
        // delta.y = 0;
        // var len = delta.magnitude;
        // if (len > 0.0f)
        // {
        // 	characterController.Move(delta);
        // 	var currentDelta = transform.position - wpos;
        // 	currentDelta.y = 0;
        // 	CurrentDistance = currentDelta.magnitude;
        // 	CameraRig.transform.position = oldCameraPos;
        // 	if (EnableCollision)
        // 	{
        // 		if (CurrentDistance > 0)
        // 		{
        // 			CameraRig.transform.position = oldCameraPos - delta;
        // 		}
        // 		//OVRInspector.instance.fader.SetFadeLevel(0);
        // 		return;
        // 	}
        // }
        // else
        // {
        // 	CurrentDistance = 0;
        // }

        // // Next, determine if the player camera is colliding with something above the player by doing a sphere test from the feet to the head.
        // var bottom = transform.position;
        // bottom += characterController.center;
        // bottom.y -= characterController.height / 2.0f;

        // RaycastHit info;
        // var max = characterController.height;
        // if (Physics.SphereCast(bottom, characterController.radius, Vector3.up, out info, max,
        // 	gameObject.layer, QueryTriggerInteraction.Ignore))
        // {
        // 	// It hit something. Use the fade distance min/max to determine how much to fade.
        // 	var dist = info.distance;
        // 	dist = max - dist;
        // 	if (dist > CurrentDistance)
        // 	{
        // 		CurrentDistance = dist;
        // 	}
        // }
    }
}

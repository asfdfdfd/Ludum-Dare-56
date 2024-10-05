using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField]
    private GameObject leftFoot;

    [SerializeField]
    private GameObject rightFoot;

    [SerializeField]
    private float maxDistanceBetweenFoots;

    [SerializeField]
    private float mouseSensivity;

    [SerializeField]
    private Camera mainCamera;

    private GameObject activeFoot;

    private GameObject inactiveFoot;

    private InputAction pointAction;
    private InputAction lookAction;    
    private InputAction attackAction;

    private void Start()
    {
        pointAction = InputSystem.actions.FindAction("Point");
        lookAction = InputSystem.actions.FindAction("Look");
        // "Click" action triggered twice on "WasPerformedThisFrame".
        attackAction = InputSystem.actions.FindAction("Attack");        
    }

    private void Update()
    {
        var pointerPosition = pointAction.ReadValue<Vector2>();

        var pointerRay = mainCamera.ScreenPointToRay(pointerPosition);

        // Foot under mouse cursor.
        GameObject mouseOverFoot = null;

        if (Physics.Raycast(pointerRay, out RaycastHit hit))
        {
            mouseOverFoot = hit.collider.gameObject;
        }

        if (attackAction.WasPerformedThisFrame())
        {
            if (activeFoot != null)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                activeFoot = null;
            }
            else if (mouseOverFoot != null)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                activeFoot = mouseOverFoot;

                if (activeFoot == leftFoot) 
                {
                    inactiveFoot = rightFoot;
                }
                else
                {
                    inactiveFoot = leftFoot;
                }
            }
        }

        // Move active foot with mouse pointer.
        if (activeFoot != null)
        {
            var activeFootOldPosition = activeFoot.transform.position;
            var pointerDelta = lookAction.ReadValue<Vector2>() * mouseSensivity;
            var movementDelta = new Vector3(pointerDelta.x, 0, pointerDelta.y);
            var activeFootNewPosition = activeFootOldPosition + movementDelta;
            var inactiveFootPosition = inactiveFoot.transform.position;

            var distanceBetweenFoots = Vector3.Distance(activeFootNewPosition, inactiveFootPosition);

            if (distanceBetweenFoots <= maxDistanceBetweenFoots)
            {
                activeFoot.transform.position = activeFootNewPosition;
            }
            else
            {
                activeFoot.transform.position -= movementDelta.normalized * 0.001f;
            }
        }
    }
}

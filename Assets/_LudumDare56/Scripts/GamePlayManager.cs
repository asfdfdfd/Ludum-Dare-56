using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField]
    private GameObject leftFoot;

    [SerializeField]
    private GameObject rightFoot;

    [SerializeField]
    private GameObject virtualBody;

    [SerializeField]
    private float maxDistanceBetweenFoots;

    [SerializeField]
    private float footSpeed;

    [SerializeField]
    private Camera mainCamera;

    private GameObject activeFoot;

    private GameObject inactiveFoot
    {
        get
        {
            if (activeFoot == leftFoot)
            {
                return rightFoot;
            }
            else if (activeFoot == rightFoot)
            {
                return leftFoot;
            }
            else
            {
                return null;
            }
        }
    }

    private InputAction pointAction;
    private InputAction lookAction;    
    private InputAction attackAction;
    private InputAction moveAction;
    private InputAction enableLeftFootAction;
    private InputAction enableRightFootAction;

    private void Start()
    {
        Cursor.visible = false;

        pointAction = InputSystem.actions.FindAction("Point");
        lookAction = InputSystem.actions.FindAction("Look");
        attackAction = InputSystem.actions.FindAction("Attack");
        moveAction = InputSystem.actions.FindAction("Move");
        enableLeftFootAction = InputSystem.actions.FindAction("Enable Left Foot");
        enableRightFootAction = InputSystem.actions.FindAction("Enable Right Foot");
    }

    private void Update()
    {
        if (enableLeftFootAction.WasPerformedThisFrame())
        {
            if (activeFoot == leftFoot)
            {
                DisableActiveFoot();
            }
            else
            {
                EnableActiveFoot(leftFoot);
            }
        }
        else if (enableRightFootAction.WasPerformedThisFrame())
        {
            if (activeFoot == rightFoot)
            {
                DisableActiveFoot();
            }
            else
            {
                EnableActiveFoot(rightFoot);
            }
        }

        // Move active foot with mouse pointer.
        if (activeFoot != null)
        {
            var movementVector2 = moveAction.ReadValue<Vector2>();
            var movementVector3 = new Vector3(movementVector2.x, 0, movementVector2.y);

            var activeFootOldPosition = activeFoot.transform.position;
            var activeFootNewPosition = activeFootOldPosition + movementVector3 * Time.deltaTime * footSpeed;;

            var inactiveFootPosition = inactiveFoot.transform.position;

            var distanceBetweenFootsNew = Vector3.Distance(activeFootNewPosition, inactiveFootPosition);

            if (distanceBetweenFootsNew <= maxDistanceBetweenFoots)
            {
                if (activeFoot == leftFoot) 
                {
                    if (activeFootNewPosition.x < rightFoot.transform.position.x)
                    {
                        // TODO: Vector3.MoveTowards?
                        activeFoot.transform.position = activeFootNewPosition;
                    }
                }
                else if (activeFoot == rightFoot)
                {
                    if (activeFootNewPosition.x > leftFoot.transform.position.x)
                    {
                        activeFoot.transform.position = activeFootNewPosition;
                    }
                }
            }
        }

        var virtualBodyPointLeft = new Vector3(leftFoot.transform.position.x, 0, leftFoot.transform.position.z);
        var virtualBodyPointRight = new Vector3(rightFoot.transform.position.x, 0, rightFoot.transform.position.z);
        var distanceBetweenFoots = Vector3.Distance(virtualBodyPointLeft,virtualBodyPointRight);
        var distanceBetweenFootsHalf = distanceBetweenFoots / 2.0f;
        var directionFromLeftToRight = (virtualBodyPointLeft - virtualBodyPointRight).normalized;
        virtualBody.transform.position = virtualBodyPointLeft - directionFromLeftToRight * distanceBetweenFootsHalf;
    }

    private void EnableActiveFoot(GameObject foot)
    {
        if (foot == null)
        {
            return;
        }

        if (activeFoot != null)
        {
            DisableActiveFoot();
        }

        activeFoot = foot;
        activeFoot.transform.position = new Vector3(activeFoot.transform.position.x, 3.0f, activeFoot.transform.position.z);
    }

    private void DisableActiveFoot()
    {
        if (activeFoot == null)
        {
            return;
        }

        activeFoot.transform.position = new Vector3(activeFoot.transform.position.x, 0.0f, activeFoot.transform.position.z);

        activeFoot = null;
    }
}

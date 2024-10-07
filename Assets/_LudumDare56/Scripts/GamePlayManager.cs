using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField]
    private float footUpSpeed = 0.3f;

    [SerializeField]
    private GameObject gameObjectTitleImage;

    [SerializeField]
    private GameObject gameObjectTitleImageRemoveTo;    

    [SerializeField]
    private GameObject gameObjectGameOverImage;

    [SerializeField]
    private GameObject leftFoot;

    private Rigidbody leftFootRigidbody;

    [SerializeField]
    private GameObject leftFootFollower;

    [SerializeField]
    private GameObject rightFoot;

    private Rigidbody rightFootRigidbody;

    [SerializeField]
    private GameObject rightFootFollower;

    [SerializeField]
    private GameObject virtualBody;

    [SerializeField]
    private float maxDistanceBetweenFoots;

    [SerializeField]
    private float footSpeed;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private AudioSource backgroundMusic;

    [SerializeField]
    private AudioSource gameOverAudioSource;

    [SerializeField]
    private GameObject congratulationsGameObject;

    [SerializeField]
    private Material congratulationsGameObjectMaterial;

    [SerializeField]
    private AudioSource legUpAudioSource;

    [SerializeField]
    private List<AudioClip> legUpAudioClips;

    [SerializeField]
    private AudioSource legDownAudioSource;

    [SerializeField]
    private List<AudioClip> legDownAudioClips;

    [SerializeField]
    private AudioSource legDownCarpetAudioSource;

    [SerializeField]
    private List<AudioClip> legDownCarpetAudioClips;    

    [SerializeField]
    private AudioSource pupupuAudioSource;

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

    public static GamePlayManager Instance;

    private InputAction pointAction;
    private InputAction lookAction;    
    private InputAction attackAction;
    private InputAction moveAction;
    private InputAction enableLeftFootAction;
    private InputAction enableRightFootAction;

    private bool isTitleImageOnScreen = true;

    private bool isGameOver = false;

    private bool isGameFinished = false;

    private HashSet<Collider> collidersOnCovrik = new HashSet<Collider>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Cursor.visible = false;

        pointAction = InputSystem.actions.FindAction("Point");
        lookAction = InputSystem.actions.FindAction("Look");
        attackAction = InputSystem.actions.FindAction("Attack");
        moveAction = InputSystem.actions.FindAction("Move");
        enableLeftFootAction = InputSystem.actions.FindAction("Enable Left Foot");
        enableRightFootAction = InputSystem.actions.FindAction("Enable Right Foot");

        leftFootRigidbody = leftFoot.GetComponent<Rigidbody>();
        rightFootRigidbody = rightFoot.GetComponent<Rigidbody>();

        leftFootFollower.GetComponentInChildren<SpriteRenderer>().DOFade(0.0f, 0.0f).From(0.0f);
        rightFootFollower.GetComponentInChildren<SpriteRenderer>().DOFade(0.0f, 0.0f).From(0.0f);

        gameObjectGameOverImage.GetComponentInChildren<Image>().DOFade(0.0f, 0.0f).From(0.0f);

        congratulationsGameObject.GetComponent<MeshRenderer>().SetMaterials(new System.Collections.Generic.List<Material>());
    }

    private void RemoveGameOverScreen()
    {
        isGameOver = false;

        // gameObjectGameOverImage.GetComponentInChildren<Image>().DOFade(0.0f, footUpSpeed).SetEase(Ease.InOutSine);

        SceneManager.LoadScene("GameplayScene");
    }

    private void Update()
    {
        if (isGameOver)
        {                
            if (Keyboard.current.anyKey.wasPressedThisFrame || enableLeftFootAction.WasPressedThisFrame() || enableRightFootAction.WasPressedThisFrame())
            {
                RemoveGameOverScreen();
            }

            return;
        }

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

            RemoveTitleImage();
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

            RemoveTitleImage();
        }

        var virtualBodyPointLeft = new Vector3(leftFoot.transform.position.x, 0, leftFoot.transform.position.z);
        var virtualBodyPointRight = new Vector3(rightFoot.transform.position.x, 0, rightFoot.transform.position.z);
        var distanceBetweenFoots = Vector3.Distance(virtualBodyPointLeft,virtualBodyPointRight);
        var distanceBetweenFootsHalf = distanceBetweenFoots / 2.0f;
        var directionFromLeftToRight = (virtualBodyPointLeft - virtualBodyPointRight).normalized;
        virtualBody.transform.position = virtualBodyPointLeft - directionFromLeftToRight * distanceBetweenFootsHalf;
    }

    private void RemoveTitleImage()
    {
        if (!isTitleImageOnScreen) 
        {
            return;
        }

        isTitleImageOnScreen = false;

        gameObjectTitleImage.transform
            .DOMoveY(gameObjectTitleImageRemoveTo.transform.position.y, footUpSpeed)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => Destroy(gameObjectTitleImage));

    }

    private void FixedUpdate()
    {
        if (isGameOver) 
        {
            return;
        }

        // Move active foot with mouse pointer.
        if (activeFoot != null)
        {
            var movementVector2 = moveAction.ReadValue<Vector2>();
            var movementVector3 = new Vector3(movementVector2.x, 0, movementVector2.y);

            var activeFootOldPosition = activeFoot.transform.position;
            var activeFootNewPosition = activeFootOldPosition + movementVector3 * Time.deltaTime * footSpeed;

            var movementDistance = Vector3.Distance(activeFootOldPosition, activeFootNewPosition);
            var movementDirection = (activeFootNewPosition - activeFootOldPosition).normalized;

            var inactiveFootPosition = inactiveFoot.transform.position;

            var activeFootOldPositionOnGround = new Vector3(activeFootOldPosition.x, 0, activeFootOldPosition.z);
            var activeFootNewPositionOnGround = new Vector3(activeFootNewPosition.x, 0, activeFootNewPosition.z);

            var distanceBetweenFoots = Vector3.Distance(activeFootOldPosition, inactiveFootPosition);
            var distanceBetweenFootsNew = Vector3.Distance(activeFootNewPosition, inactiveFootPosition);

            if (distanceBetweenFootsNew <= maxDistanceBetweenFoots)
            {
                if (activeFoot == leftFoot) 
                {
                    if (activeFootNewPosition.x < rightFoot.transform.position.x)
                    {                                              
                        if (!leftFootRigidbody.SweepTest(movementDirection, out RaycastHit hit, movementDistance))
                        {                            
                            leftFootRigidbody.MovePosition(activeFootNewPosition);
                        }
                    }
                }
                else if (activeFoot == rightFoot)
                {
                    if (activeFootNewPosition.x > leftFoot.transform.position.x)
                    {                                              
                        if (!rightFootRigidbody.SweepTest(movementDirection, out RaycastHit hit, movementDistance))
                        {                            
                            rightFootRigidbody.MovePosition(activeFootNewPosition);
                        }
                    }
                }
            }

            var followerPosition = new Vector3(activeFootNewPositionOnGround.x, 0, activeFootNewPositionOnGround.z + 1.7f);

            if (activeFoot == leftFoot)
            {
                leftFootFollower.transform.position = followerPosition;
            }
            else
            {
                rightFootFollower.transform.position = followerPosition;
            }             
        }
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

        legUpAudioSource.PlayOneShot(legUpAudioClips[Random.Range(0, legUpAudioClips.Count)]);

        activeFoot = foot;

        var position = new Vector3(activeFoot.transform.position.x, 3.0f, activeFoot.transform.position.z);

        activeFoot.transform.DOMove(position, footUpSpeed);

        mainCamera.DOShakePosition(1.0f);

        if (activeFoot == leftFoot)
        {
            leftFootFollower.GetComponentInChildren<SpriteRenderer>().DOFade(1.0f, footUpSpeed).From(0.0f);
        }
        else
        {
            rightFootFollower.GetComponentInChildren<SpriteRenderer>().DOFade(1.0f, footUpSpeed).From(0.0f);
        }        
    }

    private void DisableActiveFoot()
    {
        if (activeFoot == null)
        {
            return;
        }

        if (collidersOnCovrik.Count > 0)
        {
            if (collidersOnCovrik.Intersect(activeFoot.GetComponentsInChildren<Collider>()).Count() > 0) 
            {
                legDownCarpetAudioSource.PlayOneShot(legDownCarpetAudioClips[Random.Range(0, legDownCarpetAudioClips.Count)]);
            }
            else
            {
                legDownAudioSource.PlayOneShot(legDownAudioClips[Random.Range(0, legDownAudioClips.Count)]);
            }
        }
        else
        {
            legDownAudioSource.PlayOneShot(legDownAudioClips[Random.Range(0, legDownAudioClips.Count)]);
        }

        var position = new Vector3(activeFoot.transform.position.x, 0.0f, activeFoot.transform.position.z);

        activeFoot.transform.DOMove(position, footUpSpeed);

        if (activeFoot == leftFoot)
        {
            leftFootFollower.GetComponentInChildren<SpriteRenderer>().DOFade(0.0f, footUpSpeed).From(1.0f);
        }
        else
        {
            rightFootFollower.GetComponentInChildren<SpriteRenderer>().DOFade(0.0f, footUpSpeed).From(1.0f);
        }

        activeFoot = null;
    }

    public void GameOver()
    {
        isGameOver = true;

        pupupuAudioSource.Play();

        gameObjectGameOverImage.GetComponentInChildren<Image>().DOFade(1.0f, footUpSpeed).SetEase(Ease.InOutSine);

        backgroundMusic.DOFade(0.0f, 0.1f);

        gameOverAudioSource.Play();
    }

    public void GameFinihsed()
    {
        if (isGameFinished)
        {
            return;
        }

        isGameFinished = true;

        congratulationsGameObject.GetComponent<MeshRenderer>().material = congratulationsGameObjectMaterial;
    }

    public void SetCovrikColliderSet(HashSet<Collider> colliders)
    {
        this.collidersOnCovrik = colliders;
    }
}

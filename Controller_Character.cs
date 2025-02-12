using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller_Character : MonoBehaviour
{
    [Space, Header("Camera Setting")]
    public bool isControl = false;
    [Range(0.05f, 0.5f)] public float DPI = 0.05f;
    [Range(0, 90)] public int clampVerticle = 0;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject axisCameraHorizon;
    [SerializeField] private GameObject axisCameraVerticle;
    public float speedSmoothCamera = 1f;
    [Range(0.5f, 1.5f)] public float CameraLocation_Height = 1.1f;
    [Range(-1f, 1f)] public float CameraLocation_Verticle = -0.7f;
    [Range(-1f, -10f)] public float CameraLocation_Distance = -2f;

    [Space, Header("Moving")]
    public bool isMoving = false;
    public bool isPauseMoving = false;
    //[SerializeField] private float velocity_Max = 10f;
    //[SerializeField] private float velocity_Current = 0f;
    [SerializeField] private float velocityRotate = 1f;
    [SerializeField] private AnimationCurve velocityMove_Anim;
    [SerializeField] private float velocityMove_Amin_Time = 0f;
    //[SerializeField] private float speedMoveVector = 1f;
    [SerializeField] private Vector2 vectorMove = new Vector2(0f, 1f);
    public GameObject point_DirectMove;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnValidate()
    {
        updateCamera();
    }

    private void FixedUpdate()
    {
        //Update POS of camera
        axisCameraHorizon.transform.position = Vector3.Lerp(axisCameraHorizon.transform.position, transform.position, Time.deltaTime * 10f);

        //Set direct move
        point_DirectMove.transform.localPosition = new Vector3(vectorMove.x * 10, 0, vectorMove.y * 10);

        //Calculate time for AnimationCurve
        velocityMove_Amin_Time = Mathf.Clamp(
            (isMoving == true ? velocityMove_Amin_Time + Time.deltaTime : velocityMove_Amin_Time - (Time.deltaTime * 3)) * (isPauseMoving == true ? 0f : 1f),
            0f,
            velocityMove_Anim.keys[1].time);

        //Move Character with Time calculated
        transform.position = Vector3.MoveTowards(transform.position, point_DirectMove.transform.position, Time.deltaTime * velocityMove_Anim.Evaluate(velocityMove_Amin_Time));

        //Rotate Character to direct move
        if (velocityMove_Amin_Time > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(point_DirectMove.transform.position - transform.position, Vector3.up), Time.deltaTime * velocityRotate);
        }

        //Set velocity into Animator
        transform.GetComponent<Animator>().SetFloat("Velocity", velocityMove_Anim.Evaluate(velocityMove_Amin_Time) / velocityMove_Anim.keys[1].value);
    }

    #region Moving
    public void moving(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            isMoving = true;
            vectorMove = context.ReadValue<Vector2>();
        }
        else if (isControl == true && context.performed)
        {
            vectorMove = context.ReadValue<Vector2>();
        }
        else if (isControl == true && context.canceled)
        {
            isMoving = false;
        }
    }

    public void dash(InputAction.CallbackContext context)
    {
        //Space Key
        if (isControl == true && context.started)
        {
            transform.GetComponent<Animator>().SetTrigger("Dash");
        }
    }

    public void takeControl(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //Tab Key
            isControl = !isControl;
            Cursor.lockState = isControl == true ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = isControl == true ? false : true;
        }
    }

    public void resetMovingVelocity()
    {
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        velocityMove_Amin_Time = 0f;
    }
    #endregion

    #region Camera
    public void lookAround(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            Vector2 vectorLook = context.ReadValue<Vector2>();

            axisCameraHorizon.transform.localRotation = Quaternion.Euler(0, axisCameraHorizon.transform.localRotation.eulerAngles.y + (vectorLook.x * DPI), 0);

            float sampleVerticle = axisCameraVerticle.transform.localRotation.eulerAngles.x + (vectorLook.y * DPI) * (-1);

            axisCameraVerticle.transform.localRotation = Quaternion.Euler(
                sampleVerticle <= (90 - clampVerticle) || sampleVerticle >= (270 + clampVerticle) ?
                sampleVerticle : sampleVerticle < 180 ?
                (90 - clampVerticle) : (270 + clampVerticle),
                0, 0);
        }
    }

    public void zoomIn(InputAction.CallbackContext context)
    {
        //Scroll mouse 3 up
        if (isControl == true && context.started)
        {
            CameraLocation_Distance = Mathf.Clamp(CameraLocation_Distance + 0.3f, -10, -1);
            updateCamera();
        }
    }

    public void zoomOut(InputAction.CallbackContext context)
    {
        //Scroll mouse 3 down
        if (isControl == true && context.started)
        {
            CameraLocation_Distance = Mathf.Clamp(CameraLocation_Distance - 0.3f, -10, -1);
            updateCamera();
        }
    }

    private void updateCamera()
    {
        //axisCameraHorizon.transform.position = transform.position;
        //axisCameraVerticle.transform.position = new Vector3(transform.position.x, transform.position.y + CameraLocation_Height, transform.position.z);
        mainCamera.transform.localPosition = new Vector3(CameraLocation_Verticle, 0, CameraLocation_Distance);
    }
    #endregion

    #region Attack
    public void normalAtk(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            transform.GetComponent<Animator>().SetTrigger("Atk_Normal");
        }
    }

    public void heavyAtk(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            transform.GetComponent<Animator>().SetTrigger("Atk_Heavy");
        }
    }

    public void breakgroundAtk(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            transform.GetComponent<Animator>().SetTrigger("Atk_BrGround");
        }
    }

    public void airAtk(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            transform.GetComponent<Animator>().SetTrigger("Atk_Air");
        }
    }
    #endregion

    #region Change Style Attack
    public void changeStyle_1(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            transform.GetComponent<AnimationEvent_Character>().changeStyle(0);
        }
    }

    public void changeStyle_2(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            transform.GetComponent<AnimationEvent_Character>().changeStyle(1);
        }
    }

    public void changeStyle_3(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            transform.GetComponent<AnimationEvent_Character>().changeStyle(2);
        }
    }

    public void changeStyle_4(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            transform.GetComponent<AnimationEvent_Character>().changeStyle(3);
        }
    }

    public void changeStyle_5(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            transform.GetComponent<AnimationEvent_Character>().changeStyle(4);
        }
    }

    public void changeStyle_6(InputAction.CallbackContext context)
    {
        if (isControl == true && context.started)
        {
            transform.GetComponent<AnimationEvent_Character>().changeStyle(5);
        }
    }
    #endregion

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag.StartsWith("hitBox"))
    //    {
    //        GetComponent<Core>().takeDamage(1, other.transform.position, );
    //    }
    //}
}

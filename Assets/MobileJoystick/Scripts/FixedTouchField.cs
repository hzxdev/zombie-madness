using UnityEngine;
using UnityEngine.EventSystems;
public class FixedTouchField : MonoBehaviour, IPointerExitHandler, IPointerUpHandler//, IPointerUpHandler, IPointerEnterHandler
{

    public Vector2 TouchDist;
    [HideInInspector]
    public Vector2 PointerOld;
    [HideInInspector]
    protected int PointerId;
    public bool Pressed;
    Touch deltaingTouch;
    RectTransform rect;
    public TouchPhase tp;
    public bool playingFromPc;
    public float pcSens;
    bool cursorLocked;
    Vector2 currentPosition, lastPosition, deltaPosition;

    // Use this for initialization
    void Start()
    {
        rect = GetComponent<RectTransform>();

        if(playingFromPc)
        {
            cursorLocked = true;
            if (cursorLocked)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

    }
    // Update is called once per frame
    void Update()
    {
        /*
        if (Pressed)
        {
            if (PointerId >= 0 && PointerId < Input.touches.Length)
            {
                TouchDist = Input.touches[PointerId].position - PointerOld;
                PointerOld = Input.touches[PointerId].position;
            }
            else
            {
                TouchDist = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - PointerOld;
                PointerOld = Input.mousePosition;
            }
        }
        */
        // else
        //{
        //   TouchDist = new Vector2();
        // }

        if(Input.GetKeyDown(KeyCode.E))
        {
            cursorLocked = !cursorLocked;
            if(cursorLocked)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            } else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        if(playingFromPc)
        {
            currentPosition = Input.mousePosition;
            deltaPosition = currentPosition - lastPosition;
            lastPosition = currentPosition;

            TouchDist = deltaPosition * pcSens;

            return;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.touches[i].position))
            {
                if (Input.touches[i].phase == TouchPhase.Moved && RectTransformUtility.RectangleContainsScreenPoint(rect, Input.touches[i].position))
                {
                    if(Input.touches[i].phase != TouchPhase.Ended)
                    TouchDist = Input.touches[i].deltaPosition * 1.5f;
                }
                else
                {
                    TouchDist = Vector2.zero;
                }
            }


            //if (deltaingTouch.phase == TouchPhase.Ended)
            //{             
            //   Pressed = false;
            //  TouchDist = Vector2.zero;
            // }


        }

        if(Input.touchCount == 0)
            TouchDist = Vector2.zero;
        tp = deltaingTouch.phase;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TouchDist = Vector2.zero;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        TouchDist = Vector2.zero;
    }

    /* public void OnPointerDown(PointerEventData eventData)
     {
         Pressed = true;
         PointerId = eventData.pointerId;
         PointerOld = eventData.position;
     }

    
     public void OnPointerEnter(PointerEventData eventData)
     {
         PointerId = eventData.pointerId;
         PointerOld = eventData.position;
     }
    */
}
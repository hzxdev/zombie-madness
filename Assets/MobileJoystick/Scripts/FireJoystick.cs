using UnityEngine;
using UnityEngine.EventSystems;

namespace DitzeGames.MobileJoystick
{

    /// <summary>
    /// Put it on any Image UI Element
    /// </summary>
    public class FireJoystick : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        protected RectTransform Background;
        public Rect bgRect;
        public bool Pressed;
        public int PointerId;
        public RectTransform Handle;
        [Range(0f, 2f)]
        public float HandleRange = 1f;
        Touch initTouch = new Touch();

        [HideInInspector]
        public Vector2 InputVector = Vector2.zero;
        public Vector2 AxisNormalized { get { return InputVector.magnitude > 0.25f ? InputVector.normalized : (InputVector.magnitude < 0.01f ? Vector2.zero : InputVector * 4f); } }
        float leftBound, rightBound, upBound, downBound;

        void Start()
        {
            if (Handle == null)
                Handle = transform.GetChild(0).GetComponent<RectTransform>();
            Background = GetComponent<RectTransform>();
            Background.pivot = new Vector2(0.5f, 0.5f);
            
            Pressed = false;
          //  leftBound = Handle.position.x - Handle.
        }

        void Update()
        {
            
          

            if (Input.touchCount >= 1)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(Handle, Input.touches[i].position))
                    {
                        initTouch = Input.touches[i];

                        if(initTouch.phase == TouchPhase.Began)
                        {
                         
                            Pressed = true;
                        }


                        
                        PointerId = initTouch.fingerId;

                        //The touch is within the bounds
                        //Handle.position = Input.touches[i].position;

                    }

                    if (Input.touches[i].fingerId == PointerId)
                        initTouch = Input.touches[i];

                    if (Pressed)
                    {
                        Vector2 direction = (PointerId >= 0 && PointerId < Input.touches.Length) ? Input.touches[PointerId].position - new Vector2(Background.position.x, Background.position.y) : new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(Background.position.x, Background.position.y);
                        InputVector = (direction.magnitude > Background.sizeDelta.x / 2f) ? direction.normalized : direction / (Background.sizeDelta.x / 2f);
                        Handle.anchoredPosition = (InputVector * Background.sizeDelta.x / 2f) * HandleRange;
                    }
                }
            }

            if (initTouch.phase == TouchPhase.Ended || initTouch.phase == TouchPhase.Canceled || Input.touchCount == 0)
            {
                Pressed = false;
                InputVector = Vector2.zero;
                Handle.anchoredPosition = Vector2.zero;
                initTouch = new Touch();             
            }
           
            //Bunun olmaması gerek gibi
           /* if ( PointerId >= Input.touchCount)
            {
                Pressed = false;
                InputVector = Vector2.zero;
                Handle.anchoredPosition = Vector2.zero;
                initTouch = new Touch();
                Debug.Log("SECOND IF");
            }
            */


        }

        public void OnPointerDown(PointerEventData eventData)
        {

      //      Pressed = true;
          // PointerId = eventData.pointerId;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
          //  Pressed = false;
           // InputVector = Vector2.zero;
            //Handle.anchoredPosition = Vector2.zero;
        }

      
    }
}


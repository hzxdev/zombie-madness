using UnityEngine;
using UnityEngine.EventSystems;

namespace DitzeGames.MobileJoystick
{

    /// <summary>
    /// Put it on any Image UI Element
    /// </summary>
    public class Joystick : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        int width, height;
        CanvasGroup cg;
        TouchPhase ph;
     
        protected RectTransform Background;
        public bool Pressed;
        protected int PointerId;
        public RectTransform Handle;
        [Range(0f,2f)]
        public float HandleRange = 1f;

        [HideInInspector]
        public Vector2 InputVector = Vector2.zero;
        public Vector2 AxisNormalized { get { return InputVector.magnitude > 0.25f ? InputVector.normalized : (InputVector.magnitude < 0.01f ? Vector2.zero : InputVector * 4f); } }

        void Start()
        {
            if (Handle == null)
                Handle = transform.GetChild(0).GetComponent<RectTransform>();
            Background = GetComponent<RectTransform>();
            Background.pivot = new Vector2(0.5f, 0.5f);
            Pressed = false;
            cg = GetComponent<CanvasGroup>();
            width = Screen.width;
            height = Screen.height;
            cg.alpha = 0;
        }

        void Update()
        {
           

            

                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.touches[i].position.x < width / 4 && Input.touches[i].position.y < height * (7.5f / 10))
                    {
                        if (Input.touches[i].position.x < width / 4 && Input.touches[i].position.y < height * (7.5f / 10)) //ekranin solu
                        {
                            if (Input.touches[i].phase == TouchPhase.Began)
                            {

                                Background.position = Input.touches[i].position;
                                cg.alpha = 1;
                            }

                            if (Input.touches[i].phase == TouchPhase.Moved)
                            {
                                Vector2 direction = Input.touches[i].position - new Vector2(Background.position.x, Background.position.y);
                                InputVector = (direction.magnitude > Background.sizeDelta.x / 2f) ? direction.normalized : direction / (Background.sizeDelta.x / 2f);
                                Handle.anchoredPosition = (InputVector * Background.sizeDelta.x / 2f) * HandleRange;
                            }


                            if (Input.touches[i].phase == TouchPhase.Ended)
                            {
                                Pressed = false;
                                InputVector = Vector2.zero;
                                Handle.anchoredPosition = Vector2.zero;
                                cg.alpha = 0;
                            }



                        }
                    }
                        
                    
                }
                
            

         
           
            
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            
            Pressed = true;
            PointerId = eventData.pointerId;
        }

        public void OnPointerUp(PointerEventData eventData) // should we do something about this?
        {
            Pressed = false;
            InputVector = Vector2.zero;
            Handle.anchoredPosition = Vector2.zero;
            cg.alpha = 0;
        }
    }
}

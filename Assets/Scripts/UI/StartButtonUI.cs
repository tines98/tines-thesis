using UnityEngine;
using UnityEngine.UI;

namespace UI{
    [RequireComponent(typeof(Button))]
    public class StartButtonUI : MonoBehaviour
    {
        private Button button;
    
        // Start is called before the first frame update
        void Start()
        {
            button = GetComponent<Button>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ToggleButtonInteractivity() => button.interactable = !button.interactable;
    }
}

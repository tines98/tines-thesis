using Demo;
using UnityEngine;
using UnityEngine.UI;

namespace UI{
    public class DemoControllerUI : MonoBehaviour{
        private FluidDemoManager fluidDemoManager;
        private Slider deathPlaneSlider;
        [SerializeField] private Text demoNumberUI;
    
        // Start is called before the first frame update
        void Start(){
            fluidDemoManager = GameObject.FindWithTag("Demo Manager")
                                         .GetComponent<FluidDemoManager>();
            
            deathPlaneSlider = GetComponentInChildren<Slider>();
        }


        // Update is called once per frame
        void Update(){
            UpdateDemoNumberUI();
        }

    
        /// <summary>
        /// Updates the UI Text component to show which demo is currently selected
        /// </summary>
        void UpdateDemoNumberUI(){
            if (fluidDemoManager.FinishedAllDemos){
                demoNumberUI.text = "Complete!";
                return;
            }
            // What the UI Text should show
            var demoNumberText = $"Demo: {fluidDemoManager.CurrentDemoIndex}/{fluidDemoManager.DemoCount}";
            // Update UI Text if not correct
            if (demoNumberUI.text != demoNumberText)
                demoNumberUI.text = demoNumberText;
        }

        /// <summary>
        /// Message receiver function for a UI slider.
        /// Updates the current demo's deathPlaneHeight 
        /// </summary>
        /// <param name="slider"></param>
        public void SliderOnChange(Slider slider) => fluidDemoManager.UpdateDeathPlane(slider.value);


        /// <summary>
        /// Stops the current demo
        /// </summary>
        public void StopDemo() => fluidDemoManager.StopDemo();

    
        /// <summary>
        /// Selects the next demo
        /// </summary>
        public void NextDemo(){
            Debug.Log("Click Next Demo");
            fluidDemoManager.NextDemo();
            deathPlaneSlider.value = 0f;
        }

    
        /// <summary>
        /// Starts the current demo
        /// </summary>
        public void StartDemo() => fluidDemoManager.StartDemo();
    }
}

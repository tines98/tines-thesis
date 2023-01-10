using System.Collections.Generic;
using PBDFluid;
using UnityEngine;
using UnityEngine.UI;

public class DemoControllerUI : MonoBehaviour{
    private int currentDemo;
    private List<FluidDemo> demos;
    private Slider deathPlaneSlider;
    [SerializeField] private Text demoNumberUI;
    
    // Start is called before the first frame update
    void Start(){
        demos = new List<FluidDemo>();
        deathPlaneSlider = GetComponentInChildren<Slider>();
        LoggingUtility.LogInfo($"Found {demos.Count} Demos");
    }

    
    /// <summary>
    /// Finds every gameobject with the tag "Demo" and returns them
    /// </summary>
    /// <returns>List of GameObjects with the tag "Demo"</returns>
    private List<GameObject> GetDemoGameObjects() => new List<GameObject>(GameObject.FindGameObjectsWithTag("Demo"));

    
    /// <summary>
    /// Iterates through <see cref="GetDemoGameObjects"/> and populates <see cref="demos"/> with <see cref="FluidDemo"/> components within
    /// </summary>
    private void GetDemos() => GetDemoGameObjects().ForEach(obj => demos.Add(obj.GetComponent<FluidDemo>()));
    

    // Update is called once per frame
    void Update(){
        UpdateDemoNumberUI();
    }

    
    /// <summary>
    /// Updates the UI Text component to show which demo is currently selected
    /// </summary>
    void UpdateDemoNumberUI(){
        // What the UI Text should show
        var demoNumberText = $"Demo: {currentDemo}/{demos.Count}";
        // Update UI Text if not correct
        if (demoNumberUI.text != demoNumberText)
            demoNumberUI.text = demoNumberText;
    }

    public void FindDemos() => GetDemos();
    
    /// <summary>
    /// Message receiver function for a UI slider.
    /// Updates the current demo's deathPlaneHeight 
    /// </summary>
    /// <param name="slider"></param>
    public void SliderOnChange(Slider slider) => demos[currentDemo].deathPlaneHeight = slider.value;

    
    /// <summary>
    /// Stops the current demo
    /// </summary>
    public void StopDemo() => demos[currentDemo].StopDemo();

    
    /// <summary>
    /// Selects the next demo
    /// </summary>
    public void NextDemo(){
        Debug.Log("Click Next Demo");
        currentDemo++;
        deathPlaneSlider.value = 0f;
        if (currentDemo >= demos.Count) currentDemo--;
    }

    
    /// <summary>
    /// Starts the current demo
    /// </summary>
    public void StartDemo() => demos[currentDemo].StartDemo();
}

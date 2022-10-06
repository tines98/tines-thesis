using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathPlane : MonoBehaviour
{
    public Vector3 size;
    public Slider slider;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        var halfWayUp = new Vector3(0, size.y / 2f, 0);
        var center = transform.position+halfWayUp;
        Gizmos.DrawWireCube(center,size);
    }

    /// <summary>
    /// Flag method, to be called from the UI slider, updates the height of the deathbox
    /// </summary>
    public void SliderHasChanged() => size.y = slider.value;
}

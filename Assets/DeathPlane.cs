using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    public Vector3 size;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }   

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        var halfWayUp = new Vector3(0, size.y / 2f, 0);
        var center = transform.position+halfWayUp;
        Gizmos.DrawWireCube(center,size);
    }
}

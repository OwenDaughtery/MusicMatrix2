using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour {

    Vector3 newPosition;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        newPosition = transform.position;
    }

    // Update is called once per frame
    void Update () {
        
    }

    //
    public static Vector3 getMousePosition() {
        var mousePoint = Input.mousePosition;
        mousePoint.z = 10;
        mousePoint = Camera.main.ScreenToWorldPoint(mousePoint);
        return mousePoint;
    }


}

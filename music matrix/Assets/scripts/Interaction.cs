using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour {

    Vector3 newPosition;
    private Camera cam;

    void Start(){
        cam = Camera.main;
        newPosition = transform.position;
    }


    void Update () {
    }

    /// <summary>
    /// Method used to get mouse position at time of call.
    /// </summary>
    /// <returns>A vector3 of the position of the mouse on the screen</returns>
    public static Vector3 getMousePosition() {
        var mousePoint = Input.mousePosition;
        mousePoint.z = 10;
        mousePoint = Camera.main.ScreenToWorldPoint(mousePoint);
        return mousePoint;
    }


}

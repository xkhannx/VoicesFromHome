using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform camPlace;
    public Transform camLookAt;
    public Transform camSmoothLookAt;
    public float camSmooth = 0.1f;
    Vector3 velocity = Vector3.zero;
    Vector3 velocity2 = Vector3.zero;
    Vector3 velocity3 = Vector3.zero;
    Vector3 upVector = Vector3.up;
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, camPlace.position, ref velocity, camSmooth);
        camSmoothLookAt.position = Vector3.SmoothDamp(camSmoothLookAt.position, camLookAt.position, ref velocity2, camSmooth);
        upVector = Vector3.SmoothDamp(upVector, camLookAt.up, ref velocity3, camSmooth);
        transform.LookAt(camSmoothLookAt, upVector);

    }
}

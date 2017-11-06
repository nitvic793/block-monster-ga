using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform followTransform;
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - followTransform.position;
        transform.position = followTransform.position + offset;
    }

    void LateUpdate()
    {
       // transform.position = player.transform.position + offset;
    }
}

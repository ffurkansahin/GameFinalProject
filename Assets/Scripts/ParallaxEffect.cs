using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    
    public Camera cam; 
    public Transform followTarget;

    Vector2 startPosition;
    float startZ;

    
    Vector2 camMoveSinceStart => (Vector2)cam.transform.position - startPosition;

    float zDistanceFromTarget => transform.position.z - followTarget.position.z;

    
    float clippingPlane => cam.transform.position.z + (zDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane);

    float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / clippingPlane;

    void Start()
    {
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    void Update()
    {
        Vector2 newPosition = startPosition + camMoveSinceStart * parallaxFactor;
        transform.position = new Vector3(newPosition.x, newPosition.y, startZ);
    }
}
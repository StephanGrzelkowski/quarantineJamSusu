using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    GameObject[] zuzus;
    public Vector2 flockCenter;
    Vector2 target;
    public Vector2 targetDirection;
    // Start is called before the first frame update
    void Start()
    {
        //get all gameobject of the flock
        zuzus = GameObject.FindGameObjectsWithTag("zuzu");

        flockCenter = GetFlockCenter(); 

        this.transform.position = flockCenter; 
    }

    // Update is called once per frame
    void Update()
    {
        flockCenter = GetFlockCenter();
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target = mousePos;
            targetDirection = (target - flockCenter).normalized;
        }
        else
        {
            targetDirection = Vector2.zero;
        }
    }

    Vector3 GetFlockCenter()
    {
        //calculate the center of the flock
        Vector3 sumPosition = Vector3.zero;
        foreach (GameObject zuzu in zuzus)
        {
            sumPosition += zuzu.transform.position;
        }

        flockCenter = sumPosition / zuzus.Length;

        return flockCenter; 
    }

    public Vector2 FlockCenter()
    {
        return flockCenter;
    }

    public Vector2 FlockTarget()
    {
        return targetDirection; 
    }

    public GameObject[] Zuzus()
    {
        return zuzus;
    }
}

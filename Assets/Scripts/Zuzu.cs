using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zuzu : MonoBehaviour
{
    //flock movement parameters
    float weightTowardCenter = 1f;
    float weightAvoidNeighbors = 1f;
    float weightToTarget = 0.1f;
    float distToNeighbors = 2f;
    float forceMultiplier = 0.5f; 

    //jump Parameters; 
    float jumpInterval = 10;
    float jumpSpeed = 1f;
    float jumpDecel = 0.0075f; 

    //constants;
    float pixelDist = 1f / 16f;
    
    //sprite object to render the sprite
    Transform renderObject;


    //pre build
    float yOffset = 0f;
    int framesToJump;
    bool jumping = false; 
    float jumpVelocity = 0f;
    Vector3 offset;
    Rigidbody2D rb;
    Vector2 movementVector;
    public GameObject flock;
    FlockManager flockManager;
    Vector2 force; 

    // Start is called before the first frame update
    void Start()
    {
        //get important components 
        rb = this.GetComponent<Rigidbody2D>();
        flockManager = flock.GetComponent<FlockManager>();

        // get the sprite render object of child 
        renderObject = this.transform.Find("render");


        framesToJump = NextJump();
        
    }

    // Update is called once per frame
    void Update()
    {
        //do all the random jumping calculations
        JumpUpdate();

        //get movement vector 
        movementVector = GetMovementVector();

        //update the sprite position 
        renderObject.position = this.transform.position + offset;
    }

    private void FixedUpdate()
    {
        
        rb.MovePosition((Vector2) transform.position + (movementVector) * Time.deltaTime);
    }

    int NextJump()
    {
        float timeToJump = Random.Range(0, jumpInterval); 
        int framesToJump = (int) Mathf.Ceil(timeToJump / Time.deltaTime);
        return framesToJump; 
    }

    void JumpUpdate()
    {
        if (framesToJump > 0)
            framesToJump--;

        if ((framesToJump == 0) && !jumping)
        {
            // add jump velocity and start new jump; 
            jumping = true;
            jumpVelocity = Random.Range(0f, jumpSpeed);
            framesToJump = -1;

        }

        //change y_offset and deceleratting the jumping velocity 
        yOffset += jumpVelocity;
        if (jumping)
        {
            jumpVelocity -= jumpDecel;
        }

        //reset y_offset and jumpVelocity 
        if ((yOffset < 0.1) && jumping)
        {
            yOffset = 0;
            jumpVelocity = 0;
            jumping = false;
            //get a new jump 
            framesToJump = (int)(1 / Time.deltaTime) + NextJump();
        }


        offset = new Vector3(0, yOffset * pixelDist, 0);

    }

    Vector2 GetMovementVector()
    {
        //move to the center of the flock 
        Vector2 flockCenter = flockManager.FlockCenter();
        Vector2 flockCenterDirection = flockCenter - (Vector2) this.transform.position;
        Vector2 flockTarget = flockManager.FlockTarget();

        GameObject[] listZuzus = flockManager.Zuzus();
        
        //calculate distance to all neighbors
        Vector3 sumVecToNeighbor = Vector3.zero;
        float numNeighbors = 0; 
        foreach (GameObject other in listZuzus)
        {
            if ((other != this.gameObject))
            {
                if (Vector3.Distance(this.transform.position, other.transform.position) < distToNeighbors)
                {
                    sumVecToNeighbor += (other.transform.position - this.transform.position);
                    numNeighbors += 1; 
                }
            }
        }
        Debug.Log(numNeighbors);
        if (numNeighbors > 0)
            sumVecToNeighbor /= -numNeighbors; ;


        force += (weightTowardCenter * flockCenterDirection + weightToTarget* flockTarget) + weightAvoidNeighbors* (Vector2)sumVecToNeighbor;
        force *= forceMultiplier;
        Vector2 move = force;

        Debug.DrawRay(this.transform.position, flockCenterDirection, Color.red);
        Debug.DrawRay(this.transform.position, flockTarget, Color.green);
        Debug.DrawRay(this.transform.position, sumVecToNeighbor, Color.blue);
        Debug.DrawRay(this.transform.position, move, Color.white);

        return move;
    }
}

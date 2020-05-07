using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zuzu : MonoBehaviour
{
    //flock movement parameters
    float weightTowardCenter = 0.1f;
    float weightAvoidNeighbors = 1f;
    float weightToTarget = 2f;
    
    float distToNeighbors = 1f;
    float distToCenter = 2.5f;

    float forceMultiplier = 0.90f;
    float moveThreshold = 0.5f;

    float randomMoveStrength = 2;
    float randomMoveMaxInterval = 10;

    float maxForce = 12f; 

    //jump Parameters; 
    float jumpInterval = 20;
    float jumpSpeed = 0.5f;
    float jumpDecel = 0.0075f; 

    //constants;
    float pixelDist = 1f / 16f;
    
    //sprite object to render the sprite
    Transform renderObject;


    //pre build
    float yOffset = 0f;
    int framesToJump;
    bool jumping = false;
    bool randMoveActive = false;
    float jumpVelocity = 0f;
    Vector3 offset;
    Rigidbody2D rb;
    Vector2 movementVector;
    public GameObject flock;
    FlockManager flockManager;
    Vector2 force;
    Vector2 flockTarget = Vector2.zero;
    int framesToNextRandMove;
    bool moveToOldTarget = false;
    Vector2 oldTarget;
    Vector2 randomMoveVec = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        //get important components 
        rb = this.GetComponent<Rigidbody2D>();
        flockManager = flock.GetComponent<FlockManager>();

        // get the sprite render object of child 
        renderObject = this.transform.Find("render");

        framesToJump = NextJump();
        framesToNextRandMove = NextRandMove(); 
    }

    // Update is called once per frame
    void Update()
    {
        //do all the random jumping calculations
        JumpUpdate();

        //get movement vector 
        movementVector = GetMovementVector();
        randomMoveVec = GetRandomMovement();
        movementVector += randomMoveVec;
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

    int NextRandMove()
    {
        float timeToMove = Random.Range(0, randomMoveMaxInterval);
        int framesToMove = (int)Mathf.Ceil(timeToMove / Time.deltaTime);
        return framesToMove;
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
        Vector2 flockCenterDirection = Vector2.zero;
        if (Vector2.Distance(this.transform.position, flockCenter) > distToCenter)
        {
            flockCenterDirection = flockCenter - (Vector2)this.transform.position;
        }
        
        Vector2 flockTarget = Vector2.zero;
        
        // get the move to target vector
        if (Input.GetMouseButton(0))
        {
            moveToOldTarget = false;
            flockTarget = (flockManager.FlockTarget() - (Vector2)this.transform.position).normalized;
            
            if (flockTarget.magnitude < moveThreshold )
            {
                flockTarget = Vector2.zero;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            moveToOldTarget = true;
            oldTarget = flockManager.FlockCenter();
        }

        if (moveToOldTarget && (Vector2.Distance(this.transform.position, oldTarget) > distToCenter))
        {
            flockTarget = (oldTarget - (Vector2)this.transform.position).normalized;
        }
        
        flockTarget *= forceMultiplier;

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

        if (numNeighbors > 0)
            sumVecToNeighbor /= -numNeighbors; 

        // add some random movement 

        //add all forces together to the movement vector 
        force += (weightTowardCenter * flockCenterDirection + weightToTarget* flockTarget) + weightAvoidNeighbors* (Vector2)sumVecToNeighbor;
        //force *= 0.95f; 
        float forceMagnitude = force.magnitude; 
        if (forceMagnitude > maxForce)
        {
            force /= forceMagnitude;
            force *= maxForce;
        }
        //force *= forceMultiplier;
        Vector2 move = force;

        Debug.DrawRay(this.transform.position, flockCenterDirection, Color.red);
        Debug.DrawRay(this.transform.position, flockTarget, Color.green);
        Debug.DrawRay(this.transform.position, sumVecToNeighbor, Color.blue);
        Debug.DrawRay(this.transform.position, move, Color.white);

        return move;
    }

    Vector2 GetRandomMovement()
    {   

        if (framesToNextRandMove > 0)
            framesToJump--;

        if ((framesToNextRandMove == 0) && !randMoveActive)
        {
            // add jump velocity and start new jump; 
            randMoveActive = true;
            float randX = Random.Range(0f, randomMoveStrength);
            float randY = Random.Range(0f, randomMoveStrength);
            randomMoveVec = new Vector2(randX, randY);
            framesToNextRandMove = -1;

        }

        //reset y_offset and jumpVelocity 
        if ((randomMoveVec.magnitude < 0.1) && randMoveActive)
        {
            randomMoveVec = Vector2.zero;
            randMoveActive = false;
            //get a new jump 
            framesToNextRandMove = (int)(1 / Time.deltaTime) + NextRandMove();
        }

        return Vector2.zero;
    }
}

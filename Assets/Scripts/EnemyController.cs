using UnityEngine;
using System.Collections;

/*
 * List of selectable enemy types 
 */
public enum EnemyType
{
    stationary, patrol
}

/*
 * List of posible states an enemy can be in
 */
public enum State
{
    idle, attack
}

public class EnemyController : MonoBehaviour {
    public EnemyType type;
    private State _state = State.idle; // Local variable to represent our state
    
    // Target (usually the player)
    public string targetLayer = "Player";   // TODO: Make this a list, for players and friendly NPC's
    public GameObject target;

    // Firing Projectiles
    public Transform firePoint;             // Point from which the enemy fires
    public GameObject projectilePrefab;     // Projectile
    public float projectileSpeed = 5;       // Speed of the projectile
    public float projectileLifeTime = 2;    // How long the projectile exists before selfdestructing
    public float fireDelay = 3;             // Time between shots
    
    // Spot
    public float spotRadius = 3;            // Radius in which a player can be spotted
    public bool drawSpotRadiusGismo = true; // Visual aid in determening if the spot radius
    private Collider2D[] collisionObjects;
    public bool playerSpotted = false;      // Debug purposes, to see in the editor if an enemy spotted the player
    
    // Shoot
    private GameObject projectile;          // Selected projectile, should handle selfdestruct and damage
    private float timeToFire = 0;           // Future date to trigger shot
    public bool playerIsLeft;               // Simple check to see if the player is left to the enemy, important for facing.
    private bool facingLeft = true;         // For determining which way the player is currently facing.

    // Patrol
    public float walkSpeed = 1f;            // Amount of velocity
    private bool walkingRight;              // Simple check to see in what direction the enemy is moving, important for facing.
    public float collideDistance = 0.6f;    // Distance from enemy to check for a wall.
    private bool colliding = false;         // If true, it touched a wall and should flip.

    void FixedUpdate()
    {
        switch (_state)
        {
            case State.idle:
                Idle();
                break;
            case State.attack:
                Attack();
                break;
        }
    }

    /*
     * Idle state
     * 
     * In this state, the enemy will wait to spot a player, and then it will go to its attack state.
     * Patroling enemys will resume to patrol after it shot at the player, as the attack state
     * will reset the timer. The first time the patroling enemy spots an enemy, the timer will
     * already have passed and it will immediately go into the attack state.
     */
    private void Idle()
    {
        // Sends the patroling enemy to patrol
        if (type == EnemyType.patrol)
        {
            Patrol();
        }

        // Will set 'playerSpotted' to true if spotted
        IsPlayerInRange();
        if (playerSpotted)
        {
            if (type == EnemyType.stationary)
            {
                timeToFire = Time.time + fireDelay;
                _state = State.attack;
                Debug.Log("Switch to Attack!");
            }
            else if (type == EnemyType.patrol)
            {
                // This delay is so that the enemy will resume patrol after shooting at the player
                if (Time.time > timeToFire)
                {
                    timeToFire = Time.time + (fireDelay/2);
                    _state = State.attack;
                    Debug.Log("Switch to Attack!");
                }
            }
        }
    }

    /*
     * Patrol script for enemy, 
     * will walk untill the linecast hits a collider, then walk the other way
     */
    private void Patrol()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(walkSpeed, GetComponent<Rigidbody2D>().velocity.y);

        FaceDirectionOfWalking();

        colliding = Physics2D.Linecast(
            new Vector2((this.transform.position.x + collideDistance), (this.transform.position.y - (GetComponent<SpriteRenderer>().bounds.size.y / 4))),
            new Vector2((this.transform.position.x + collideDistance), (this.transform.position.y + (GetComponent<SpriteRenderer>().bounds.size.y / 2))),
            ~( 
                (1 << LayerMask.NameToLayer(targetLayer)) + 
                (1 << LayerMask.NameToLayer("EnemyProjectile")) 
            ) // Collide with all layers, except the targetlayer and the enemy projectiles
        );

        if (colliding)
        {
            Debug.Log(this.name + " hit a wall, now walking the other way.");
            walkSpeed *= -1;
            collideDistance *= -1;
        }
    }

    /*
     * This method makes sure the enemy will be facing the direction it is going in
     */
    private void FaceDirectionOfWalking()
    {
        if (GetComponent<Rigidbody2D>().velocity.x > 0)
        {
            walkingRight = true;
        }
        else
        {
            walkingRight = false;
        }
        if (walkingRight && facingLeft)
        {
            Flip();
        }
        else if (!walkingRight && !facingLeft)
        {
            Flip();
        }
    }

    /*
     * Checks to see if an entity of the "Player" layer has entered the range of the enemy.
     * 
     * Gets a list colliders that collided with the overlapcircle and uses the first result to 
     * become the target of the enemy. This is so that you don't have to manually add the target to every enemy
     * and will help when multiplayer is implemented
     */
    private void IsPlayerInRange()
    {
        collisionObjects = Physics2D.OverlapCircleAll(this.transform.position, spotRadius, 1 << LayerMask.NameToLayer(targetLayer));

        if (collisionObjects.Length > 0)
        {
            target = collisionObjects[0].gameObject;
            playerSpotted = true;
        }
        else
        {
            playerSpotted = false;
        }
    }

    /*
     * Attack!
     * 
     * The enemy spots its target and will attack it.
     * First it will face the player, wait for the delay to run out and then shoot
     * before going back to the idle state. 
     * */
    private void Attack()
    {
        // Patroling enemy needs to stop moving before shooting.
        // This enemy will resume patrol in the idle state
        if (type == EnemyType.patrol)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        }

        FacePlayer();

        if (Time.time > timeToFire)
        {
            Debug.Log("Shooting!");
            Shoot();
            timeToFire = Time.time + fireDelay;
            _state = State.idle;
        }
    }
    /*
     * Script to make the enemy face the player
     */
    private void FacePlayer()
    {
        //Player could be destroyed
        if (target != null)
        {
            playerIsLeft = target.transform.position.x < this.transform.position.x;

            if (!playerIsLeft && facingLeft)
            {
                Flip();
            }
            else if (playerIsLeft && !facingLeft)
            {
                Flip();
            }
        }
    }

    /*
     * Flips the sprite of the enemy the other way around so it will face left/right.
     * 
     * Used by both FacePlayer() and FaceDirectionOfWalking().
     */
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingLeft = !facingLeft;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        //Changes the speed to negative, making it fire the other way
        projectileSpeed = -projectileSpeed;
    }

    /*
     * Shoots a projectile in the direction the enemy is facing.
     * 
     * Auto destructs after lifetime has ended. 
     * Projectile should have a script attached to destruct it on collision.
     */
    private void Shoot()
    {
        projectile = (GameObject)Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        projectile.GetComponent<Rigidbody2D>().velocity = new Vector2((projectileSpeed * -1), GetComponent<Rigidbody2D>().velocity.y);
        Destroy(projectile, projectileLifeTime);
    }

    /*
     * Draws a circle gizmo to show the field of view or 'agro' range of an enemy
     */
    private void OnDrawGizmos()
    {
        if (drawSpotRadiusGismo)
        {
            Gizmos.color = Color.red;
            //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
            Gizmos.DrawWireSphere(this.transform.position, spotRadius);
        }
        // Draws the collision for the patrol enemies
        if (type == EnemyType.patrol)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                new Vector2((this.transform.position.x + collideDistance), (this.transform.position.y - (GetComponent<SpriteRenderer>().bounds.size.y / 4))),
                new Vector2((this.transform.position.x + collideDistance), (this.transform.position.y + (GetComponent<SpriteRenderer>().bounds.size.y / 2)))
                );
        }
    }
}
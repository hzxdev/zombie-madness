using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static GameplayManager;

public enum ZombieState
{
    Chasing,
    Wandering
}

public abstract class ZombieTemp : MonoBehaviour, IPoolable
{

    public ZombieState currentState;
    public ZombieSO zombieScriptable;
    internal GameplayManager gameplayManager;
    internal ObjectPooler objectPooler;
    internal NavMeshAgent agent;
    internal Transform target;
    internal Animator anim;
    internal float maxHealth;
    internal float setTargetPeriod;
    internal float tiredProgress;
    float setTargetPeriodTimestamp;
    public float zombieHearingRadius;
    Vector3 lastZombieHit;
    // [Range(0, 360)]

    Vector3 direction;
    Quaternion rotation;

   // public bool canSeePlayer;


    public GameObject fleshSmallImpactGO, fleshBigImpactGO;
   internal float health;
    internal Rigidbody rigid;
   internal bool alive;
    internal int damageMin, damageMax;



    //SOUNDS
    internal AudioSource source;
    public AudioClip[] zombieIdleSounds, zombieAttackSounds, zombieGetAngrySounds;
    public int zombieRandomTimeMin, zombieRandomTimeMax;
    float zombieSoundTimer;
   internal int zombieSoundRandomTime;
    public ParticleSystem standardModeSpawnEffect;

    float timeAfterLastKillToTriggerAZombie;



     public virtual void Start()
    {
        alive = true;


        health = maxHealth;

        anim = GetComponent<Animator>();
        anim.SetBool("alive", alive);
        anim.SetFloat("runningAnimSpeedMultp", 1);

        agent = GetComponent<NavMeshAgent>();
        source = GetComponent<AudioSource>();
        zombieSoundRandomTime = Random.Range(zombieRandomTimeMin, zombieRandomTimeMax);
        target = GameObject.FindGameObjectWithTag("Player").transform;
        gameplayManager = GameplayManager.instance;
        objectPooler = GameObject.FindObjectOfType<ObjectPooler>();
        rigid = GetComponent<Rigidbody>();
        timeAfterLastKillToTriggerAZombie = instance.levels[instance.currentLevelId].timeAfterLastKillToTriggerAZombie;
        StartWandering();
        
       
        Initialize();


    }

   

    public virtual void Initialize() //Apply scriptable object values and shit
    {
        if(currentState == ZombieState.Chasing)
        agent.speed = zombieScriptable.chasingSpeed;
        else if (currentState == ZombieState.Wandering)
            agent.speed = zombieScriptable.wanderingSpeed;
        maxHealth = zombieScriptable.health;
        health = maxHealth;
        damageMin = zombieScriptable.damageMin;
        damageMax = zombieScriptable.damageMax;
        anim.runtimeAnimatorController = zombieScriptable.animOverride;
        setTargetPeriod = zombieScriptable.setTargetPeriod;
        

    }



    public virtual void Update()
    {
        if (!alive)
            return;



       if(tiredProgress > zombieScriptable.chasingStamina) //zombie gets tired after a lot of  chasing
        {
            if(Vector3.Distance(transform.position, target.position) > 10) // if there is a certain distance ofc
            {
                SlowDown(7);
                Invoke(nameof(StartWandering), 7);
                tiredProgress = 0;
            }
            
        }
        
        DoRandomSounds();

        if(Input.GetKeyDown(KeyCode.G))
        {
            StartWandering();
        }

        if(currentState == ZombieState.Chasing)
        {
            setTargetPeriodTimestamp += Time.deltaTime;
            tiredProgress += Time.deltaTime;

            if (setTargetPeriodTimestamp > setTargetPeriod)
            {
                SetPlayerAsTarget();
                setTargetPeriodTimestamp = 0;
            }

            if (Vector3.Distance(transform.position, target.position) <= agent.stoppingDistance)
            {
                WhenInAttackRange();
            }

            else
            {

                WhenOutAttackRange();
            }
        } else if(currentState == ZombieState.Wandering)
        {
           
            
            if (agent.remainingDistance <= agent.stoppingDistance ||agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                //Change path

                agent.SetDestination(GetRandomNavMeshLocation());

            }
                

        }
        
        if(gameplayManager.forHowMuchTimeAZombieWasntDead > timeAfterLastKillToTriggerAZombie)
        {
            StartChasing();
            gameplayManager.forHowMuchTimeAZombieWasntDead = 0;
        }
    }

    void SetDestinationToRandomPoint()
    {
        if (!alive)
            return;

        agent.SetDestination(GetRandomNavMeshLocation());
        anim.SetBool("lookAround", false);
    }

    public virtual void WhenInAttackRange()
    {
        anim.SetBool("isStopped", true);
        agent.isStopped = true;
        RotateToTarget();   
       
    }

    public virtual void WhenOutAttackRange()
    {
        anim.SetBool("isStopped", false);
        agent.isStopped = false;

    }

    public void SetPlayerAsTarget()
    {
        agent.SetDestination(new Vector3(target.position.x, target.position.y, target.position.z));
    }

    public void RotateToTarget()
    {
       direction = new Vector3(target.position.x - transform.position.x, 0 /*- transform.position.y*/, target.position.z - transform.position.z);
        rotation = Quaternion.LookRotation(direction, transform.up);
        transform.rotation = rotation;
    }

    public virtual void SlowDown(float duration)
    {

       
            agent.speed = zombieScriptable.chasingSpeed * 0.35f;
        anim.SetFloat("runningAnimSpeedMultp", 0.35f);
            CancelInvoke(nameof(StopSlowDown));
            Invoke(nameof(StopSlowDown), duration);
        
    }

   public void StopSlowDown()
    {
        if (currentState == ZombieState.Chasing)
            agent.speed = zombieScriptable.chasingSpeed;
        if (currentState == ZombieState.Wandering)
            agent.speed = zombieScriptable.wanderingSpeed;
        anim.SetFloat("runningAnimSpeedMultp", 1);
    }

    public virtual void TakeDamage(int damage, BodyPart.Part part, Vector3 effectPos, Quaternion effectRot, bool alertNearby)
    {
        tiredProgress = 0;
        CancelInvoke(nameof(StartWandering));
        if(currentState == ZombieState.Wandering)
        StartChasing(); //vurulunca eðer dolaþýyorsa chasing moduna geçecek
        SlowDown(GunController.instance.GetCurrentWeapon().zombieSlowDownTime); // bi yavaþlicak vurulunca
        if (part == BodyPart.Part.Head)
        {
            GameObject impactGO = Instantiate(fleshBigImpactGO, effectPos, effectRot);
            Destroy(impactGO, 3);
        }
        else
        {
            GameObject impactGO = Instantiate(fleshSmallImpactGO, effectPos, effectRot);
            Destroy(impactGO, 3);
        }

        

        if (alive)
        {
            health -= damage;

            HurtAnim();
            if (health <= 0)
            {
                Die(part, false);
                health = 0;
            }
        }

        if(alertNearby)
        {
           
            
            lastZombieHit = effectPos;


            foreach (var col in Physics.OverlapSphere(effectPos, zombieHearingRadius))
            {
                Debug.Log("col.transform.root.name = " + col.transform.root.name);
                if (col.transform.root.name != GameplayManager.instance.RootPoolName)
                {
                    Debug.Log("Yok");
                    return;
                }

                Debug.Log(col.GetComponentInParent<ZombieTemp>().gameObject.name + " chasing");
                col.GetComponentInParent<ZombieTemp>().StartChasing();
             
            }

        }

    }

    public virtual void HurtAnim()
    {
        anim.SetTrigger("hit");
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(lastZombieHit, zombieHearingRadius);
    }

    public virtual void DropRandom()
    {
        int r = Random.Range(0, 6);
        if (r == 5)
        {
            Pickup randomPickup = WeaponsList.instance.pickups[Random.Range(0, WeaponsList.instance.pickups.Length)];
            if (randomPickup.pickupType == PickupType.Weapon) // if weapon pickup should be spawned a bit higher
                Instantiate(randomPickup.prefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
            else
                Instantiate(randomPickup.prefab, transform.position, Quaternion.identity);
        }

    }

    public virtual void Die(BodyPart.Part shotFromWhere, bool killedByGame)
    {
        alive = false;

        if(!killedByGame)
        {
            gameplayManager.AddKill();
            gameplayManager.coinsCollectedInDay += zombieScriptable.killedCoin;
            gameplayManager.survivalScore += zombieScriptable.killedCoin * 10;
            gameplayManager.UpdateCoinsCollected();
            gameplayManager.UpdateSurvivalScore();

            //drop random shit 1/6 chance
            DropRandom();
           
        }

        gameplayManager.forHowMuchTimeAZombieWasntDead = 0;
        agent.enabled = false;
        gameplayManager.aliveEntities--;
        source.Stop();
        Invoke(nameof(ReturnToPool), 3);
        anim.SetBool("alive", alive);
        switch (shotFromWhere)
        {
            case BodyPart.Part.Head:
                anim.SetTrigger("fallingBack");
                break;
            case BodyPart.Part.Torso:
                anim.SetTrigger("fallingBack");
                break;
            case BodyPart.Part.Limb:
                anim.SetTrigger("fallingBack");
                break;
        }







    }

    internal void ReturnToPool()
    {
        GameplayManager.instance.Despawn(gameObject);
      /*  gameObject.SetActive(false);
        GameplayManager.Pool pool;
        for (int i = 0; i < gameplayManager.pools.Count; i++)
        {
           if( gameplayManager.pools[i].tag == zombieScriptable.zombieName)
            {
                pool = gameplayManager.pools[i];
                break;
            }

        }
        */

    }

    public virtual void DoRandomSounds()
    {
        if (!source.isPlaying)
            zombieSoundTimer += Time.deltaTime;

        if (zombieSoundTimer >= zombieSoundRandomTime)
        {
            zombieSoundTimer = 0;
            source.PlayOneShot(zombieIdleSounds[(int)Random.Range(0, zombieIdleSounds.Length)]);
            zombieSoundRandomTime = Random.Range(zombieRandomTimeMin, zombieRandomTimeMax);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform.CompareTag("Beartrap"))
        {
            if (!col.transform.GetComponent<Beartrap>().isClosed)
            {
                SlowDown(4);
                TakeDamage(35, BodyPart.Part.Limb, col.transform.position, Quaternion.identity, false);
            }

            col.transform.GetComponent<Beartrap>().Close();
        }
    }
    public virtual void ZombieAttackMoment()
    {
        if(!gameplayManager.levelCompleted) //level bitiþ ekranýndayken vuramasýnlar
        PlayerHealth.instance.PlayerTakeDamage(Random.Range(damageMin, damageMax));
    }

    internal void PlayZombieAttackSound()
    {
        source.PlayOneShot(zombieAttackSounds[Random.Range(0, zombieAttackSounds.Length)]);
    }

    public void StartChasing()
    {
        source.PlayOneShot(zombieGetAngrySounds[Random.Range(0, zombieGetAngrySounds.Length)]);
        currentState = ZombieState.Chasing;
        agent.speed = zombieScriptable.chasingSpeed;
        anim.SetBool("chaseMode", true);
        anim.SetBool("wanderMode", false);
    }

    public void StartWandering()
    {
        tiredProgress = 0;
        currentState = ZombieState.Wandering;
        agent.speed = zombieScriptable.wanderingSpeed;
        agent.SetDestination(GetRandomNavMeshLocation());
        anim.SetBool("wanderMode", true);
        anim.SetBool("chaseMode", false);

    }

 public void PlayerEnteredFieldOfVision()
    {
        StartChasing();

    }
   

    private Vector3 GetRandomNavMeshLocation()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        int maxIndices = navMeshData.indices.Length - 3;

        // pick the first indice of a random triangle in the nav mesh
        int firstVertexSelected = UnityEngine.Random.Range(0, maxIndices);
        int secondVertexSelected = UnityEngine.Random.Range(0, maxIndices);

        // spawn on verticies
        Vector3 point = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];

        Vector3 firstVertexPosition = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];
        Vector3 secondVertexPosition = navMeshData.vertices[navMeshData.indices[secondVertexSelected]];

        // eliminate points that share a similar X or Z position to stop spawining in square grid line formations
        if ((int)firstVertexPosition.x == (int)secondVertexPosition.x || (int)firstVertexPosition.z == (int)secondVertexPosition.z)
        {
            point = GetRandomNavMeshLocation(); // re-roll a position - I'm not happy with this recursion it could be better
        }
        else
        {
            // select a random point on it
            point = Vector3.Lerp(firstVertexPosition, secondVertexPosition, UnityEngine.Random.Range(0.05f, 0.95f));
        }

        return point;
    }

    public virtual void Spawn()
    {
        Debug.Log("Zombie Spawn()");
        GameplayManager.instance.aliveEntities++;
        alive = true;
        GetComponent<Animator>().SetBool("alive", alive);
        GetComponent<NavMeshAgent>().enabled = true;
        timeAfterLastKillToTriggerAZombie = instance.levels[instance.currentLevelId].timeAfterLastKillToTriggerAZombie;

        // Invoke(nameof(StartChasing), 20); //replaced with forHowMuchTimeAZombieWasntDead


        health = maxHealth;
        tiredProgress = 0;
        CancelInvoke(nameof(StartWandering));
        standardModeSpawnEffect.Play();
    }

    public virtual void Despawn()
    {
        CancelInvoke(nameof(StartChasing));

        Debug.Log("Zombie Despawn()");
    }

}

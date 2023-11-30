using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    // We will need this variable for condition based delays after
    public float health = 100.0f;

    // Some sequences which we will save to stop them after manually
    private DelayManager.Sequence tick;
    private DelayManager.Sequence condition;

    void Start()
    {
        
        // Let's create a basic sequence where we will throw a ball at specific directions
        // As you can see after adding a method with .Add then we add a delay with .Delay that will wait for x seconds before calling the second method we added with .Add and so on
        // We call .Invoke at the end to call the sequence
        // You can add a initial delay if you want to wait for some time before the sequence is started
        // Note: the initial delay get's called only once
        DelayManager.instance.BuildSequence()
            .InitialDelay(5.0f)
            .Add(x => ThrowBall(this.transform.forward))
            .Delay(1.0f)
            .Add(x => ThrowBall(this.transform.right))
            .Delay(0.5f)
            .Add(x => ThrowBall(this.transform.forward))
            .Delay(0.5f)
            .Add(x => ThrowBall(-this.transform.right))
            .Delay(0.5f)
            .Add(x => ThrowBall(this.transform.up))
            .Delay(0.5f)
            .Add(x => ThrowBall(-this.transform.forward))
            .Invoke();


        // Let's create a basic sequence which will get repeated every 0.5f for 5 times
        // This is the same principle as the previous example but instead of calling the sequence only once, we call it 5 times with a delay of 0.5f between each repeat
        DelayManager.instance.BuildSequence()
            .Add(x => SpawnMuzzleFlashParticle(this.transform.position))
            .Delay(0.1f)
            .Add(x => FireBullet())
            .Repeat(5, 0.5f);

        // Let's create a basic sequence that will get constantly called every 1.0f seconds
        // In this case we create a sequence with only one method, but you can pass as many as you want
        // After calling Tick this method will continue forever unless we cancel it after
        // Check the code below to see how to cancel it
        tick = DelayManager.instance.BuildSequence()
            .Add(x => Print("Ticking"))
            .Tick(1.0f);


        // Let's create a queue of sequences that we will call later on
        // Same principle as the previous examples
        // But this time we call the .Queue method at the end
        DelayManager.instance.BuildSequence()
            .Add(x => SpawnEnemies(5))
            .Delay(1.0f)
            .Add(x => SpawnEnemies(30))
            .Queue();

        DelayManager.instance.BuildSequence()
            .Add(x => SpawnAllies(10))
            .Delay(1.0f)
            .Add(x => SpawnAllies(60))
            .Queue();

        // A tagged queued sequence
        DelayManager.instance.BuildSequence()
            .Add(x => FindEnemyBaseTarget())
            .Delay(1.0f)
            .Add(x => LaunchMissileToEnemyBase())
            .SetTag("Fire")
            .Queue();

        DelayManager.instance.BuildSequence()
            .Add(x => Print("Fire Sequence"))
            .SetTag("Fire")
            .Queue();

        // Let's create a sequence which will fire different methods with their own delays after a certain condition is met
        // In this case the condition is to check if the player is defeated or not
        // You can set the player's health to 0.0f during play by pressing K, which will change the health to 0.0f and thus call this sequence because we are listening to it
        condition = DelayManager.instance.BuildSequence()
            .Add(x => DestroyAllEnemies())
            .Delay(1.0f)
            .Add(x => SetGamePaused(true))
            .Delay(1.0f)
            .Add(x => SpawnGameOverUI())
            .Condition(() => IsPlayerDefeated());

        // Let's fire a basic condition listener which will get triggerred when the player's health will be set to 0.
        // Once the condition is met it will callback a SpawnGameOverUI() method which will simply print a string for testing purposes.
        DelayManager.instance.Condition(x => SpawnGameOverUI(), () => IsPlayerDefeated());
    }

    void Update()
    {
        #region HOW TO STOP SEQUENCES
        // If we press B we stop the condition based sequence to listen to the function IsPlayerDefeated()
        // By doing this you basically stop the entire sequence
        // Same principle can be applied with Invoke, Repeat and Tick
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Here we are stopping the condition listener which was set to fire a sequence during Start
            if (condition != null)
                DelayManager.instance.StopCondition(condition);
        }

        // Here we clear all the registered ticks
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Clearing all registered ticks
            DelayManager.instance.ClearTicks();
        }

        // Here we clear only the saved sequence where we know that there is a tick event running
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Clearing saved tick from sequence
            if (tick != null)
                DelayManager.instance.StopTick(tick);
        }
        #endregion

        #region CHANGING CONDITIONS TO FIRE EVENTS
        if (Input.GetKeyDown(KeyCode.K))
        {
            // Let's change the health to <= 0.0f which is requried for IsPlayerDefeated() to return true and trigger the condition
            health = 0.0f;
        }
        #endregion

        #region FIRE A SEQUENCE AFTER INPUT
        // Let's simulate firing some bullets from a rifle after we press F
        if (Input.GetKeyDown(KeyCode.F))
        {
            // First we spawn the muzzle flash particle at a X position, then after 0.1f seconds we fire the bullet, then we repeat this every 0.1 seconds for 5 times to simulate a semi automatic rifle
            DelayManager.instance.BuildSequence()
                .Add(x => SpawnMuzzleFlashParticle(this.transform.position))
                .Delay(0.1f)
                .Add(x => FireBullet())
                .Repeat(5, 0.1f);

            // Alternative method to repeat a single method without building a sequence
            //DelayManager.instance.Repeat(x => Print("Firing Bullet!"), 0.1f, 5);
        }
        #endregion

        #region SIMPLE DELAY
        // This is the method that you should call if you want to fire only one method after some time
        if (Input.GetKeyDown(KeyCode.H))
        {
            // Let's execute a simple delay without using sequences
            DelayManager.instance.Delay(x => Print("Hello World!"), 3.0f);
        }
        #endregion

        #region CALL QUEUED SEQUENCES
        // Call all queued sequences
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Calling all the queued sequences and then clearing, The 0.0f is the delay amount between each sequence, Set the boolean to false if you want to leave the queued sequences in memory so that you can call them again in the future
            DelayManager.instance.CallAllQueuedSequences(0.0f, true);
        }
        #endregion

        #region CALL TAGGED QUEUED SEQUENCES
        // Let's call the "Fire" tagged queued sequences
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Change 5.0f to 0.0f if you don't want any delays between each sequence call
            DelayManager.instance.CallQueuedSequencesWithTag("Fire", 5.0f, false);
        }

        #endregion

    }



    #region EXAMPLE METHODS
    // These are just placeholders and example methods
    // You can call any method with any number of parameters instead of these ones

    // Methods called from condition sequences
    private void DestroyAllEnemies()
    {
        Print("All enemies destroyed");
        // TODO
    }
    private void SetGamePaused(bool state)
    {
        if (state)
            Print("Game is paused");
        else
            Print("Game is resumed");
        // TODO
    }
    private void SpawnGameOverUI()
    {
        Print("Game over");
        // TODO
    }
    // The method we pass as condition checker
    private bool IsPlayerDefeated()
    {
        return health <= 0.0f ? true : false;
    }


    // Method called as example from the basic sequence
    private void ThrowBall(Vector3 direction)
    {
        Print("Throwing ball at " + direction + " direction");
        // TODO
    }

    // Methods called as example from the repeat sequence
    private void FireBullet()
    {
        Print("Fire!");
        // TODO
    }
    private void SpawnMuzzleFlashParticle(Vector3 position)
    {
        Print("Spawned muzzle flash particle");
        // TODO
    }

    // Methods called as example to show the queue feature
    private void SpawnEnemies(int count)
    {
        Print("Spawned " + count + " enemies");
        // TODO
    }
    private void SpawnAllies(int count)
    {
        Print("Spawned " + count + " allies");
        // TODO
    }

    private void FindEnemyBaseTarget()
    {
        Print("Searching enemy base");
        // TODO
    }
    private void LaunchMissileToEnemyBase()
    {
        Print("Launcing missile to enemy base");
        // TODO
    }

    // A simple debug log
    private void Print(string message)
    {
        Debug.Log("Message: " + message);
    }
    
    #endregion
}

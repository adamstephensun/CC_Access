using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class DelayManager : MonoBehaviour
{
    // Singleton instance
    public static DelayManager instance;

    // Queues & Ticks
    public List<Sequence> sequences = new List<Sequence>();
    public List<Coroutine> ticks = new List<Coroutine>();

    #region SINGLETON
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion

    #region SIMPLE DELAYS
    public void Repeat(Action<object[]> method, float delay, int times)
    {
        method.Invoke(null);
        times--;

        StartCoroutine(Delayer(method, delay, times));
    }
    public void Delay(Action<object[]> method, float delay)
    {
        StartCoroutine(Delayer(method, delay));
    }
    public void Condition(Action<object[]> method, Func<bool> condition)
    {
        StartCoroutine(Delayer(method, condition));
    }

    // Simple IEnumerators
    IEnumerator Delayer(Action<object[]> method, float delay)
    {
        yield return new WaitForSeconds(delay);
        method.Invoke(null);
    }
    IEnumerator Delayer(Action<object[]> method, float delay, int times)
    {
        while (times > 0)
        {
            yield return new WaitForSeconds(delay);
            method.Invoke(null);
            times--;
        }
    }
    IEnumerator Delayer(Action<object[]> method, Func<bool> condition)
    {
        yield return new WaitUntil(condition);
        method.Invoke(null);
    }
    #endregion

    #region SEQUENCER
    public class Sequence
    {
        // Primitives
        private bool bInitialDelay;
        private float delay;
        private string tag;

        // The saved coroutines for this instance, in case we want to cancel it after
        public Coroutine invoke;
        public Coroutine repeat;
        public Coroutine condition;
        public Coroutine tick;

        // A list containing all the methods
        private Dictionary<Action<object[]>, float> methods;

        public Sequence()
        {
            this.methods = new Dictionary<Action<object[]>, float>();
            this.delay = 0.0f;
            this.bInitialDelay = false;
            this.invoke = null;
            this.repeat = null;
            this.condition = null;
            this.tick = null;
            this.tag = "";
        }

        public Sequence Add(Action<object[]> method)
        {
            this.methods.Add(method, 0.0f);
            return this;
        }

        public Sequence InitialDelay(float seconds)
        {
            this.delay = seconds;
            this.bInitialDelay = true;
            return this;
        }

        public Sequence Delay(float seconds)
        {
            this.methods[methods.LastOrDefault().Key] = seconds;
            return this;
        }

        public Sequence Queue()
        {
            DelayManager.instance.sequences.Add(this);
            return this;
        }

        public Sequence Invoke()
        {
            this.invoke = DelayManager.instance.SequenceBasicInvoke(this.methods, bInitialDelay, delay);
            return this;
        }

        public Sequence Repeat(float times, float interval)
        {
            this.repeat = DelayManager.instance.SequenceRepeatInvoke(this.methods, bInitialDelay, delay, times, interval);
            return this;
        }

        public Sequence Condition(Func<bool> condition)
        {
            this.condition = DelayManager.instance.SequenceConditionInvoke(this.methods, condition);
            return this;
        }

        public Sequence Tick(float interval)
        {
            this.tick = DelayManager.instance.SequenceTickInvoke(this.methods, interval);
            return this;
        }

        public Sequence SetTag(string tag)
        {
            this.tag = tag;
            return this;
        }
        public string GetTag()
        {
            return this.tag;
        } 
    }

    // Sequence Builder
    public Sequence BuildSequence()
    {
        return new Sequence();
    }

    // Sequence Calls
    private Coroutine SequenceBasicInvoke(Dictionary<Action<object[]>, float> methods, bool bInitialDelay, float delay)
    {
        var coroutine = StartCoroutine(Invoker(methods, bInitialDelay, delay));
        return coroutine;
    }
    private Coroutine SequenceRepeatInvoke(Dictionary<Action<object[]>, float> methods, bool bInitialDelay, float delay, float times, float interval)
    {
        var coroutine = StartCoroutine(Repeater(methods, bInitialDelay, delay, times, interval));
        return coroutine;
    }
    private Coroutine SequenceConditionInvoke(Dictionary<Action<object[]>, float> methods, Func<bool> condition)
    {
        var coroutine = StartCoroutine(Conditioner(methods, condition));
        return coroutine;
    }
    private Coroutine SequenceTickInvoke(Dictionary<Action<object[]>, float> methods, float interval)
    {
        var coroutine = StartCoroutine(Ticker(methods, interval));
        ticks.Add(coroutine);
        return coroutine;
    }

    // Helpers
    public void CallAllQueuedSequences(float delay, bool clear)
    {
        StartCoroutine(Queue(this.sequences, delay));

        if (clear)
            sequences.Clear();
    }
    public void CallQueuedSequencesWithTag(string tag, float delay, bool clear)
    {
        List<Sequence> taggedSequences = new List<Sequence>();

        foreach (var s in sequences)
        {
            if (s.GetTag().Equals(tag))
            {
                taggedSequences.Add(s);

                if (clear)
                    sequences.Remove(s);
            }           
        }

        StartCoroutine(Queue(taggedSequences, delay));
    }
    public void StopAllQueuedSequences()
    {
        foreach (var s in sequences)
        {
            StopInvoke(s);
        }
    }
    public void StopAllQueuedSequencesWithTag(string tag)
    {
        foreach (var s in sequences)
        {
            if (s.GetTag().Equals(tag))
            {
                StopInvoke(s);
            }
        }
    }
    public Sequence CallFirstQueuedSequence()
    {
        var sequence = sequences.FirstOrDefault();

        if (sequence != null)
            sequence.Invoke();

        return sequence;
    }
    public Sequence CallLastQueuedSequence()
    {
        var sequence = sequences.LastOrDefault();

        if (sequence != null)
            sequence.Invoke();

        return sequence;
    }
    public Sequence CallFirstQueuedSequenceWithTag(string tag)
    {
        var sequence = sequences.Where(x => x.GetTag().Equals(tag)).FirstOrDefault();

        if (sequence != null)
            sequence.Invoke();

        return sequence;
    }
    public Sequence CallLastQueuedSequenceWithTag(string tag)
    {
        var sequence = sequences.Where(x => x.GetTag().Equals(tag)).LastOrDefault();

        if (sequence != null)
            sequence.Invoke();

        return sequence;
    }
    public void ClearTicks()
    {
        foreach (var t in ticks)
        {
            StopCoroutine(t);
        }
        ticks.Clear();
    }

    // Stopping started invokes, repeats and ticks
    public void StopInvoke(Sequence sequence)
    {
        StopCoroutine(sequence.invoke);
    }
    public void StopRepeat(Sequence sequence)
    {
        StopCoroutine(sequence.repeat);
    }
    public void StopCondition(Sequence sequence)
    {
        StopCoroutine(sequence.condition);
    }
    public void StopTick(Sequence sequence)
    {
        StopCoroutine(sequence.tick);
    }


    // Sequence IEnumerators
    IEnumerator Invoker(Dictionary<Action<object[]>, float> methods, bool bInitialDelay, float delay)
    {
        if (bInitialDelay)
        {
            bInitialDelay = false;
            yield return new WaitForSeconds(delay);
        }

        foreach (var method in methods)
        {
            method.Key.Invoke(null);
            yield return new WaitForSeconds(method.Value);
        }
    }
    IEnumerator Repeater(Dictionary<Action<object[]>, float> methods, bool bInitialDelay, float delay, float times, float interval)
    {
        if (bInitialDelay)
        {
            bInitialDelay = false;
            yield return new WaitForSeconds(delay);
        }

        for (int i = 0; i < times; i++)
        {
            foreach (var method in methods)
            {
                method.Key.Invoke(null);
                yield return new WaitForSeconds(method.Value);
            }

            yield return new WaitForSeconds(interval);
        }
    }
    IEnumerator Conditioner(Dictionary<Action<object[]>, float> methods, Func<bool> condition)
    {
        yield return new WaitUntil(condition);

        foreach (var method in methods)
        {
            method.Key.Invoke(null);
            yield return new WaitForSeconds(method.Value);
        }
    }
    IEnumerator Ticker(Dictionary<Action<object[]>, float> methods, float interval)
    {
        while (true)
        {
            foreach (var method in methods)
            {
                method.Key.Invoke(null);
                yield return new WaitForSeconds(method.Value);
            }

            yield return new WaitForSeconds(interval);
        }
    }
    IEnumerator Queue(List<Sequence> sequences, float delay)
    {
        foreach (var s in sequences)
        {
            s.Invoke();
            yield return new WaitForSeconds(delay);
        }
    }
    #endregion

}

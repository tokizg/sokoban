using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class easeing
{
    float startTime;
    float duration;
    bool easeOut = false;

    public float value()
    {
        if (this.isDone())
            return 1;
        float time = Time.time - this.startTime;
        float t = time / this.duration;
        if (this.easeOut)
        {
            return 1 - (1 - t) * (1 - t);
        }
        else
        {
            return t * t;
        }
    }

    public bool isDone()
    {
        return Time.time - this.startTime > this.duration;
    }

    public easeing(float d, bool o)
    {
        this.startTime = Time.time;
        this.duration = d;
        this.easeOut = o;
    }
}

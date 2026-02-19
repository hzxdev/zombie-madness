using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beartrap : MonoBehaviour
{
    Animation anim;
    AudioSource source;
    public bool isPersistent;
    [HideInInspector]
    public bool isClosed;

    private void Start()
    {
        anim = GetComponent<Animation>();
        source = GetComponent<AudioSource>();
    }

    public void Close()
    {
        if(!isClosed)
        {
            anim.Play("TrapClose");
            Invoke(nameof(Open), 4);
            source.Play();
            isClosed = true;
        }
        

    }

    public void Open()
    {
        anim.Play("TrapOpen");
        isClosed = false;
    }
}

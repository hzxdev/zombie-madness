using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadAnimationEventParentCall : MonoBehaviour
{
    public string detachMagSoundName, insertMagSoundName, slideSoundName;
    GunController parentGC;
    

    // Start is called before the first frame update
    void Start()
    {
        parentGC = transform.GetComponentInParent<GunController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AnimationEventCompleted()
    {
        parentGC.ReloadComplete();
    }

    public void DetachMagSound()
    {
        if (!String.IsNullOrEmpty(detachMagSoundName))
            AudioManager.instance.PlayOneShot(detachMagSoundName);
    }

    public void InsertMagSound()
    {
        if (!String.IsNullOrEmpty(insertMagSoundName))
            AudioManager.instance.PlayOneShot(insertMagSoundName);
    }

    public void SlideSound()
    {
        if(!String.IsNullOrEmpty(slideSoundName))
        AudioManager.instance.PlayOneShot(slideSoundName);
    }
}

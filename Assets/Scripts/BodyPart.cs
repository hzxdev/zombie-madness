using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    // Assign this in the inspector
    public Part bodyPart;

    public enum Part
    {
        Head,
        Torso,
        Limb
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateSpawnpoint : MonoBehaviour
{
    new Renderer renderer;

    private void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    public bool IsVisible()
    {
        return renderer.isVisible;
    }
}

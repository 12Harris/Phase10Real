using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Test : NetworkBehaviour
{
    [SyncVar] public int Value;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowValue()
    {
        Debug.Log("value is: " + Value);
    }
}

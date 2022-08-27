using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class HandDisplay : NetworkBehaviour
{

    [SyncVar(hook = nameof(SetText))] public string text;
    

    void SetText(string oldText, string newText)
    {
        GetComponent<TextMeshPro>().text = newText;
    }
}

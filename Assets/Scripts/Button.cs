using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using System;

public class Button : NetworkBehaviour
{

    public static event Action<Button> OnClicked;

    [SyncVar]public int index = 0;
    [SyncVar(hook = nameof(OnTextChanged))] public string text;

    public void Clicked()
    {

        if(hasAuthority)
        {
            OnClicked?.Invoke(this);
        }
    }

    public void OnTextChanged(string oldText, string newText)
    {
        GameObject obj = transform.Find("Button/Text").gameObject;

        transform.Find("Button/Text").GetComponent<TextMeshProUGUI>().text = newText;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class UICardData : NetworkBehaviour
{

    [SyncVar(hook = nameof(SetColor))] public Color color;

    [SyncVar(hook = nameof(SetText))] public string text;

    [SyncVar] public int deckIndex;

    [SyncVar] public int HandIndex;

    public bool active;

    public Vector3 InitialPosition;

    public void Start()
    {
        DragDrop.OnStartedDrag += onStartedDrag;
    }

    void onStartedDrag(GameObject o)
    {
        if(o == this.gameObject)
        {
            active = true;
        }
    }

    private void OnDestroy()
    {
        DragDrop.OnStartedDrag -= onStartedDrag;
    }

    void SetColor(Color oldColor, Color newColor)
    {
        GetComponent<Image>().color = newColor;
    }

    void SetText(string oldText, string newText)
    {
        transform.Find("Text").GetComponent<TextMeshPro>().text = newText;
    }
}

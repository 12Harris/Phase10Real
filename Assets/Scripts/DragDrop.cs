using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using System;

public class DragDrop : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler, IDropHandler
{
    private RectTransform rectTransform;
    public bool dragging = false;
    public static event Action<GameObject> OnStartedDrag;
    public Vector3 lastPosition;
    public bool clicked;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        lastPosition = transform.position;
        clicked = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("On Pointer Down");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //if(hasAuthority)
        {
            Debug.Log("clicked");
            clicked = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(hasAuthority)
        {
            //startedDragging = true;
            //Debug.Log("started dragging");
            OnStartedDrag?.Invoke(this.gameObject);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //rectTransform.anchoredPosition += eventData.delta;
        //transform.localPosition += eventData.delta;
    	
        if(hasAuthority)
        {
            dragging = true;
            
            /*Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = transform.position.z;
            transform.position = worldPos;*/
            
        }
        
    }

    public void OnDrop(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(hasAuthority)
        {
            /*Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = transform.position.z;
            transform.position = worldPos;*/
            dragging = false;
        }
    }
}
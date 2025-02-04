using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FretButton : MonoBehaviour, IPointerDownHandler
{
    public bool pressed = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
    }

    public KeyCode key;
    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            pressed = true;
        }
    }
}

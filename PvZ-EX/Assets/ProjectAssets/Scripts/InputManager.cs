using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    [Tooltip("Allows debug messages for this script")]
    public bool allowDebug;

    
    public void OnMove(InputValue value)
    {
        if (value.Get<Vector2>() != Vector2.zero)
        {
            if (allowDebug)
            {
                print(value.Get<Vector2>() + "Pressed, OnMove");
            }
        }
        else
        {
            if (allowDebug)
            {
                print(value.Get<Vector2>() + "Not Pressed, OnMove");
            }
        }
    }
}

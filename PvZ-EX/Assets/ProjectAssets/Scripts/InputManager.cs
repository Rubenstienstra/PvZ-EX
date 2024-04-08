using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Tooltip("Allows debugging prints for this script")]
    public bool allowDebug;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnMove(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            if (allowDebug)
            {
                print(value.ReadValue<Vector2>() + "Performed, moveVector");
            }
        }
        if (value.canceled)
        {
            if (allowDebug)
            {
                print(value.ReadValue<Vector2>() + "Canceled, moveVector");
            }
        }
    }
}

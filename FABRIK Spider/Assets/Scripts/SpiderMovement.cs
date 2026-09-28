using UnityEngine;
using UnityEngine.InputSystem;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5f;
    private Vector3 direction = Vector3.up;
    [SerializeField] private SimpleFabrik[] legs;

    void Update()
    {
        HandleInput();
        
        bool groupAStepping = false;
        bool groupBStepping = false;

        for (int i = 0; i < legs.Length; i++)
        {
            bool isGroupA = i % 2 == 0;
            
            if (legs[i].takingStep) {
                if (isGroupA) groupAStepping = true;
                else groupBStepping = true;
            }
        }

        for (int i = 0; i < legs.Length; i++) 
        {
            bool isGroupA = (i % 2 == 0);

            if (isGroupA && !groupBStepping && legs[i].NeedsStep()) 
            {
                legs[i].TryStep();
                groupAStepping = true;
            }
            else if (!isGroupA && !groupAStepping && legs[i].NeedsStep()) 
            {
                legs[i].TryStep();
                groupBStepping = true; 
            }
        }
    }

    private void HandleInput()
    {
        if (Keyboard.current.wKey.isPressed) Go();
        else if (Keyboard.current.sKey.isPressed) Go(-1);

        if (Keyboard.current.aKey.isPressed)
        {
            transform.Rotate(0, 0, 100 * Time.deltaTime);
        } else if (Keyboard.current.dKey.isPressed)
        {
            transform.Rotate(0, 0, -100 * Time.deltaTime);
        }
        
        direction = transform.up;
    }

    private void Go(int i = 1)
    {
        transform.position += direction.normalized * (i * movementSpeed * Time.deltaTime);
    }
}

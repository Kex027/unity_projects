using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private int movementSpeed = 100;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(-movementSpeed, 0, 0));
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(movementSpeed, 0, 0));
        }
    }
}

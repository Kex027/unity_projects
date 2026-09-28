using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampColor : MonoBehaviour
{
    public GameObject light;
    [SerializeField] private Color startColor;
    [SerializeField] private Color playerIn;
    [SerializeField] private Color playerOut;
 
    private void Start() {
        light.GetComponent<Light>().color = startColor;
    }

    private void OnTriggerEnter(Collider other) {
        light.GetComponent<Light>().color = playerIn;
    }

    private void OnTriggerExit(Collider other) {
        light.GetComponent<Light>().color = playerOut;
    }
}

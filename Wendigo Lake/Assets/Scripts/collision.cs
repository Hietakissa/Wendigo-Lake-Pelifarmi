using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision : MonoBehaviour
{

    void OnTriggerEnter(Collider collision)
    {
            Debug.Log("entered");
            
    }
    void OnTriggerExit(Collider collision)
    {
            Debug.Log("left");
    }
}

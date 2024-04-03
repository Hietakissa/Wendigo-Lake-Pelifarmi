using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class collision1 : MonoBehaviour
{
    
    void OnTriggerEnter(Collider collision)
    {
            Debug.Log("entered");
            SceneManager.LoadScene(6);
            
    }
    void OnTriggerExit(Collider collision)
    {
            Debug.Log("left");
    }
    

}

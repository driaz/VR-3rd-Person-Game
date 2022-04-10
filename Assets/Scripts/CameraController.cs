using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //declare variables
    public GameObject player;
    public Camera mainCamera;
    private Transform playerTransform;
    private Vector3 playerPosition;
  

    // Start is called before the first frame update
    void Awake()
    {
        playerTransform = player.GetComponent<Transform>();

       
    }

    // Update is called once per frame
    void Update()
    {
       
        //playerPosition = playerTransform.position;
        mainCamera.gameObject.transform.position = player.transform.position;
        Debug.Log("the main camera position is " + mainCamera.transform.position);
        Debug.Log("the player position is " + player.transform.position);


      

    }
}

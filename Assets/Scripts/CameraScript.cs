using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraScript : MonoBehaviour
{
    public Transform attachedPlayer;
    public bool followMode;
    public AudioSource levelMusic;
    Camera thisCamera;
    // Use this for initialization
    void Start()
    {
        thisCamera = GetComponent<Camera>();
        if (levelMusic != null) { levelMusic.Play(); }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (followMode)
        {
            
            Vector3 player = attachedPlayer.transform.position;
            Vector3 newCamPos = new Vector3(player.x, player.y, transform.position.z);
            transform.position = newCamPos;
        }
    }

}

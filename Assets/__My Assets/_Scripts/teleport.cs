﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class teleport : MonoBehaviour
{
    [Header("Movement/Speed")]
    public float speed;
    private float defSpeed;
    public float speedModifier;
    bool moveToTeleport;
    bool special; //if hit special teleporter

    [Header("Sound")]
    public AudioSource teleportSound;
    public AudioSource flyingSound;

    [Header("Player/GameObject Locations")]
    public Transform player;
    public Transform gun;
    public GameObject spawnPos; //Where player will spawn
    public Vector3 cubePos; //Where cube will spawn
    public Quaternion cubeRot; //Where cube will spawn
    public Vector3 cubeScale; //Where cube will spawn
    public Vector3 gunPos; //Where Pistol will spawn
    public Quaternion gunRot; //Where Pistol will spawn
    public Vector3 offset;
    public int current; //Current teleporter

    // public Teleport_Manager manager;

    [Header("Physics Systems")]
    public Rigidbody hotBod;
    Collider cubeCollider;

    [Header("Oculus")]
    public OVRGrabbable grabbable;
    public OVRScreenFade fade;

    [Header("Timers")]
    public float timer;
    public float timeLimit = 2;
    bool scaled; //True if velocity has been scaled
    float SceneTimer;
    public float SceneTimeLimit;


    // Use this for initialization
    void Start()
    {
        defSpeed = speed;
        player = GameObject.Find("LocalAvatarWithGrab").transform;
        gun = GameObject.Find("Functional Pistol").transform;
        hotBod.constraints = RigidbodyConstraints.FreezeAll;
        // current = manager.teleporters.Length + 1;
        cubePos = transform.position;
        cubeRot = transform.rotation;
        cubeCollider = GetComponent<Collider>();
        cubeScale = transform.localScale;
        gunPos = gun.transform.position;
        gunRot = gun.transform.rotation;
        hotBod = GetComponent<Rigidbody>();
        fade = GameObject.Find("CenterEyeAnchor").GetComponent<OVRScreenFade>();
        // grabbable = GetComponent<OVRGrabbable>();
    }

    // Update is called once per frame
    void Update()
    {

        if (grabbable.isGrabbed)
        {
            if (flyingSound.isPlaying)
                flyingSound.Stop();

            transform.localScale = cubeScale;
            hotBod.constraints = RigidbodyConstraints.None;
            //manager.TurnOn(current);
            timer = 0.0f;
            //Show all teleporters in world.
            //manager.TurnOn(9);
        }
        //else if not grabbed
        else
        {
            if (transform.position != cubePos) //If Cube isn't in original position, move it while keeping teleporters on
            {
                transform.localScale += new Vector3(0.001f, 0.001f, 0.001f);
                if (!flyingSound.isPlaying)
                    flyingSound.Play();
                //Maybe change color too
                timer += Time.deltaTime;

                if (!moveToTeleport)
                {
                    Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                    //if (hotBod.velocity.sqrMagnitude < 0.5 && hotBod.velocity.sqrMagnitude > 800)
                    //{
                    //    hotBod.AddRelativeForce(Vector3.up * 800, ForceMode.Force);
                    //}
                    hotBod.AddForce(new Vector3(input.x, input.y, 0) * 4);
                }
                //if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick)) timer = timeLimit+1;

                if (timer >= timeLimit)
                {
                    transform.localScale = Vector3.MoveTowards(transform.localScale, cubeScale, 0.01f);
                    hotBod.velocity = Vector3.zero;
                    transform.position = Vector3.MoveTowards(transform.position, cubePos, speed); //Move to original position
                    speed += speedModifier;
                    //transform.rotation = cubeRot;
                }
            }
            else
            {
                if (flyingSound.isPlaying)
                    flyingSound.Stop();
                transform.localScale = cubeScale;
                hotBod.constraints = RigidbodyConstraints.FreezePosition;
                timer = 0;
                speed = defSpeed;
                // manager.TurnOff(9);
            }//Turn off teleporters
        }

        if (moveToTeleport)
        { //Teleport player slowly
            if (player.transform.position != spawnPos.transform.position)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, spawnPos.transform.position, 0.15f);
                //Play sound
            }
            if (!gun.GetComponent<OVRGrabbable>().isGrabbed) //Teleport Gun
            {
                if (gun.position != gunPos)
                {
                    gun.position = gunPos;
                    gun.rotation = gunRot;                                //Play sound
                }
            }
            if (transform.position != cubePos)
            {
                transform.position = Vector3.MoveTowards(transform.position, cubePos, 0.13f);
                transform.rotation = cubeRot;                                //Play sound
            }

            if (player.transform.position == spawnPos.transform.position)
                moveToTeleport = false;
        }

        if (special) //If menu teleporter is activated
        {
            SceneTimer += Time.deltaTime;
            if (SceneTimer >= SceneTimeLimit)
            {
                Debug.Log("sce");
                SceneManager.LoadScene(1);
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {

        if (moveToTeleport)
            Physics.IgnoreCollision(other, cubeCollider);
        else if (other.gameObject.CompareTag("teleporter") || other.gameObject.CompareTag("teleporter_special"))
        {
            spawnPos = other.gameObject.transform.parent.Find("spawnPos").gameObject; //Replace current spawnPos with touched one.
            gunPos = other.gameObject.transform.parent.Find("GunPos").gameObject.transform.position; //Replace current spawnPos with touched one.
            cubePos = other.gameObject.transform.parent.Find("CubePos").gameObject.transform.position; //Replace current spawnPos with touched one.;

            teleportSound.Play();

            //Play teleport fx
            moveToTeleport = true;
            hotBod.constraints = RigidbodyConstraints.FreezeAll;
            gun.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            gun.gameObject.GetComponent<gun>().originalPos = gunPos;
            gun.gameObject.GetComponent<gun>().originalRot = gunRot;

            if (other.gameObject.CompareTag("teleporter_special"))
            {
                special = true;
                fade.FadeOut();
            }
            //other.gameObject.transform.parent.gameObject.SetActive(false); //Set teleporter = false
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("laser"))
        {
            hotBod.velocity = collision.gameObject.GetComponent<Rigidbody>().velocity;
        }
    }

}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lazur : MonoBehaviour
{

    private Rigidbody hotBod;
    public float speed;
    float timer;
    public float timeLimit;
    public GameObject player;
    public GameObject gun;
    public gun gunn;
    // Use this for initialization
    void Start()
    {
        gun = GameObject.FindGameObjectWithTag("gun");
        gunn = gun.GetComponent<gun>();
        hotBod = GetComponent<Rigidbody>();
        hotBod.AddForce(transform.up * speed, ForceMode.Impulse);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (hotBod.velocity.sqrMagnitude < speed*speed)
        {
            hotBod.AddRelativeForce(Vector3.up * speed, ForceMode.Force);
        }

        timer += Time.deltaTime;
        if (timer >= timeLimit) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("gun"))
            Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
        //if contacted with enemies, spawn explosion fx and destroy projectile
        else if (collision.gameObject.CompareTag("pixel"))
        {
            gunn.score += 10;
            collision.gameObject.GetComponent<pixel>().health--;
            float chance = Random.Range(0, 1);
            //if (chance >= .75f) //Ammo
            //  gunn.reserve += 15;
           // Destroy(gameObject);

        }
        //else if (collision.gameObject.CompareTag("pickUpHealth"))
        //{
        //    player.GetComponent<Player_Health>().health += 5;
        //    Destroy(collision.gameObject);
        //}
        //else if (collision.gameObject.CompareTag("pickUpAmmo"))
        //{
        //    player.GetComponent<gun>().reserve += 5;
        //    if (player.GetComponent<gun>().reserve > 90)
        //    {
        //        player.GetComponent<gun>().reserve = 90;
        //    }
        //}
    }

    private void OnTriggerEnter(Collider other)
    {

    }
}

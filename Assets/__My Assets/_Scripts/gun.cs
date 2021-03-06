﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum FIRE_MODE { PISTOL, BURST, AUTO };

public class gun : MonoBehaviour
{
    [Header("Player/GameObject Locations")]
    public GameObject player;
    public Transform spawnPos;
    public Vector3 originalPos; //Original position of gun
    public Quaternion originalRot; //Original rotation of gun
    public Transform PistolOffset;

    [Header("Physics Systems")]
    public Rigidbody hotBod;
    private ParticleSystem shootSpark;

    [Header("Oculus")]
    public OVRGrabbable grabbable;
    public OVRGrabber LeftGrabber; //Left Controller
    public OVRGrabber RightGrabber; //Right Controller
    public OVRScreenFade fade;
    bool isVibrating;
    bool TriggerInput;
    bool ReloadInput;
    bool fireModeInput;


    [Header("Sound")]
    public AudioSource laserSound;
    public AudioClip[] laserAudioList;
    public AudioSource uiSound;
    public AudioClip[] uiAudioList;

    [Header("Timers")]
    public float timeLimit; //Time before respawn
    float timer; //Respawn timer
    float ReloadTimer; //Reload timer
    public float ReloadTimeLimit; //Time before reload completion
    float EmptyTimer;
    public float EmptyTimeLimit;//Time till ammo recharge
    public bool leftHandedMode;
    bool isQuitExecuting;
    float QuitTimer;
    public float QuitTimeLimit;
    public Text Quit;

    [Header("UI")]
    public Text AmmoText;
    public Text LeftIndicator;
    public Text ModeIndicator; //Indicates fire mode
    public GameObject ReloadIndicator; //Says to press x/b
    public int score;
    public Text scoreText;

    [Header("Weapon")]
    public GameObject lazur;
    public FIRE_MODE firemode;
    public float burstFireRate;
    public float autoFireRate;
    public bool firing; //Is the gun firing right now?
    bool isReloading;
    public int ammo;
    public int reserve; //Reload into ammo when ammo = 0;   
    int FullAmmo;
    //  int FullReserve;

    // Use this for initialization
    void Start()
    {
        fade = GameObject.Find("CenterEyeAnchor").GetComponent<OVRScreenFade>();
        player = GameObject.Find("LocalAvatarWithGrab");
        PistolOffset = GameObject.Find("PistolOffset").transform;
        LeftGrabber = GameObject.Find("AvatarGrabberLeft").GetComponent<OVRGrabber>();
        RightGrabber = GameObject.Find("AvatarGrabberRight").GetComponent<OVRGrabber>();

        score = 0;

        FullAmmo = ammo;
        //FullReserve = reserve;
        firemode = FIRE_MODE.PISTOL;
        ModeIndicator.text = "Pistol";
        UpdateAmmo();
        //hotBod = GetComponent<Rigidbody>();
        shootSpark = GetComponent<ParticleSystem>();
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "" + "Score:" + score.ToString("000000");
        UpdateAmmo(); //Update Ammo count and UI display

        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick)) //Toggle Left Handed Mode
        {
            LeftHandedModeToggle();
        }

        if (OVRInput.Get(OVRInput.Button.Start))// && SceneManager.GetActiveScene().buildIndex != 0 && SceneManager.GetActiveScene().buildIndex != 0) //Quit to main menu
        {
            QuitTimer += Time.deltaTime;
            Quit.gameObject.transform.parent.gameObject.SetActive(true);
            Quit.text = "Quitting to Main Menu\n---\n" + (QuitTimeLimit - QuitTimer).ToString("F4");
            if (QuitTimer >= QuitTimeLimit)
            {
                StartCoroutine(FadeToQuit("Title"));
            }
        }
        else
        {
            QuitTimer = 0;
            Quit.gameObject.transform.parent.gameObject.SetActive(false);
        }

        //If we grab 
        if (grabbable.isGrabbed)
        {
            //StartCoroutine(Vibration(1, 0.2f, 0.1f));
            hotBod.constraints = RigidbodyConstraints.None;
            timer = 0.0f;

            //Check which hand is grabbing
            if (grabbable.grabbedBy == LeftGrabber)
            {
                switch (firemode)
                {
                    case (FIRE_MODE.PISTOL):
                        TriggerInput = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger); //Left hand
                        break;
                    case (FIRE_MODE.AUTO):
                        TriggerInput = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger); //Left hand
                        break;
                    case (FIRE_MODE.BURST):
                        TriggerInput = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger); //Left hand
                        break;
                }
                ReloadInput = OVRInput.Get(OVRInput.Button.Three); //Left hand
                fireModeInput = OVRInput.GetDown(OVRInput.Button.Four);
            }
            else if (grabbable.grabbedBy == RightGrabber)
            {
                switch (firemode)
                {
                    case (FIRE_MODE.PISTOL):
                        TriggerInput = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger); //Right hand
                        break;
                    case (FIRE_MODE.AUTO):
                        TriggerInput = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger); //Right hand
                        break;
                    case (FIRE_MODE.BURST):
                        TriggerInput = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger); //Right hand
                        break;
                }
                ReloadInput = OVRInput.Get(OVRInput.Button.One); //Right hand
                fireModeInput = OVRInput.GetDown(OVRInput.Button.Two);
            }
            //if we press trigger
            if (TriggerInput)
            {
                if (ammo >= 1)
                    Fire();
                else if (!uiSound.isPlaying)
                {
                    uiSound.PlayOneShot(uiAudioList[0]);
                }
            }
            if (fireModeInput)
            {//Change Fireing mode
                ChangeFireMode();
                Debug.Log("Firemodechange");
            }
        }
        else
        {
            //If not grabbing
            timer += Time.deltaTime;
            //Respawn Gun
            if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
                timer = timeLimit += 1;

            //Move gun back to center
            if (timer >= timeLimit)
            {
                RespawnGun();
            }


        }
    }

    void Fire()
    {
        switch (firemode)
        {
            case (FIRE_MODE.PISTOL):
                firing = true;
                GameObject instance = Instantiate(lazur, spawnPos.position, spawnPos.rotation);
                instance.GetComponent<lazur>().gun = gameObject;
                //Play particle effect
                shootSpark.Play();
                //Play lazr sound
                //if (laserSound.isPlaying) sound.Stop();
                laserSound.clip = laserAudioList[Random.Range(0, 8)];
                laserSound.Play();
                StartCoroutine(Vibration(1, 30, 0.1f));
                ammo--;
                firing = false;
                break;
            case (FIRE_MODE.AUTO):
                if (!firing)
                    StartCoroutine(BurstFire(autoFireRate));
                break;
            case (FIRE_MODE.BURST):
                if (!firing)
                    StartCoroutine(BurstFire(burstFireRate));
                break;
        }

    }

    void ChangeFireMode()
    {
        if (firemode == FIRE_MODE.PISTOL)
        {
            firemode = FIRE_MODE.BURST;
            ModeIndicator.text = "BURST";
            ModeIndicator.color = Color.yellow;
            //if (!uiSound.isPlaying)
            //{
            uiSound.PlayOneShot(uiAudioList[2]);
            //}

        }
        else if (firemode == FIRE_MODE.BURST)
        {
            firemode = FIRE_MODE.AUTO;
            ModeIndicator.text = "AUTO";
            ModeIndicator.color = new Color(244 / 255f, 119 / 255f, 17 / 255f);
            //if (!sound.isPlaying)
            // {
            uiSound.PlayOneShot(uiAudioList[3]);
            // }
        }

        else if (firemode == FIRE_MODE.AUTO)
        {
            firemode = FIRE_MODE.PISTOL;
            ModeIndicator.text = "PISTOL";
            ModeIndicator.color = new Color(.29f, 2.24f, 2.26f);
            // if (!sound.isPlaying)
            // {
            uiSound.PlayOneShot(uiAudioList[1]);
            // }
        }
    }

    void UpdateAmmo()
    {
        if (ammo == 0 && reserve == 0)
        {
            EmptyTimer += Time.deltaTime;
            if (EmptyTimer >= EmptyTimeLimit)
                StartCoroutine(FadeToQuit("Game Over"));
        }

        //Check state
        if (grabbable.isGrabbed)
        {
            //Reloading
            // if (ReloadInput && ammo <= 0 && reserve > 0)
            if (ReloadInput && ammo < FullAmmo && reserve > 0 && !firing)
            { //Can reload
                isReloading = true;

                ReloadTimer += Time.deltaTime;
                StartCoroutine(Vibration(0.2f, 0.5f, 0.1f));
                AmmoText.color = Color.yellow;
                AmmoText.text = "---\n" + (ReloadTimeLimit - ReloadTimer).ToString("F2");
                if (ReloadTimer >= ReloadTimeLimit)
                {
                    //if (reserve >= 30) { ammo = 30; reserve -= 30; }
                    // else { ammo = reserve; reserve = 0; }
                    if (reserve >= FullAmmo - ammo)
                    {
                        reserve -= FullAmmo - ammo;
                        ammo += FullAmmo - ammo;
                    }
                    else
                    {
                        ammo += reserve;
                        reserve = 0;
                    }
                    ReloadTimer = 0.0f;
                    StartCoroutine(Vibration(0.2f, 0.7f, 0.1f));
                }
            }
            else //Not Reloading
            {
                isReloading = false;
                ReloadTimer = 0.0f;
                CheckAmmoColor(1);
            }
        }
        else if (transform.position != originalPos) //Preping to respawn
            CheckAmmoColor(2);
        else //In respawn
            CheckAmmoColor(3);

    }

    void RespawnGun() //Reset gun position to teleported position
    {
        //timer = 0.0f;
        transform.position = Vector3.MoveTowards(transform.position, originalPos, 0.35f);

        if (transform.position == originalPos)
        {
            timer = 0.0f;
            transform.rotation = originalRot;
            hotBod.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    IEnumerator BurstFire(float fireRate)
    {
        float bulletDelay = 60 / fireRate; 
        firing = true;
        for (int i = 0; i < 3; i++)
        {
            if (ammo > 0)
            {
                GameObject instance = Instantiate(lazur, spawnPos.position, spawnPos.rotation);
                instance.GetComponent<lazur>().gun = gameObject;
                //Play particle effect
                shootSpark.Play();
                //Play lazr sound
                laserSound.clip = laserAudioList[Random.Range(0, 8)];
                laserSound.Play();
                StartCoroutine(Vibration(1, 30, 0.1f));
                ammo--;
            }
            yield return new WaitForSeconds(bulletDelay); // wait till the next round
        }
        firing = false;
    }

    void CheckAmmoColor(int flag)
    { //Flag - 1 = Held, 2 = Floating, 3 = Reset to original position
        //Check ammo
        if (ammo > 0)
        {
            ReloadIndicator.SetActive(false);
            if (ammo == FullAmmo) //Full 
                AmmoText.color = Color.green;
            else if (ammo > 6) //Normal
                AmmoText.color = new Color(.29f, 2.24f, 2.26f); //Standary Cyan Blue
            else //Warning
                AmmoText.color = Color.yellow;
            if (flag == 1)
                AmmoText.text = ammo + "\n" + reserve;
            else if (flag == 2)
                AmmoText.text = "--" + "\n" + (timeLimit - timer).ToString("F2"); //Display countdown
            else if (flag == 3)
                AmmoText.text = "--\n";
        }

        else if (reserve > 0) //Reloadable
        {
            ReloadIndicator.SetActive(true);
            AmmoText.color = new Color(244 / 255f, 119 / 255f, 17 / 255f); //Orange
            if (flag == 1)
            {
                AmmoText.text = "Rel\n" + reserve;
                if (!isReloading) StartCoroutine(Vibration(0.2f, 0.1f, 0.1f));
            }
            else if (flag == 2)
                AmmoText.text = "--" + "\n" + (timeLimit - timer).ToString("F2"); //Display countdown
            else if (flag == 3)
                AmmoText.text = "--\n";
        }

        else //Empty
        {
            ReloadIndicator.SetActive(false);
            AmmoText.color = Color.red;
            if (flag == 1)
            {
                AmmoText.text = "END\n" + (EmptyTimeLimit - EmptyTimer).ToString("F0");
                StartCoroutine(Vibration(0.2f, 0.15f, 0.1f));
            }
            else if (flag == 2)
                AmmoText.text = "--" + "\n" + (timeLimit - timer).ToString("F2"); //Display countdown
            else if (flag == 3)
                AmmoText.text = "--\n";

        }
    }

    public void LeftHandedModeToggle()
    {
        leftHandedMode = !leftHandedMode;
        if (leftHandedMode)
        { //Left hand
            LeftIndicator.text = "L";
            PistolOffset.position = new Vector3(0, 0.02f, -0.11f);

        }
        else if (!leftHandedMode)
        { //Right hand
            PistolOffset.position = new Vector3(0.05f, 0.02f, -0.11f);
            LeftIndicator.text = "R";
        }
    }

    IEnumerator FadeToQuit(string name)
    {
        if (isQuitExecuting)
            yield break;

        isQuitExecuting = true;

        fade.FadeOut();
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(name);
    }

    IEnumerator Vibration(float frequency, float amplitude, float timeLimit)
    {

        if (isVibrating)
            yield break;

        isVibrating = true;

        if (grabbable.grabbedBy == LeftGrabber)
        {
            OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.LTouch);
        }
        else if (grabbable.grabbedBy == RightGrabber)
        {
            OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
        }

        yield return new WaitForSeconds(timeLimit); //Set vibration on until timer runs out

        if (grabbable.grabbedBy == LeftGrabber)
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        }
        else if (grabbable.grabbedBy == RightGrabber)
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
        isVibrating = false;

    }

}

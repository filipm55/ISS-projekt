﻿using UnityEngine;


public class GunfireController : MonoBehaviour
{
    // --- Audio ---
    public AudioClip GunShotClip;
    public AudioClip ReloadClip;
    public AudioSource source;
    public AudioSource reloadSource;
    public Vector2 audioPitch = new Vector2(.9f, 1.1f);

    // --- Muzzle ---
    public GameObject muzzlePrefab;
    public GameObject muzzlePosition;

    // --- Config ---
    public float shotDelay = .5f;
    public RaycastHit hit;

    // --- Projectile ---
    [Tooltip("The projectile gameobject to instantiate each time the weapon is fired.")]
    //public GameObject projectilePrefab;
    public HomingScript missile;
    [SerializeField]
    private Transform target;
    [Tooltip("Sometimes a mesh will want to be disabled on fire. For example: when a rocket is fired, we instantiate a new rocket, and disable" +
        " the visible rocket attached to the rocket launcher")]
    public GameObject projectileToDisableOnFire;

    // --- Timing ---
    [SerializeField] private float timeLastFired;


    //scope
    public GameObject scopeOverlay;
    private bool isScoped = false;
    public Camera mainCamera;
    public float scopedFOV = 10f;
    private float normalFOV;


    private void Start()
    {
        if(source != null) source.clip = GunShotClip;
        timeLastFired = 0;
    }

    private void Update()
    {

        if (Input.GetButtonDown("Fire2"))
        {
            isScoped = !isScoped;
            scopeOverlay.SetActive(isScoped);

            if (isScoped)
            {
                normalFOV = mainCamera.fieldOfView;
                mainCamera.fieldOfView = scopedFOV;

            }
            else
            {
                mainCamera.fieldOfView = normalFOV;
            }
        }

        if (Input.GetKeyDown(KeyCode.L))    // for standalone
        {
            Debug.Log("trace");
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay(mouse);
            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity))
            {
                Debug.Log(hit.collider);
                if (hit.collider.gameObject.CompareTag("enemy"))
                {
                    target = hit.collider.transform;
                }
            }
        }

        // --- Fires the weapon if the delay time period has passed since the last shot ---
        if (Input.GetButtonDown("Fire1") && ((timeLastFired + shotDelay) <= Time.time) && isScoped)
        {
            mainCamera.fieldOfView = normalFOV;
            isScoped = false;
            scopeOverlay.SetActive(false);
            FireWeapon();
        }

        

    }

    public void FireWeapon()
    {
        // --- Keep track of when the weapon is being fired ---
        timeLastFired = Time.time;

        // --- Spawn muzzle flash ---
        var flash = Instantiate(muzzlePrefab, muzzlePosition.transform);

        // --- Shoot Projectile Object ---
        if (missile != null) //if (projectilePrefab != null)
        {
            HomingScript newProjectile = Instantiate(missile, muzzlePosition.transform.position, muzzlePosition.transform.rotation); //Instantiate(projectilePrefab, muzzlePosition.transform.position, muzzlePosition.transform.rotation);
            newProjectile.target = target;
        }

        // --- Disable any gameobjects, if needed ---
        if (projectileToDisableOnFire != null)
        {
            projectileToDisableOnFire.SetActive(false);
            Invoke("ReEnableDisabledProjectile", 3);
        }

        // --- Handle Audio ---
        if (source != null)
        {
            // --- Sometimes the source is not attached to the weapon for easy instantiation on quick firing weapons like machineguns, 
            // so that each shot gets its own audio source, but sometimes it's fine to use just 1 source. We don't want to instantiate 
            // the parent gameobject or the program will get stuck in a loop, so we check to see if the source is a child object ---
            if(source.transform.IsChildOf(transform))
            {
                source.Play();
            }
            else
            {
                // --- Instantiate prefab for audio, delete after a few seconds ---
                AudioSource newAS = Instantiate(source);
                if ((newAS = Instantiate(source)) != null && newAS.outputAudioMixerGroup != null && newAS.outputAudioMixerGroup.audioMixer != null)
                {
                    // --- Change pitch to give variation to repeated shots ---
                    newAS.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", Random.Range(audioPitch.x, audioPitch.y));
                    newAS.pitch = Random.Range(audioPitch.x, audioPitch.y);

                    // --- Play the gunshot sound ---
                    newAS.PlayOneShot(GunShotClip);

                    // --- Remove after a few seconds. Test script only. When using in project I recommend using an object pool ---
                    Destroy(newAS.gameObject, 4);
                }
            }
        }

        // --- Insert custom code here to shoot projectile or hitscan from weapon ---

    }

    private void ReEnableDisabledProjectile()
    {
        reloadSource.Play();
        projectileToDisableOnFire.SetActive(true);
    }
}

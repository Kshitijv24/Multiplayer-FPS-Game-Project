using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float timeBetweenShots;
    public float heatPerShot;
    public bool isAutomatic;
    public GameObject muzzleFlash;
    public int shotDamage;
    public float adsZoom;
    public AudioSource shotSound;
}

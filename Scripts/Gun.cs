using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private float damage = 15f;

    [SerializeField] private AudioClip[] gunSounds = new AudioClip[0];
    [SerializeField] private AudioClip nadeExplosionSound = null;   
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
            if (Input.GetMouseButtonDown(0))
            {
                if (CustomCameraComponents.Raycaster.CastFromCamera(out RaycastHit hit) && hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    //try to kill em and if you do, knock em around a bit.
                    if (damageable.TakeDamage(damage) && hit.collider.TryGetComponent(out Rigidbody rb))
                        rb.AddForceAtPosition(transform.forward * Random.Range(50f, 250f), hit.point, ForceMode.Impulse); 

                }
                if (gunSounds.Length > 0)
                    AudioSource.PlayClipAtPoint(gunSounds[Random.Range(0, gunSounds.Length)], transform.position);
            }
            //nade
            if (Input.GetKeyDown(KeyCode.G))
            {
                var o = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                //nade color
                o.GetComponent<Renderer>().material.color = Color.green;

                //nade size
                o.transform.localScale *= .3f;

                //Put it in hand
                o.transform.position = transform.position;

                //Give it physics
                var rb = o.AddComponent<Rigidbody>();
                rb.mass = 5f;

                //Throw it
                rb.AddForce((transform.forward + (transform.up * .45f)) * 50f, ForceMode.Impulse);

                //blow it up (later)
                StartCoroutine(DetonateGrenade(o));
            }
        }

    IEnumerator DetonateGrenade(GameObject nade) 
    {
        yield return new WaitForSeconds(1.2f);

        float nadeDist = 10f;
        float maxDamage = 50f;

        if (nadeExplosionSound != null)
            AudioSource.PlayClipAtPoint(nadeExplosionSound, nade.transform.position);

        var cols = Physics.OverlapSphere(nade.transform.position, nadeDist);

        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].TryGetComponent(out IDamageable damageable))
            {
                Vector3 dir = cols[i].transform.position - nade.transform.position;
                float dist = dir.magnitude;

                //if dies
                if (damageable.Dead() || damageable.TakeDamage(maxDamage - (maxDamage * (dist / maxDamage)))) //damage reduction for distance.
                {
                    if (cols[i].TryGetComponent(out Rigidbody rb))
                        rb.AddForce(dir * (100f - (100f * (dist / 100f))), ForceMode.Impulse);
                }
            }
        }

        Destroy(nade);
    }
}

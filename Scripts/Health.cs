using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    #region More for debug/visualization
    [SerializeField] private HealthBar healthBar = null;
    #endregion

    public bool Dead ()=> currentHealth <= 0f;

    #region Important fields for video
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Regeneration")]
    [SerializeField] private float timeBetweenDamageAndRegen = .5f; //the amount of time after taking damage before beginning to regen
    private float startRegenTime = 0.0f;

    private bool RegenCanStart => Time.time > startRegenTime;


    [SerializeField] private float regenRate = 10.0f; //Amount regenerated/second

    private bool needsRegen = false;
    #endregion

    #region Taking damage
    /// <summary>
    /// Use this to affect the entity's health.
    /// </summary>
    /// <param name="damageInstance">Use this for sending in damage.</param>
    /// <returns>TRUE: the entity DIED from this damage instance. (Could return false but already be dead)</returns>
    public bool TakeDamage(float damageInstance) 
    {
        if (currentHealth <= 0f)
            return false; //already dead

        currentHealth -= damageInstance;

        OnTakeDamage();

        //dead
        if (currentHealth <= 0f)
        {
            Die();
            return true;
        }

        //alive
        return false;
    }

    private void OnTakeDamage() 
    {
        needsRegen = true;

        startRegenTime = Time.time + timeBetweenDamageAndRegen;

        OnChangeHealth();
    }

    private void Die() 
    {
        needsRegen = false; //probably when you most need regen but, whatever. 

        GetComponent<Renderer>().material.color = Color.black;

        var rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 25f;
    }
    #endregion


    private void OnChangeHealth() 
    {
        if (healthBar)
            healthBar.UpdateFill(currentHealth / maxHealth);
    }

    #region Regeneration
    private void RegenHealth() 
    {
        currentHealth += regenRate * Time.deltaTime;

        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;

            needsRegen = false;
        }

        OnChangeHealth();
    }
    #endregion


    private void Start()
    {
        OnChangeHealth(); //set the bar to full
    }

    // Update is called once per frame
    void Update()
    {
        if (needsRegen && RegenCanStart)
            RegenHealth();
    }
}

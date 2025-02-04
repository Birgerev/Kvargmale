﻿using System;
using System.Collections;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class LivingEntity : Entity
{
    //Entity Properties
    public static Color damageColor = new Color(1, 0.5f, 0.5f, 1);

    public Nameplate nameplate;

    //Entity Data Tags
    [EntitySaveField(false)] [SyncVar] public float health;
    [EntitySaveField(false)] [SyncVar] public string displayName;
    
    //TODO even needs to be sync var?
    [SyncVar] public bool isOnClimbable;

    private EntityController controller;

    public virtual float maxHealth { get; } = 20;

    public override void Start()
    {
        base.Start();
        
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    [Server]
    public override void Initialize()
    {
        health = maxHealth;
        controller = GetController();

        base.Initialize();
    }

    [Server]
    public override void Tick()
    {
        base.Tick();
        
        //Controller tick
        if (controller != null)
            controller.Tick();

        UpdateAnimatorValues();
        AmbientSoundCheck();
        
        //Climbable check
        Block block = Location.GetBlock();
        isOnClimbable = block != null && block.IsClimbable;
    }

    [Server]
    public override void Teleport(Location loc)
    {
        FallDamage fallDamage = GetComponent<FallDamage>();
        if(fallDamage) fallDamage.lastGroundedHeight = float.MinValue;
        
        base.Teleport(loc);
    }

    [Client]
    public override void ClientUpdate()
    {
        base.ClientUpdate();

        UpdateNameplate();
    }

    [Server]
    public void AmbientSoundCheck()
    {
        int checkDuration = 4;
        float timeOffset =
            (float) new Random(uuid.GetHashCode()).NextDouble() * checkDuration; //Uses a static seed (id)

        if ((Time.time + timeOffset) % checkDuration - Time.deltaTime <= 0)
            if (new Random(Time.time.GetHashCode() + uuid.GetHashCode()).NextDouble() < 0.5f)
                AmbientSound();
    }

    [Server]
    public virtual void UpdateAnimatorValues()
    {
        Animator anim = GetComponent<Animator>();

        if (anim == null)
            return;

        anim.SetFloat("velocity-x", Mathf.Abs(GetVelocity().x));
    }

    [Client]
    public virtual void UpdateNameplate()
    {
        if (nameplate == null)
        {
            Debug.LogWarning("Nameplate is missing for entity type: " + name);
            return;
        }

        nameplate.text = displayName;
    }

    [Server]
    public virtual EntityController GetController()
    {
        return new EntityController(this);
    }

    
    [Server]
    public override void Hit(float damage, Entity source)
    {
        base.Hit(damage, source);
        
        Knockback(transform.position - source.transform.position);
    }
    
    [Server]
    public override void Damage(float damage)
    {
        HurtSound();
        DamageSound();

        health -= damage;

        if (health <= 0)
            Die();

        RedDamageEffect();
    }

    [Server]
    public override void Die()
    {
        if (dead)
            return;

        DeathSound();
        GetComponent<EntityParticleEffects>()?.RPC_DeathSmokeEffect();

        base.Die();
    }

    [Server]
    public void DamageSound()
    {
        Sound.Play(Location, "entity/damage", SoundType.Entities, 0.5f, 1.5f);
    }

    [Server]
    public virtual void HurtSound()
    {
        string soundName = "entity/" + GetType() + "/hurt";

        if (Sound.Exists(soundName))
            Sound.Play(Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }

    [Server]
    public virtual void DeathSound()
    {
        string soundName = "entity/" + GetType() + "/death";

        if (Sound.Exists(soundName))
            Sound.Play(Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }

    [Server]
    public virtual void AmbientSound()
    {
        string soundName = "entity/" + GetType() + "/idle";

        if (Sound.Exists(soundName))
            Sound.Play(Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }

    public virtual void Knockback(Vector2 direction)
    {
        direction.Normalize();

        GetComponent<Rigidbody2D>().velocity += new Vector2(direction.x * 5f, 5f);
    }

    [ClientRpc]
    public void RedDamageEffect()
    {
        StartCoroutine(TurnRedByDamage());
    }

    private IEnumerator TurnRedByDamage()
    {
        GetRenderer().color = damageColor;
        yield return new WaitForSeconds(0.2f);
        GetRenderer().color = Color.white;
    }
}

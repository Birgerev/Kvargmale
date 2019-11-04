﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : Entity
{
    //Entity Properties
    public static Color damageColor = new Color(1, 0.5f, 0.5f, 1);
    public virtual float maxHealth { get; } = 20;
    
    [Header("Movement Properties")]
    private float acceleration = 4f;
    private float walkSpeed = 4.3f;
    private float sprintSpeed = 5.6f;
    private float sneakSpeed = 1.3f;
    private float jumpVelocity = 6f;
    private float swimVelocity = 6f;
    private float groundFriction = 0.92f;
    private float airDrag = 0.92f;
    private float liquidDrag = 0.5f;

    //Entity Data Tags
    [EntityDataTag(false)]
    public float health;
    

    //Entity State
    private float last_jump_time;
    private float highestYlevelsinceground;
    public EntityController controller;

    public override void Start()
    {
        health = maxHealth;

        base.Start();

        controller = GetController();

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public virtual void FixedUpdate()
    {
        ProcessMovement();
        fallDamageCheck();
    }

    public override void Update()
    {
        base.Update();

        controller.Tick();
        CalculateFlip();
        UpdateAnimatorValues();
    }

    public virtual void UpdateAnimatorValues()
    {
        Animator anim = GetComponent<Animator>();

        if (anim == null)
            return;

        if(anim.isInitialized)
            anim.SetFloat("velocity-x", getVelocity().x);
    }

    public virtual void ProcessMovement()
    {
        ApplyFriction();
    }

    public virtual void ApplyFriction()
    {
        if (isInLiquid)
            setVelocity(getVelocity() * liquidDrag);
        if (!isInLiquid && !isOnGround)
            setVelocity(new Vector3(getVelocity().x * airDrag, getVelocity().y));
        if (!isInLiquid && isOnGround)
            setVelocity(getVelocity() * groundFriction);
    }
    
    public virtual void Walk(int direction)
    {
        if (getVelocity().x < walkSpeed && getVelocity().x > -walkSpeed)
        {
            float targetXVelocity = 0;

            if (direction == -1)
                targetXVelocity -= walkSpeed;
            else if (direction == 1)
                targetXVelocity += walkSpeed;
            else targetXVelocity = 0;
            
            GetComponent<Rigidbody2D>().velocity += new Vector2(targetXVelocity * (acceleration * Time.deltaTime), 0);
        }
    }

    public virtual EntityController GetController()
    {
        return new EntityController(this);
    }

    public virtual void CalculateFlip()
    {
        if (getVelocity().x != 0)
        {
            flipRenderX = (getVelocity().x < 0);
        }
    }

    public virtual void Jump()
    {
        if (isOnGround)
        {
            if (Time.time - last_jump_time < 0.7f)
                return;

            setVelocity(getVelocity() + new Vector2(0, jumpVelocity));
            last_jump_time = Time.time;
        }
        if (isInLiquid && getVelocity().y < swimVelocity)
        {
            setVelocity(getVelocity() + new Vector2(0, swimVelocity*0.1f));
        }
    }


    private void fallDamageCheck()
    {
        if (isOnGround && !isInLiquid)
        {
            float damage = (highestYlevelsinceground - transform.position.y) - 3;
            if (damage >= 1)
                TakeFallDamage(damage);
        }

        if (isOnGround || isInLiquid)
            highestYlevelsinceground = 0;
        else if (transform.position.y > highestYlevelsinceground)
            highestYlevelsinceground = transform.position.y;
    }

    public override void Hit(float damage)
    {
        base.Hit(damage);

        Knockback(transform.position - Player.localInstance.transform.position);
    }

    public virtual void TakeFallDamage(float damage)
    {
        Damage(damage);
    }

    public override void Damage(float damage)
    {
        health -= damage;

        if (health <= 0)
            Die();

        StartCoroutine(TurnRedByDamage());
    }

    public override void Die()
    {
        Particle.Spawn_SmallSmoke(transform.position, Color.white);

        base.Die();
    }

    public virtual void Knockback(Vector2 direction)
    {
        direction.Normalize();

        GetComponent<Rigidbody2D>().velocity += new Vector2(direction.x*3f, 4f);
    }

    IEnumerator TurnRedByDamage()
    {
        Color baseColor = getRenderer().color;

        getRenderer().color = damageColor;
        yield return new WaitForSeconds(0.15f);
        getRenderer().color = baseColor;
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : Entity
{
    public static Color damageColor = new Color(1, 0.5f, 0.5f, 1);

    public virtual float health { get; set; }
    public virtual float maxHealth { get; } = 20;
    
    [Header("Movement Properties")]
    private float walkSpeed = 4.3f;
    private float sprintSpeed = 5.6f;
    private float sneakSpeed = 1.3f;
    private float jumpVelocity = 7f;
    private float swimVelocity = 6f;
    private float groundFriction = 0.7f;
    private float airDrag = 0.98f;
    private float liquidDrag = 0.5f;

    private float last_jump_time;

    private float highestYlevelsinceground;

    public override void Start()
    {
        base.Start();

        health = maxHealth;

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

        checkSuffocation();
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
            
            setVelocity(new Vector2(targetXVelocity, getVelocity().y));
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

    private void checkSuffocation()
    {
        if (Time.frameCount % (int)(0.75f / Time.deltaTime) == 1)
        {
            if (Chunk.getBlock(Vector2Int.FloorToInt(((Vector2)transform.position))))
            {
                if(Chunk.getBlock(Vector2Int.FloorToInt(((Vector2)transform.position))).playerCollide)
                    TakeSuffocationDamage(1);
            }
        }
    }

    public virtual void TakeFallDamage(float damage)
    {
        Damage(damage);
    }

    public virtual void TakeSuffocationDamage(float damage)
    {
        Damage(damage);
    }

    public virtual void Damage(float damage)
    {
        health -= damage;

        if (health <= 0)
            Die();

        StartCoroutine(TurnRedByDamage());
    }

    public override SpriteRenderer getRenderer()
    {
        return transform.Find("_renderer").GetComponent<SpriteRenderer>();
    }

    IEnumerator TurnRedByDamage()
    {
        Color baseColor = getRenderer().color;

        getRenderer().color = damageColor;
        yield return new WaitForSeconds(0.15f);
        getRenderer().color = baseColor;
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}

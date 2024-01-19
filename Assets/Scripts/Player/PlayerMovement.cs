using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private const float SneakSpeed = 1.3f;
    private const float SprintSpeed = 5.6f;
    
    [SyncVar] public bool sprinting;
    [SyncVar] public bool sneaking;
    
    private float _lastDoubleTapSprintTap;
    private bool _ladderSneakingLastFrame;
    
    private Player _player;
    private EntityMovement _movement;

    //TODO clean
    private void Update()
    {
        if (!isOwned) return;
        
        PerformInput();
        CrouchOnLadderCheck();
    }

    [Client]
    private void PerformInput()
    {
        if (!PlayerInteraction.CanInteractWithWorld()) return;
        
        //Toggle Debug disabled lighting
        if (Input.GetKeyDown(KeyCode.F4) && Debug.isDebugBuild)
        {
            LightManager.DoLight = !LightManager.DoLight;
            LightManager.UpdateAllLight();
        }

        //Open chat
        if (Input.GetKeyDown(KeyCode.T))
            ChatMenu.instance.open = true;
        
        //Walking
        if (Input.GetKey(KeyCode.A))
            _movement.Walk(-1);
        if (Input.GetKey(KeyCode.D))
            _movement.Walk(1);

        //Jumping
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
            _movement.Jump();

        //Sneaking
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.S))
        {
            sneaking = true;
            _movement.speed = SneakSpeed;
        }

        if (sneaking && (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.S)))
        {
            sneaking = false;
            _movement.speed = _movement.walkSpeed;
        }

        //Stop Sprinting
        if (sprinting &&
            (Mathf.Abs(_player.GetVelocity().x) < 0.1f || 
             sneaking || 
             _player.hunger <= 6 || 
             (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))))
        {
            sprinting = false;
            _movement.speed = _movement.walkSpeed;
        }
        
        //CTRL start sprint
        if (Input.GetKeyDown(KeyCode.LeftControl) && _player.hunger > 6 && !sneaking)
        {
            sprinting = true;
            _movement.speed = SprintSpeed;
        }
        
        //Double press walk start sprint
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - _lastDoubleTapSprintTap < 0.3f &&  !sneaking)
            {
                sprinting = true;
                _movement.speed = SprintSpeed;
            }
            
            _lastDoubleTapSprintTap = Time.time;
        }
    }

    public void CrouchOnLadderCheck()
    {
        bool isLadderSneaking = _player.isOnClimbable && sneaking;
        
        //Started ladder sneaking
        if (isLadderSneaking && !_ladderSneakingLastFrame)
        {
            GetComponent<Rigidbody2D>().gravityScale = 0;
            _ladderSneakingLastFrame = true;
        }
        
        //Stopped ladder sneaking
        if (!isLadderSneaking && _ladderSneakingLastFrame)
        {
            GetComponent<Rigidbody2D>().gravityScale = 1;
            _ladderSneakingLastFrame = false;
        }
    }

    private void Awake()
    {
        _player = GetComponent<Player>();
        _movement = GetComponent<EntityMovement>();
    }
}

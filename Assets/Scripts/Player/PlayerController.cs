using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    private bool _isMoving;
    private Vector2 _playerInput;
    [SerializeField] private Animator _animator;
    [SerializeField] private LayerMask _solidObjectsLayer;
    [SerializeField] private LayerMask _tallGrassLayer;

    public event Action OnEncounter;
    void Start()
    {
        
    }

    public void HandleUpdate()
    {
        Move();
    }

    void Move()
    {
        if (!_isMoving) //check if we are moving, allowing to receive input only if not moving
        {
            //receive player input
            _playerInput.x = Input.GetAxisRaw("Horizontal");
            _playerInput.y = Input.GetAxisRaw("Vertical");

            //avoid walking diagonaly
            if (_playerInput.x != 0)
                _playerInput.y = 0;

            //check if we have any player input
            if (_playerInput != Vector2.zero)
            {
                //apply proper animation based on received input
                _animator.SetFloat("moveX", _playerInput.x);
                _animator.SetFloat("moveY", _playerInput.y);

                //set the position of our movement based on received input
                Vector3 targetPos = transform.position;
                targetPos.x += _playerInput.x;
                targetPos.y += _playerInput.y;

                //check if we can move to our next tile
                if(IsWalkable(targetPos))
                    StartCoroutine(ApplyMovement(targetPos));
            }
        }

        _animator.SetBool("isMoving", _isMoving);
    }

    IEnumerator ApplyMovement(Vector3 targetPos)
    {
        //turn our bool to true to make sure we can't receive further player input
        _isMoving = true;

        //apply movement to the player until its on the next tile
        while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed * Time.deltaTime);
            yield return null;
        }

        //when the position is close enough, make sure to set the player pos to the next tile
        transform.position = targetPos;

        //turn our bool to false so we can receive input from the player
        _isMoving = false;

        CheckBattleEncounter();
    }

    void CheckBattleEncounter()
    {
        //check if the player is currently on a grass tile
        if (Physics2D.OverlapCircle(transform.position, 0.2f, _tallGrassLayer) != null)
        {
            if (Random.Range(1, 101) <= 15)
            {
                _animator.SetBool("isMoving", false);
                OnEncounter?.Invoke();
            }
        }
    }

    bool IsWalkable(Vector3 targetPos)
    {
        //check if the next tile we are going to move is from the SolidObjectsLayer
        if (Physics2D.OverlapCircle(targetPos, 0.2f, _solidObjectsLayer) != null)
            return false;

        return true;
    }
}

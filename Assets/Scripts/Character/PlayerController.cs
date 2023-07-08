using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    private Vector2 _playerInput;
    private int _enCounter;
    private Vector3 _facingDir;
    private Character _character;
    public event Action OnEncounter;
    void Awake()
    {
        _character = GetComponent<Character>();
        if (_character == null)
            Debug.LogError("Character is NULL on " + transform.name);
    }

    public void HandleUpdate()
    {
        Move();
        if (Input.GetKeyDown(KeyCode.X))
        {
            Interact();
        }
    }

    void Move()
    {
        if (!_character.IsMoving) //check if we are moving, allowing to receive input only if not moving
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
                StartCoroutine(_character.ApplyMovement(_playerInput, CheckBattleEncounter));
            }
        }

        _character.HandleUpdate();
    }

    void CheckBattleEncounter()
    {
        //check if the player is currently on a grass tile
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.Instance.TallGrassLayer) != null)
        {
            if (Random.Range(1, 101) <= (10 + (2 * _enCounter)))
            {
                _enCounter = 0;
                _character.Animator.IsMoving = false;
                OnEncounter?.Invoke();
            }
            _enCounter++;
            Math.Clamp(_enCounter, 0, 20);
        }
        else
            _enCounter = 0;
    }

    void Interact()
    {
        _facingDir.Set(_character.Animator.MoveX, _character.Animator.MoveY, transform.position.z);
        Vector3 interactablePos = transform.position + _facingDir;

        Collider2D collider = Physics2D.OverlapCircle(interactablePos, 0.3f, GameLayers.Instance.InteractableLayer);
        if (collider != null)
        {
            _character.Animator.IsMoving = false;
            collider.GetComponent<Interactable>()?.Interact();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState { Idle, Walking, Dialogue }
public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialogue _dialogue;
    [SerializeField] List<Vector2> _movementPattern;
    [SerializeField] float _timeBetweenPatterns;
    NPCState _state;
    float _idleTimer = 0f;
    int _currentPattern = 0;

    Character _character;

    void Awake()
    {
        _character = GetComponent<Character>();
        if (_character == null)
            Debug.LogError("Character is NULL on " + transform.name);

    }
    public void Interact(Transform initiator)
    {
        if (_state == NPCState.Idle)
        {
            _state = NPCState.Dialogue;
            _character.LookTorwards(initiator.position);
            StartCoroutine(DialogueManager.Instance.ShowDialogue(_dialogue, () => 
            {
                _idleTimer = 0f;
                _state = NPCState.Idle; 
            }
            ));
        }
            
    }

    void Update()
    {
        if (GameManager.gameState != GameState.FreeRoam)
            return;

        if (_state == NPCState.Idle)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer > _timeBetweenPatterns)
            {
                _idleTimer = 0f;
                if (_movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }

        _character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        _state = NPCState.Walking;

        Vector3 oldPos = transform.position;
        yield return _character.ApplyMovement(_movementPattern[_currentPattern]);
        if(transform.position != oldPos)
            _currentPattern = (_currentPattern + 1) % _movementPattern.Count;

        _state = NPCState.Idle;
    }
}
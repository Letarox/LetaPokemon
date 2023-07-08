using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoSingleton<DialogueManager>
{
    [SerializeField] GameObject _dialogueBox;
    [SerializeField] TextMeshProUGUI _dialogueText;
    [SerializeField] int _lettersPerSecond;

    int _lineCounter;
    Dialogue _dialogue;
    Action onDialogueFinished;
    bool _isTyping;
    WaitForEndOfFrame _frameDelay = new WaitForEndOfFrame();

    public event Action OnShowDialogue;
    public event Action OnCloseDialogue;

    public bool IsShowingDialogue { get; set; }

    public IEnumerator ShowDialogue(Dialogue dialogue, Action onFinished = null)
    {
        yield return _frameDelay;

        _dialogue = dialogue;
        onFinished = onDialogueFinished;
        if (OnShowDialogue != null)
            OnShowDialogue?.Invoke();
        IsShowingDialogue = true;
        _dialogueBox.SetActive(true);
        StartCoroutine(TypeDialogue(dialogue.Lines[0]));
    }

    IEnumerator TypeDialogue(string line)
    {
        _isTyping = true;
        _dialogueText.text = string.Empty;
        foreach (char letter in line.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(1f / _lettersPerSecond);
        }
        _isTyping = false;
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.X) && !_isTyping)
        {
            _lineCounter++;
            if (_lineCounter < _dialogue.Lines.Count)
            {
                StartCoroutine(TypeDialogue(_dialogue.Lines[_lineCounter]));
            }
            else
            {
                _lineCounter = 0;
                IsShowingDialogue = false;
                _dialogueBox.SetActive(false);
                if (OnCloseDialogue != null)
                    OnCloseDialogue?.Invoke();
                if (onDialogueFinished != null)
                    onDialogueFinished?.Invoke();
            }

        }
    }
}

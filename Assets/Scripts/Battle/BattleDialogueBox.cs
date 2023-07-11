using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _dialogueText;
    [SerializeField] Color _highlightedColor;
    [SerializeField] int _lettersPerSecond;
    [SerializeField] GameObject _actionSelector;
    [SerializeField] GameObject _moveSelector;
    [SerializeField] GameObject _moveDetails;
    [SerializeField] List<TextMeshProUGUI> _actionTextList;
    [SerializeField] List<TextMeshProUGUI> _moveTextList;
    [SerializeField] TextMeshProUGUI _ppText;
    [SerializeField] TextMeshProUGUI _typeText;
    [SerializeField] WaitForSeconds _dialogueDelay = new WaitForSeconds(0.75f);

    public void SetDialogue(string dialogue)
    {
        _dialogueText.text = dialogue;
    }

    public IEnumerator TypeDialogue(string dialogue)
    {
        if (dialogue == null)
            yield break;

        _dialogueText.text = string.Empty;

        StringBuilder stringBuilder = new StringBuilder();

        foreach (char letter in dialogue)
        {
            stringBuilder.Append(letter);
            _dialogueText.text = stringBuilder.ToString();

            yield return new WaitForSeconds(1f / _lettersPerSecond);
        }
        yield return _dialogueDelay;
    }

    public void EnableDialogueText(bool enable)
    {
        _dialogueText.enabled = enable;
    }

    public void EnableActionSelector(bool enable)
    {
        _actionSelector.SetActive(enable);
    }

    public void EnableMoveSelector(bool enable)
    {
        _moveSelector.SetActive(enable);
        _moveDetails.SetActive(enable);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for(int i = 0; i < _actionTextList.Count; i++)
        {
            if (i == selectedAction)
                _actionTextList[i].color = _highlightedColor;
            else
                _actionTextList[i].color = Color.black;
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move, Pokemon enemyPokemon)
    {
        for(int i = 0; i < _moveTextList.Count; i++)
        {
            if (i == selectedMove)
                _moveTextList[i].color = _highlightedColor;
            else
                _moveTextList[i].color = Color.black;
        }

        if (move.PP == 0)
            _ppText.color = Color.red;
        else
            _ppText.color = Color.black;
        _ppText.text = $"PP: { move.PP }/{ move.Base.PP }";
        _typeText.text = move.Base.Type.ToString();

        //check to see if the move is super-effective. If it is, change the color to green. If its not very-effective, change color to Red. Default is black
        float value = TypeChart.GetEffectiveness(move.Base.Type, enemyPokemon.Base.PrimaryType) * TypeChart.GetEffectiveness(move.Base.Type, enemyPokemon.Base.SecondaryType);
        if (value > 1)
        {
            _typeText.color = Color.green;
            _typeText.fontStyle = FontStyles.Normal;
        }            
        else if (value < 1 && value > 0)
        {
            _typeText.color = Color.red;
            _typeText.fontStyle = FontStyles.Normal;
        }            
        else if (value == 0)
        {
            _typeText.color = Color.red;
            _typeText.fontStyle = FontStyles.Strikethrough;
        }
        else
        {
            _typeText.color = Color.black;
            _typeText.fontStyle = FontStyles.Normal;
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for(int i = 0; i < _moveTextList.Count; i++)
        {
            if (i < moves.Count)
                _moveTextList[i].text = moves[i].Base.Name;
            else
                _moveTextList[i].text = "-";
        }
    }
}

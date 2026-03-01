using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/New DialogueData")]
public class DialogueData_SO : ScriptableObject
{
    public List<DialoguePiece> Pieces = new List<DialoguePiece>();
}

[System.Serializable]
public class DialoguePiece
{
    public string CharacterName;
    [TextArea]
    public string CharacterContent;
}

using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class Node : MonoBehaviour, ITypable
{
    [Header("Connections")]
    public List<Node> neighbors = new List<Node>();

    [Header("UI")]
    [SerializeField] private TMP_Text wordDisplay; 
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    private string _currentWord;
    private bool _isTargetable = false;

    // Метод активации узла
    public void SetTargetable(bool status)
    {
        _isTargetable = status;
        
        if (_isTargetable)
        {
            StopAllCoroutines();
            StartCoroutine(RequestWordRoutine());
        }
        else
        {
            ClearNode();
        }
    }

    private IEnumerator RequestWordRoutine()
    {
        // Ждем, пока WordProvider инициализируется (на случай гонки кадров)
        while (WordProvider.Instance == null) yield return null;

        _currentWord = WordProvider.Instance.GetUniqueWord(WordType.Navigation);
        
        // Если пул еще не загружен, ждем один кадр и пробуем снова
        if (string.IsNullOrEmpty(_currentWord))
        {
            yield return null; 
            _currentWord = WordProvider.Instance.GetUniqueWord(WordType.Navigation);
        }

        if (!string.IsNullOrEmpty(_currentWord))
        {
            if (wordDisplay != null)
            {
                wordDisplay.text = _currentWord;
                wordDisplay.color = normalColor;
            }
            TypingManager.Instance.RegisterTypable(this);
        }
    }

    private void ClearNode()
    {
        StopAllCoroutines();
        if (TypingManager.Instance != null)
            TypingManager.Instance.UnregisterTypable(this);
        
        if (WordProvider.Instance != null && !string.IsNullOrEmpty(_currentWord))
        {
            WordProvider.Instance.ReleaseWord(_currentWord);
        }
        
        _currentWord = "";
        if (wordDisplay != null) wordDisplay.text = "";
    }

    public string GetWord() => _currentWord;

    public void OnCharTyped(int index)
    {
        if (string.IsNullOrEmpty(_currentWord) || wordDisplay == null) return;
        string typed = _currentWord.Substring(0, index);
        string remaining = _currentWord.Substring(index);
        wordDisplay.text = $"<color=#{ColorUtility.ToHtmlStringRGB(highlightColor)}>{typed}</color>{remaining}";
    }

    public void OnReset() => wordDisplay.text = _currentWord;

    public void OnComplete()
    {
        // Регистрируем завершенное слово для комбо и WPM
        ProgressionManager.Instance.RegisterCompletedWord(_currentWord);
    
        PlayerController.Instance.MoveToNode(this);
    }

    public Transform GetTransform() => transform;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null) Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }
}
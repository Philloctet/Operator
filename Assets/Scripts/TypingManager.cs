using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

public class TypingManager : MonoBehaviour
{
    public static TypingManager Instance;

    private string _currentBuffer = "";
    private List<ITypable> _activeTypables = new List<ITypable>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }

    // МЕТОДЫ РЕГИСТРАЦИИ (Добавь их сюда)
    public void RegisterTypable(ITypable typable)
    {
        if (!_activeTypables.Contains(typable))
        {
            _activeTypables.Add(typable);
        }
    }

    public void UnregisterTypable(ITypable typable)
    {
        if (_activeTypables.Contains(typable))
        {
            _activeTypables.Remove(typable);
        }
    }

    private void OnTextInput(char ch)
    {
        if (char.IsControl(ch)) return;

        char upperChar = char.ToUpper(ch);
        ProcessChar(upperChar);
    }

    private void ProcessChar(char c)
    {
        string newBuffer = _currentBuffer + c;
    
        var candidates = _activeTypables
            .Where(t => t != null && !string.IsNullOrEmpty(t.GetWord()) && t.GetWord().StartsWith(newBuffer))
            .ToList();

        if (candidates.Count > 0)
        {
            _currentBuffer = newBuffer;
            UpdateCandidates(candidates);
            // ЗДЕСЬ БОЛЬШЕ НЕТ RegisterCorrectChar()
        }
        else
        {
            var newCandidates = _activeTypables
                .Where(t => t != null && !string.IsNullOrEmpty(t.GetWord()) && t.GetWord().StartsWith(c.ToString()))
                .ToList();
        
            if (newCandidates.Count > 0)
            {
                ResetAll();
                _currentBuffer = c.ToString();
                UpdateCandidates(newCandidates);
                // ЗДЕСЬ БОЛЬШЕ НЕТ RegisterCorrectChar()
            }
            else
            {
                ResetAll();
                ProgressionManager.Instance.RegisterMistake(); // Ошибка всё еще сбрасывает комбо
            }
        }

        CheckCompletion();
    }

    private void UpdateCandidates(List<ITypable> candidates)
    {
        foreach (var t in _activeTypables)
        {
            if (t == null) continue;

            if (candidates.Contains(t))
                t.OnCharTyped(_currentBuffer.Length);
            else
                t.OnReset();
        }
    }

    private void CheckCompletion()
    {
        var completed = _activeTypables.FirstOrDefault(t => 
            t != null && 
            !string.IsNullOrEmpty(_currentBuffer) && 
            t.GetWord() == _currentBuffer);

        if (completed != null)
        {
            ResetAll();
            completed.OnComplete();
        }
    }

    public void ResetAll()
    {
        _currentBuffer = "";
        foreach (var t in _activeTypables)
        {
            if (t != null) t.OnReset();
        }
    }
}
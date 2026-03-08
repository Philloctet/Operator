using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

public class TypingManager : MonoBehaviour
{
    public static TypingManager Instance;

    private string _currentBuffer = "";
    private bool _isMenuMode = false;
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
    
    public void SetMenuMode(bool active)
    {
        _isMenuMode = active;
        ResetAll(); // Сбрасываем текущий буфер при переключении
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
    
        // Фильтруем список активных целей в зависимости от режима
        var candidates = _activeTypables.Where(t => {
            if (t == null) return false;
        
            // Если мы в меню, игнорируем всё, кроме UpgradeCardUI
            if (_isMenuMode) return t is UpgradeCardUI;
        
            // Если мы в игре, игнорируем карточки (на всякий случай)
            return !(t is UpgradeCardUI);
        }).ToList();

        // Дальше идет стандартная логика поиска по буквам внутри отфильтрованных кандидатов
        var finalCandidates = candidates
            .Where(t => !string.IsNullOrEmpty(t.GetWord()) && t.GetWord().StartsWith(newBuffer))
            .ToList();

        if (finalCandidates.Count > 0)
        {
            _currentBuffer = newBuffer;
            UpdateCandidates(finalCandidates);
        }
        else
        {
            // Умный сброс (также с фильтрацией)
            var newCandidates = candidates
                .Where(t => !string.IsNullOrEmpty(t.GetWord()) && t.GetWord().StartsWith(c.ToString()))
                .ToList();
        
            if (newCandidates.Count > 0)
            {
                ResetAll();
                _currentBuffer = c.ToString();
                UpdateCandidates(newCandidates);
            }
            else
            {
                ResetAll();
                // Ошибку в ProgressionManager шлем только если мы НЕ в меню
                if (!_isMenuMode) ProgressionManager.Instance.RegisterMistake();
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
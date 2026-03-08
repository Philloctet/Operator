using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

public class TypingManager : MonoBehaviour
{
    public static TypingManager Instance;

    private List<ITypable> _activeTypables = new List<ITypable>();
    private string _currentBuffer = "";
    private bool _isMenuMode = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        // Подписка на ввод символов через New Input System
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }

    public void RegisterTypable(ITypable typable) => _activeTypables.Add(typable);
    public void UnregisterTypable(ITypable typable) => _activeTypables.Remove(typable);

    // Метод, который запрашивает Enemy для проверки режима фантома
    public string GetCurrentBuffer() => _currentBuffer;

    public void SetMenuMode(bool active)
    {
        _isMenuMode = active;
        ResetAll();
    }

    private void OnTextInput(char ch)
    {
        // Игнорируем управляющие символы (Esc, Enter и т.д.)
        if (char.IsControl(ch)) return;
        ProcessChar(char.ToUpper(ch));
    }

    private void ProcessChar(char c)
    {
        // ПУНКТ №2: Санитарная очистка списка от битых ссылок
        // Удаляем все элементы, которые стали null или у которых уничтожен Transform
        _activeTypables.RemoveAll(t => t == null || t.GetTransform() == null);

        string newBuffer = _currentBuffer + c;

        // Фильтруем цели в зависимости от режима (Бой или Меню)
        var candidates = _activeTypables.Where(t => 
        {
            if (_isMenuMode) return t is UpgradeCardUI;
            return !(t is UpgradeCardUI);
        }).ToList();

        // Ищем совпадения по введенным буквам
        var matches = candidates
            .Where(t => !string.IsNullOrEmpty(t.GetWord()) && t.GetWord().StartsWith(newBuffer))
            .ToList();

        if (matches.Count > 0)
        {
            // Успешный ввод буквы
            _currentBuffer = newBuffer;
            UpdateCandidates(matches);
        }
        else
        {
            // Умный перенос: проверяем, не является ли буква началом ДРУГОГО слова
            var newMatches = candidates
                .Where(t => !string.IsNullOrEmpty(t.GetWord()) && t.GetWord().StartsWith(c.ToString()))
                .ToList();

            if (newMatches.Count > 0)
            {
                ResetAll();
                _currentBuffer = c.ToString();
                UpdateCandidates(newMatches);
            }
            else
            {
                // Полная ошибка
                ResetAll();
                if (!_isMenuMode) ProgressionManager.Instance.RegisterMistake();
            }
        }

        CheckCompletion();
    }

    private void UpdateCandidates(List<ITypable> matches)
    {
        // Сбрасываем всех, кто не попал в новый список совпадений
        foreach (var typable in _activeTypables)
        {
            if (!matches.Contains(typable)) typable.OnReset();
        }

        // Подсвечиваем прогресс у актуальных кандидатов
        foreach (var match in matches)
        {
            match.OnCharTyped(_currentBuffer.Length);
        }
    }

    private void CheckCompletion()
    {
        // Ищем объекты, чьи слова полностью совпадают с буфером
        var completed = _activeTypables
            .Where(t => t != null && t.GetWord() == _currentBuffer)
            .ToList();

        foreach (var t in completed)
        {
            t.OnComplete();
        }

        if (completed.Count > 0)
        {
            ResetAll();
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
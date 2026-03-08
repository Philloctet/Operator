using UnityEngine;

public interface ITypable
{
    string GetWord();
    void OnComplete();
    void OnCharTyped(int index); // Для визуальной подсветки буквы
    void OnReset();              // Если игрок сбросил ввод этого слова
    Transform GetTransform();    // Чтобы знать, где находится объект
}
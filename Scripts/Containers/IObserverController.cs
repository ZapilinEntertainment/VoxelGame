using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObserverController<T>
{
    T GetObserver();
    void SetPosition(RectTransform parent, Rect r, SpriteAlignment alignment);
    void Show<T>(T obj);
    void Show<T>(RectTransform window, Rect r, SpriteAlignment alignment, T obj, bool useCloseButton);
    void Refresh();
    void DisableObserver();
    void DestroyObserver();
}

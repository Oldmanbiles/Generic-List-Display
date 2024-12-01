using UnityEngine;

namespace GenericListDisplay
{
    public interface ICanDisplayInList
    {
        string GetName();
        Sprite GetDisplaySprite();
    }
}
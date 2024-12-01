using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GenericListDisplay
{
    public class GenericListItemUI : MonoBehaviour
    {
        [SerializeField] protected Image DisplayImage;
        [SerializeField] protected TextMeshProUGUI DisplayNameLabel;
        [SerializeField] private GameObject Selected;
        private ICanDisplayInList _displayListTarget;

        public virtual void SetUI<T>(T target, params object[] par) where T : ICanDisplayInList
        {
            SetUI(target);
        }
        
        public virtual void SetUI<T>(T target) where T : ICanDisplayInList
        {
            Deselect();
        }

        public virtual bool MatchTarget<T>(T target) => _displayListTarget is T && _displayListTarget.Equals(target);

        public virtual void Refresh()
        {
            
        }

        public virtual void Select()
        {
            if(Selected != null)
                Selected.gameObject.SetActive(true);
        }

        public virtual void Deselect()
        {
            if(Selected != null)
                Selected.gameObject.SetActive(false);
        }

        public void ToggleSelect()
        {
            if (Selected == null) return;
            if(Selected.activeSelf)
                Deselect();
            else
                Select();
        }

        public virtual bool CanBeSelected => true;
        public bool IsSelected => Selected.gameObject.activeInHierarchy;
    }
}
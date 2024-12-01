using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace GenericListDisplay
{
    public class GenericDisplayListUI : MonoBehaviour
    {
        [SerializeField] private LayoutGroup ItemParent;
        [SerializeField] private GameObject GenericListItemUIPrefab;
        [SerializeField] private ScrollRect ScrollRect;

        private ObjectPool<GenericListItemUI> _pool;
        private List<GenericListItemUI> _tracked;
        private bool _isInitialised;
        public GenericDisplayListUI(LayoutGroup itemParent, GameObject genericListItemUIPrefab, ObjectPool<GenericListItemUI> pool)
        {
            ItemParent = itemParent;
            GenericListItemUIPrefab = genericListItemUIPrefab;
            _pool = pool;
        }

        private void Awake()
        {
            if(!_isInitialised)
                Initialise();
        }

        public void Initialise()
        {
            _pool = new ObjectPool<GenericListItemUI>(OnPoolCreated, OnPoolFetch, OnPoolReleased, OnPoolDestroyed, true,
                6);
            _tracked = new List<GenericListItemUI>();
            _isInitialised = true;
        }

        GenericListItemUI OnPoolCreated()
        {
            var go = Instantiate(GenericListItemUIPrefab, ItemParent.transform);
            var ui = go.GetComponent<GenericListItemUI>();
            go.transform.SetAsLastSibling();
            _tracked.Add(ui);
            return ui;
        }

        void OnPoolFetch(GenericListItemUI ui)
        {
            ui.gameObject.SetActive(true);
            _tracked.Add(ui);
        }

        void OnPoolReleased(GenericListItemUI ui)
        {
            var btn = ui.GetComponent<Button>();
            if(btn)
                btn.onClick.RemoveAllListeners();
            
            _tracked.Remove(ui);
            ui.gameObject.SetActive(false);
        }

        void OnPoolDestroyed(GenericListItemUI ui)
        {
            _tracked.Remove(ui);
            Destroy(ui.gameObject);
        }

        public void SetList<T>(List<T> inputList,bool selectFirst = false, Action<T> clickCallback = null) where T : ICanDisplayInList
        {
            if(!_isInitialised)
                Initialise();
            
            ClearList();
            
            if (inputList.IsNullOrEmpty())
            {
                return;
            }

            int lowestIndex = 99999;
            T lowestT = default(T);
            GenericListItemUI lowestUI = null;
            int index = 0;

            foreach (T target in inputList)
            {
                var ui = _pool.Get();
                ui.transform.SetSiblingIndex(index);
                ui.SetUI(target);
                if(clickCallback == null) continue;
                var button = ui.GetComponent<Button>();
                if(button != null)
                    button.onClick.AddListener(() =>
                    {
                        clickCallback.Invoke(target);
                        _tracked.ForEach(x => x.Deselect());
                        ui.Select();
                    });
                int childIndex = ui.transform.GetSiblingIndex();
                if (childIndex < lowestIndex)
                {
                    lowestT = target;
                    lowestUI = ui;
                    lowestIndex = childIndex;
                }

                index++;
            }

            if (selectFirst && lowestUI != null)
            {
                clickCallback.Invoke(lowestT);
                _tracked.ForEach(x => x.Deselect());
                lowestUI.Select();
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(ItemParent.transform as RectTransform);
        }

        public void SelectAndCenterObject<T>(T value)
        {
            GenericListItemUI ui = null;
            foreach (var listItemUI in _tracked)
            {
                if (!listItemUI.gameObject.activeSelf) continue;
                if(!listItemUI.MatchTarget(value)) continue;
                ui = listItemUI;
            }

            if (ui == null) return;
            ui.Select();
            SetSnapToPositionToBringChildIntoView((RectTransform)ui.transform);
        }
        
        public void SetSnapToPositionToBringChildIntoView(RectTransform child)
        {
            Canvas.ForceUpdateCanvases();
            Vector2 viewportLocalPosition = ScrollRect.viewport.localPosition;
            Vector2 childLocalPosition   = child.localPosition;
            Vector2 result = new Vector2(
                0 - (viewportLocalPosition.x + childLocalPosition.x),
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );
            ScrollRect.content.localPosition = result;
        }

        public void RefreshList()
        {
            foreach (Transform t in ItemParent.transform)
            {
                if(!t.gameObject.activeSelf) continue;
                t.GetComponent<GenericListItemUI>().Refresh();
            }
        }

        public void ClearList()
        {
            foreach (Transform t in ItemParent.transform)
            {
                if(!t.gameObject.activeSelf) continue;
                _pool.Release(t.GetComponent<GenericListItemUI>());
            }
        }

        public int PrefabListCount => _pool.CountActive;
        public List<GenericListItemUI> Tracked => _tracked;
    }
}
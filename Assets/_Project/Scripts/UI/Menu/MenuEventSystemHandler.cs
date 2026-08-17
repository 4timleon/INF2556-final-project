using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuEventSystemHandler : MonoBehaviour {
    [Header("References")]
    [SerializeField] protected List<Selectable> selectables = new List<Selectable>();
    [SerializeField] protected Selectable _firstSelected;

    [Header("Controls")]
    [SerializeField] protected PlayerInputHandler playerInputHandler;

    [Header("Animation")]
    [SerializeField] protected float _selectedAnimationScale = 1.1f;
    [SerializeField] protected float _scaleDuration = 0.25f;
    [SerializeField] protected List<GameObject> _animationExclusion = new List<GameObject>();

    [Header("Sounds")]
    [SerializeField] protected UnityEvent SoundEvent;

    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();

    protected Selectable _lastSelected;

    protected LTDescr _scaleUpTween;
    protected LTDescr _scaleDownTween;

    private void Awake() {
        foreach (var selectable in selectables) {
            AddSelectionListeners(selectable);
            _scales.Add(selectable, selectable.transform.localScale);
        }
    }

    public virtual void OnEnable() {
        //Ensure all selectables are reset back to original size
        for (int i = 0; i < selectables.Count; i++) {
            selectables[i].transform.localScale = _scales[selectables[i]];
        }

        StartCoroutine(SelectAfterDelay());

        playerInputHandler.OnNavigateAction += PlayerInputHandler_OnNavigate;
    }

    protected virtual IEnumerator SelectAfterDelay() {
        yield return null;
        EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
    }

    public virtual void OnDisable() {
        playerInputHandler.OnNavigateAction -= PlayerInputHandler_OnNavigate;
    }

    protected virtual void AddSelectionListeners(Selectable selectable) {

        //Add Listener
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        //Add Select Event
        EventTrigger.Entry selectEntry = new EventTrigger.Entry {
            eventID = EventTriggerType.Select
        };
        selectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(selectEntry);

        //Add Deselect Event
        EventTrigger.Entry deselectEntry = new EventTrigger.Entry {
            eventID = EventTriggerType.Deselect
        };
        deselectEntry.callback.AddListener(OnDeselect);
        trigger.triggers.Add(deselectEntry);

        //Add OnPointerEnter Event
        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnterEntry.callback.AddListener(OnPointerEnter);
        trigger.triggers.Add(pointerEnterEntry);

        //Add OnPointerExit Event
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerExit
        };
        pointerExitEntry.callback.AddListener(OnPointerExit);
        trigger.triggers.Add(pointerExitEntry);

        //Add OnPointerClick Event
        EventTrigger.Entry pointerClick = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerClick
        };
        pointerClick.callback.AddListener(OnPointerClick);
    }

    public void OnSelect(BaseEventData eventData) {
        SoundEvent?.Invoke();
        _lastSelected = eventData.selectedObject.GetComponent<Selectable>();

        if (_animationExclusion.Contains(eventData.selectedObject))
            return;

        Vector3 newScale = eventData.selectedObject.transform.localScale * _selectedAnimationScale;
        _scaleUpTween = eventData.selectedObject.transform.LeanScale(newScale, _scaleDuration);
    }

    public void OnDeselect(BaseEventData eventData) {
        if (_animationExclusion.Contains(eventData.selectedObject))
            return;

        Selectable selectable = eventData.selectedObject.GetComponent<Selectable>();
        _scaleDownTween = eventData.selectedObject.transform.LeanScale(_scales[selectable], _scaleDuration);
    }

    private void OnPointerEnter(BaseEventData eventData) {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null) {
            Selectable selectable = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
            if (selectable == null) {
                selectable = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }

            pointerEventData.selectedObject = selectable.gameObject;
        }
    }

    private void OnPointerExit(BaseEventData eventData) {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null) {
            pointerEventData.selectedObject = null;
        }
    }

    private void OnPointerClick(BaseEventData eventData) {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null) {
            if (pointerEventData.button == PointerEventData.InputButton.Left) {

            }
        }
    }

    protected virtual void PlayerInputHandler_OnNavigate() {
        if (EventSystem.current.currentSelectedGameObject == null && _lastSelected != null) {
            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
        }
    }

}
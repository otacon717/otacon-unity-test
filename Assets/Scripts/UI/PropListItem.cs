using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// One entry of the prop dropdown list. Raises static drag events so the
/// placement system can react without the UI knowing about it.
/// </summary>
public class PropListItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static event Action<PropDefinition, PointerEventData> PropDragStarted;
    public static event Action<PointerEventData> PropDragMoved;
    public static event Action<PointerEventData> PropDragEnded;

    private PropDefinition definition;

    public void Bind(PropDefinition def)
    {
        definition = def;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (definition != null)
        {
            PropDragStarted?.Invoke(definition, eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        PropDragMoved?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        PropDragEnded?.Invoke(eventData);
    }
}

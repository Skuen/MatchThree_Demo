using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiamondController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Vector2 _startPos;
    Vector2 _endPos;
    Vector3 _destination;

    bool _swapping;
    float _timer;

    public void OnPointerDown(PointerEventData eventData)
    {
        _startPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _endPos = eventData.position;

        Vector2 delta = _endPos - _startPos;

        if (delta.magnitude > 0.2f)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x > 0)
                {
                    _destination = transform.localPosition + Vector3.right * GetComponent<RectTransform>().sizeDelta.x / 2;
                }
                else
                {
                    _destination = transform.localPosition + Vector3.left * GetComponent<RectTransform>().sizeDelta.x / 2;
                }
            }
            else
            {
                if (delta.y > 0)
                {
                    _destination = transform.localPosition + Vector3.up * GetComponent<RectTransform>().sizeDelta.y / 2;
                }
                else
                {
                    _destination = transform.localPosition + Vector3.down * GetComponent<RectTransform>().sizeDelta.y / 2;
                }
            }
            _swapping = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_swapping)
        {
            transform.position = Vector3.Lerp(transform.localPosition, _destination, 0.1f);
            if (transform.position == _destination)
            {
                _swapping = false;
            }
        }
    }
}

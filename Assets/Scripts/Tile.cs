using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private static Tile selected=null; // 1 
    private SpriteRenderer Renderer; // 2
    public Vector2Int Position;
    private void Start()
    {
        Renderer = GetComponent<SpriteRenderer>();
    }
    public void Select() // 4
    {
        Renderer.color = Color.grey;
    }

    public void Unselect() // 5 
    {
        Renderer.color = Color.white;
    }

    private void OnMouseDown()
    {
        if (Renderer.sprite == null) return;
        if (selected != null)
        {
            if (selected == this)
                return;
            selected.Unselect();
            if (Vector2Int.Distance(selected.Position, Position) == 1)
            {
                GridManager.Instance.SwapTiles(Position, selected.Position);
                selected = null;
            }
            else
            {
                SoundManager.Instance.PlaySound(SoundType.TypeSelect);
                selected = this;
                Select();
            }
        }
        else
        {
            SoundManager.Instance.PlaySound(SoundType.TypeSelect);
            selected = this;
            Select();
        }
    }
    
}

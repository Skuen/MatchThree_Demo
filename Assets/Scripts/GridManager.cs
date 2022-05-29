using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
public class GridManager : MonoBehaviour
{
    public List<Sprite> Sprites = new List<Sprite>();
    public GameObject TilePrefab;
    public int GridDimension = 8;
    public float Distance = 1.0f;
    private GameObject[,] Grid;
    float TweenDuration = 0.25f;
    private Sequence sequence;


    //score
    public GameObject GameOverMenu; // 2
    public GameObject PauseBtn;
    public TextMeshProUGUI MovesText;
    public TextMeshProUGUI ScoreText;

    public int StartingMoves = 50; // 2
    private int _numMoves; // 3
    private int _score;
    public int NumMoves
    {
        get
        {
            return _numMoves;
        }
        set
        {
            _numMoves = value;
            MovesText.text = _numMoves.ToString();
        }
    }
    public int Score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            ScoreText.text = _score.ToString();
        }
    }
    //

    // Start is called before the first frame update
    public static GridManager Instance { get; private set; }
    void Awake() {
        Instance = this;
        Score = 0;
        NumMoves = StartingMoves;
        GameOverMenu.SetActive(false);
    }
    void Start()
    {
        Grid = new GameObject[GridDimension, GridDimension];
        InitGrid();
    }
    void InitGrid()
    {
        Vector3 positionOffset = transform.position - new Vector3(GridDimension * Distance / 2.0f, GridDimension * Distance / 2.0f, 0); // 1
        for (int row = 0; row < GridDimension; row++)
            for (int column = 0; column < GridDimension; column++) // 2
            {
                List<Sprite> possibleSprites = new List<Sprite>(Sprites); // 1

                //Choose what sprite to use for this cell 
                //also do this to check matches, we go through down right to top left so we only need to check left and down
                //if the left 2 have the same color with left1 we remove them from the possile sprites list - the same with down
                Sprite left1 = GetSpriteAt(column - 1, row); //2
                Sprite left2 = GetSpriteAt(column - 2, row);
                if (left2 != null && left1 == left2) // 3
                {
                    possibleSprites.Remove(left1); // 4
                }

                Sprite down1 = GetSpriteAt(column, row - 1); // 5
                Sprite down2 = GetSpriteAt(column, row - 2);
                if (down2 != null && down1 == down2)
                {
                    possibleSprites.Remove(down1);
                }

                //init tile
                GameObject newTile = Instantiate(TilePrefab); // 3
                SpriteRenderer renderer = newTile.GetComponent<SpriteRenderer>(); // 4
                renderer.sprite = possibleSprites[Random.Range(0, possibleSprites.Count)]; // 5

                //add pos for tile 
                Tile tile = newTile.AddComponent<Tile>();
                tile.Position = new Vector2Int(column, row);

                newTile.transform.parent = transform; // 6
                newTile.transform.position = new Vector3(column * Distance, row * Distance, 0) + positionOffset; // 7

                Grid[column, row] = newTile; // 8
            }
    }

    Sprite GetSpriteAt(int column, int row)
    {
        if (column < 0 || column >= GridDimension
            || row < 0 || row >= GridDimension)
            return null;
        GameObject tile = Grid[column, row];
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();

        return renderer.sprite;
    }
    private IEnumerator SwapAnimate(GameObject tile1, GameObject tile2)
    {
        sequence = DOTween.Sequence();
        sequence.Join(tile1.transform.DOMove(tile2.transform.position, TweenDuration))
            .Join(tile2.transform.DOMove(tile1.transform.position, TweenDuration));
        sequence.Play();
        yield return new WaitForSeconds(TweenDuration);
    }
    public void SwapTiles(Vector2Int tile1Position, Vector2Int tile2Position) // 1
    {

        // 2
        GameObject tile1 = Grid[tile1Position.x, tile1Position.y];
        SpriteRenderer renderer1 = tile1.GetComponent<SpriteRenderer>();

        GameObject tile2 = Grid[tile2Position.x, tile2Position.y];
        SpriteRenderer renderer2 = tile2.GetComponent<SpriteRenderer>();
        //swap animmation
        //StartCoroutine(SwapAnimate(tile1, tile2));

        // move tile
        Sprite temp = renderer1.sprite;
        renderer1.sprite = renderer2.sprite;
        renderer2.sprite = temp;

        bool changesOccurs = CheckMatches();
        if (!changesOccurs)
        {

            //StartCoroutine(SwapAnimate(tile1, tile2));
            temp = renderer1.sprite;
            renderer1.sprite = renderer2.sprite;
            renderer2.sprite = temp;
            SoundManager.Instance.PlaySound(SoundType.TypeMove);
        }
        else
        {
            SoundManager.Instance.PlaySound(SoundType.TypePop);
            NumMoves--;
            do
            {
                FillHoles();
            } while (CheckMatches());
            if (NumMoves <= 0)
            {
                NumMoves = 0; GameOver();
            }
        }
    }
    SpriteRenderer GetSpriteRendererAt(int column, int row)
    {
        if (column < 0 || column >= GridDimension
             || row < 0 || row >= GridDimension)
            return null;
        GameObject tile = Grid[column, row];
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        return renderer;
    }
    bool CheckMatches()
    {
        HashSet<SpriteRenderer> matchedTiles = new HashSet<SpriteRenderer>(); // 1
        for (int row = 0; row < GridDimension; row++)
        {
            for (int column = 0; column < GridDimension; column++) // 2
            {
                SpriteRenderer current = GetSpriteRendererAt(column, row); // 3

                List<SpriteRenderer> horizontalMatches = FindColumnMatchForTile(column, row, current.sprite); // 4
                if (horizontalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(horizontalMatches);
                    matchedTiles.Add(current); // 5
                    if (horizontalMatches.Count >= 4) Score += horizontalMatches.Count - 3;


                }

                List<SpriteRenderer> verticalMatches = FindRowMatchForTile(column, row, current.sprite); // 6
                if (verticalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(verticalMatches);
                    matchedTiles.Add(current);
                    if (verticalMatches.Count >= 4) Score += verticalMatches.Count - 3;
                }
            }
        }

        foreach (SpriteRenderer renderer in matchedTiles) // 7
        {
            renderer.sprite = null;
        }
        Score += matchedTiles.Count;
        return matchedTiles.Count > 0;
    }
    List<SpriteRenderer> FindColumnMatchForTile(int col, int row, Sprite sprite)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        for (int i = col + 1; i < GridDimension; i++)
        {
            SpriteRenderer nextColumn = GetSpriteRendererAt(i, row);
            if (nextColumn.sprite != sprite)
            {
                break;
            }
            result.Add(nextColumn);
        }
        return result;
    }
    List<SpriteRenderer> FindRowMatchForTile(int col, int row, Sprite sprite)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        for (int i = row + 1; i < GridDimension; i++)
        {
            SpriteRenderer nextRow = GetSpriteRendererAt(col, i);
            if (nextRow.sprite != sprite)
            {
                break;
            }
            result.Add(nextRow);
        }
        return result;
    }

    void FillHoles()
    {
        for (int column = 0; column < GridDimension; column++)
        {
            for (int row = 0; row < GridDimension; row++) // 1
            {
                while (GetSpriteRendererAt(column, row).sprite == null) // 2
                {
                    SpriteRenderer current = GetSpriteRendererAt(column, row);
                    SpriteRenderer next = current;
                    for (int filler = row; filler < GridDimension - 1; filler++) // 3
                    {
                        next = GetSpriteRendererAt(column, filler + 1);
                        current.sprite = next.sprite;
                        current = next;
                    }
                    next.sprite = Sprites[Random.Range(0, Sprites.Count)];
                }
            }
        }
    }
    
    public void GameOver()
    {
        Debug.Log("GAME OVER");
    PlayerPrefs.SetInt("score", Score);
    GameOverMenu.SetActive(true);
        PauseBtn.SetActive(false);
    SoundManager.Instance.PlaySound(SoundType.TypeGameOver);
    }
    //score func

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum BlockType { Type1,Type2,Type3,Type4,Type5}
/*
class TestBlock
{
    private int x { get; set; }
    private int y{ get; set; }

    internal BlockType BlockType
    {
        get
        {
            return blockType;
        }

        set
        {
            blockType = value;
        }
    }

    private BlockType blockType;
    public TestBlock(int x,int y, BlockType blockType)
    {
        this.x = x;
        this.y = y;
        this.BlockType = blockType;
    }
}


class Board
{
    private int xMax;
    private int yMax;
    public List<TestBlock> boardBlockList;
    public Board(int x,int y)
    {
        xMax = x;
        yMax = y;
        boardBlockList = new List<TestBlock>();
    }
    public void init()
    {
        for(int i = 0; i < yMax; i++)
        {
            for(int j = 0; j <xMax; j++)
            {


                TestBlock block = new TestBlock(j,i,typeRandom());
                boardBlockList.Add(block);
            }
        }
       
    }
    public BlockType typeRandom()
    {
        int i = Random.Range(1, 5);
        switch (i)
        {
            case 1:return BlockType.Type1;
                break;
            case 2:return BlockType.Type2;
                break;
            case 3:return BlockType.Type3;
                break;
            case 4:return BlockType.Type4;
                break;
            case 5:return BlockType.Type5;
                break;
        }
        return BlockType.Type1;
    }
    
}

struct ViewBlock
{
    public int x;
    public int y;
    public GameObject instance;
    public ViewBlock(int x,int y)
    {
        this.x = x;
        this.y = y;
        instance = null;
    }
}

class View
{
    private int xMax;
    private int yMax; 

    public int XMax
    {
        get
        {
            return xMax;
        }

        set
        {
            xMax = value;
        }
    }

    public int YMax
    {
        get
        {
            return yMax;
        }

        set
        {
            yMax = value;
        }
    }

    public  List<ViewBlock> viewBlockList;
    public View(int x,int y)
    {
        XMax = x;
        YMax = y;
        viewBlockList = new List<ViewBlock>();
    }
    public void init()
    {
        for (int i = 0; i < YMax; i++)
        {
            for (int j = 0; j < XMax; j++)
            {
                ViewBlock vb = new ViewBlock(j,i);
                viewBlockList.Add(vb);
            }
        }
    }
}

public class MapControl : MonoBehaviour {
    [SerializeField]
    private int boardX;
    [SerializeField]
    private int boardY;
    [SerializeField]
    private int viewX;
    [SerializeField]
    private int viewY;
    [SerializeField]
    private GameObject blockType1;
    [SerializeField]
    private GameObject blockType2;
    [SerializeField]
    private GameObject blockType3;
    [SerializeField]
    private GameObject blockType4;
    [SerializeField]
    private GameObject blockType5;

    private Board board;
    private View view;


    void initBlocks()
    {
        foreach (ViewBlock vb in view.viewBlockList)
        {
            int x = vb.x*2;
            int y = -vb.y*2;
            BlockType blocktype = board.boardBlockList[vb.x + view.XMax * vb.y].BlockType;

            GameObject gameObject = null;
            switch (blocktype)
            {
                case BlockType.Type1:
                    gameObject = blockType1;
                    break;
                case BlockType.Type2:
                    gameObject = blockType2;
                    break;
                case BlockType.Type3:
                    gameObject = blockType3;
                    break;
                case BlockType.Type4:
                    gameObject = blockType4;
                    break;
                case BlockType.Type5:
                    gameObject = blockType5;
                    break;

            }

             Instantiate(gameObject, new Vector3(x, y, 0), Quaternion.identity);
        }
    }
    // Use this for initialization
    void Awake () {
         board = new Board(boardX, boardY);
        board.init();
        view = new View(viewX,viewY);
        view.init();
       // Debug.Log(view.viewBlockList.Count);
    }
     void Start()
    {
        initBlocks();
    }

    // Update is called once per frame
    void Update () {
		
	}
}

    */

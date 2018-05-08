using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public static float COLLISION_SIZE = 1.0f; // 블록의 충돌 크기.
    public static float VANISH_TIME = 3.0f; // 불 붙고 사라질 때까지의 시간.
    public struct iPosition
    { // 그리드에서의 좌표를 나타내는 구조체.
        public int x; // X 좌표.
        public int y; // Y 좌표.
    }
    public enum COLOR
    { // 블록 색상.
        NONE = -1, // 색 지정 없음.
        PINK = 0, // 분홍색.
        BLUE, // 파란색.
        YELLOW, // 노란색.
        GREEN, // 녹색.
        MAGENTA, // 마젠타.
        ORANGE, // 주황색.
        GRAY, // 그레이.
        NUM, // 컬러가 몇 종류인지 나타낸다(=7).
        FIRST = PINK, // 초기 컬러(분홍색).
        LAST = ORANGE, // 최종 컬러(주황색).
        NORMAL_COLOR_NUM = GRAY, // 보통 컬러(회색 이외의 색)의 수.
    };
    public enum DIR4
    { // 상하좌우 네 방향.
        NONE = -1, // 방향지정 없음.
        RIGHT, // 우.
        LEFT, // 좌.
        UP, // 상.
        DOWN, // 하.
        NUM, // 방향이 몇 종류 있는지 나타낸다(=4).
    };
    public static int BLOCK_NUM_X = 9;
    // 블록을 배치할 수 있는 X방향 최대수.
    public static int BLOCK_NUM_Y = 9;
    // 블록을 배치할 수 있는 Y방향 최대수.
}

// BlockControl.cs: BlockControl class
// 블록을 조작하는 클래스이다.
public class BlockControl : MonoBehaviour
{
    public Block.COLOR color = (Block.COLOR)0; // 블록 색.
    public BlockRoot block_root = null; // 블록의 신.
    public Block.iPosition i_pos; // 블록 좌표.
    void Start()
    {
        this.setColor(this.color); // 색칠을 한다.
    }
    void Update()
    {
    }
    // 인수 color의 색으로 블록을 칠한다.
    public void setColor(Block.COLOR color)
    {
        this.color = color; // 이번에 지정된 색을 멤버 변수에 보관한다.
        Color color_value; // Color 클래스는 색을 나타낸다.
        switch (this.color)
        { // 칠할 색에 따라서 갈라진다.
            default:
            case Block.COLOR.PINK:
                color_value = new Color(1.0f, 0.5f, 0.5f);
                break;
            case Block.COLOR.BLUE:
                color_value = Color.blue;
                break;
            case Block.COLOR.YELLOW:
                color_value = Color.yellow;
                break;
            case Block.COLOR.GREEN:
                color_value = Color.green;
                break;
            case Block.COLOR.MAGENTA:
                color_value = Color.magenta;
                break;
            case Block.COLOR.ORANGE:
                color_value = new Color(1.0f, 0.46f, 0.0f);
                break;
        }
        // 이 게임 오브젝트의 머티리얼 색상을 변경한다.
        this.GetComponent<Renderer>().material.color = color_value;
    }
}
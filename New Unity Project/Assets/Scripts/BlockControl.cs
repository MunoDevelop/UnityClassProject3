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

    public enum STEP
    {
        NONE = -1, // 상태 정보 없음.
        IDLE = 0, // 대기 중.
        GRABBED, // 잡혀 있음.
        RELEASED, // 떨어진 순간.
        SLIDE, // 슬라이드 중.
        VACANT, // 소멸 중.
        RESPAWN, // 재생성 중.
        FALL, // 낙하 중.
        LONG_SLIDE, // 크게 슬라이드 중.
        NUM, // 상태가 몇 종류인지 표시.
    };
}

// BlockControl.cs: BlockControl class
// 블록을 조작하는 클래스이다.
public class BlockControl : MonoBehaviour
{
    public Block.COLOR color = (Block.COLOR)0; // 블록 색.
    public BlockRoot block_root = null; // 블록의 신.
    public Block.iPosition i_pos; // 블록 좌표.
    public Block.STEP step = Block.STEP.NONE; // 지금 상태.
    public Block.STEP next_step = Block.STEP.NONE; // 다음 상태.
    private Vector3 position_offset_initial = Vector3.zero; // 교체 전 위치.
    public Vector3 position_offset = Vector3.zero; // 교체 후 위치.
    void Start()
    {
        this.setColor(this.color);
        this.next_step = Block.STEP.IDLE; // 다음 블록을 대기중으로.
    }
    void Update()
    {
        Vector3 mouse_position; // 마우스 위치.
        this.block_root.unprojectMousePosition( // 마우스 위치 획득.
        out mouse_position, Input.mousePosition);
        // 획득한 마우스 위치를 X와 Y만으로 한다.
        Vector2 mouse_position_xy =
        new Vector2(mouse_position.x, mouse_position.y);
        // '다음 블록' 상태가 '정보 없음' 이외인 동안.
        // ＝'다음 블록' 상태가 변경된 경우.
        while (this.next_step != Block.STEP.NONE)
        {
            this.step = this.next_step;
            this.next_step = Block.STEP.NONE;
            switch (this.step)
            {
                case Block.STEP.IDLE: // '대기' 상태.
                    this.position_offset = Vector3.zero;
                    // 블록 표시 크기를 보통 크기로 한다.
                    this.transform.localScale = Vector3.one * 1.0f;
                    break;
                case Block.STEP.GRABBED: // '잡힌' 상태.
                                         // 블록 표시 크기를 크게 한다.
                    this.transform.localScale = Vector3.one * 1.2f;
                    break;
                case Block.STEP.RELEASED: // '떨어져 있는' 상태.
                    this.position_offset = Vector3.zero;
                    // 블록 표시 크기를 보통 사이즈로 한다.
                    this.transform.localScale = Vector3.one * 1.0f;
                    break;
            }
        }
        // 그리드 좌표를 실제 좌표(씬의 좌표)로 변환하고.
        // position_offset을 추가한다.
        Vector3 position =
        BlockRoot.calcBlockPosition(this.i_pos) + this.position_offset;
        // 실제 위치를 새로운 위치로 변경한다.
        this.transform.position = position;
    }
    public void beginGrab() // 잡혔을 때 호출한다.
    {
        this.next_step = Block.STEP.GRABBED;
    }
    public void endGrab() // 놓았을 때 호출한다.
    {
        this.next_step = Block.STEP.IDLE;
    }
    public bool isGrabbable() // 잡을 수 있는 상태 인지 판단한다.
    {
        bool is_grabbable = false;
        switch (this.step)
        {
            case Block.STEP.IDLE: // '대기' 상태일 때만.
                is_grabbable = true; // true(잡을 수 있다)를 반환한다.
                break;
        }
        return (is_grabbable);
    }
    public bool isContainedPosition(Vector2 position)
    {
        bool ret = false;
        Vector3 center = this.transform.position;
        float h = Block.COLLISION_SIZE / 2.0f;
        do
        {
            // X 좌표가 자신과 겹치지 않으면 break로 루프를 빠져 나간다.
            if (position.x < center.x - h || center.x + h < position.x)
            {
                break;
            }
            // Y 좌표가 자신과 겹치지 않으면 break로 루프를 빠져 나간다.
            if (position.y < center.y - h || center.y + h < position.y)
            {
                break;
            }
            // X 좌표, Y 좌표 모두 겹쳐 있으면 true(겹쳐 있다)를 반환한다.
            ret = true;
        } while (false);
        return (ret);
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
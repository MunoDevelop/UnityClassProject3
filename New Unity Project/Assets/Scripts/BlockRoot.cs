﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderBlock{

   public UnderBlock(Block.COLOR color,GameObject instance)
    {
        
        this.color = color;
        this.instance = instance;
    }
    
    public Block.COLOR color;
    public GameObject instance;
}

public class BlockRoot : MonoBehaviour
{
    public GameObject BlockPrefab = null; // 만들어낼 블록의 프리팹.
    
    public BlockControl[,] blocks; // 그리드.

    public GameObject queueBlockPrefeb = null;

    public Queue<Block.COLOR> colorQueue;
    public Queue<UnderBlock> underBlockQueue;

    private GameObject main_camera = null; // 메인 카메라.
    private BlockControl grabbed_block = null; // 잡은 블록.

    private ScoreCounter score_counter = null; // 점수 카운터 ScoreCounter.
    protected bool is_vanishing_prev = false; // 앞에서 발화했는가?

    public TextAsset levelData = null; // 레벨 데이터의 텍스트를 저장.
    public LevelControl level_control; // LevelControl를 저장.

    public void create()
    {
        this.level_control = new LevelControl();
        this.level_control.initialize(); // 레벨 데이터 초기화.
        this.level_control.loadLevelData(this.levelData); // 데이터 읽기.
        this.level_control.selectLevel(); // 레벨 선택.
    }

   
    public Block.COLOR selectBlockColor()
    {
        Block.COLOR color = Block.COLOR.FIRST;
        // 이번 레벨의 레벨 데이터를 가져온다.
        LevelData level_data = this.level_control.getCurrentLevelData();
        float rand = Random.Range(0.0f, 1.0f); // 0.0~1.0 사이의 난수.
        float sum = 0.0f; // 출현 확률의 합계.
        int i = 0;
        // 블록의 종류 전체를 처리하는 루프.
        for (i = 0; i < level_data.probability.Length - 1; i++)
        {
            if (level_data.probability[i] == 0.0f)
            {
                continue; // 출현 확률이 0이면 루프의 처음으로 점프.
            }
            sum += level_data.probability[i]; // 출현 확률을 더한다.
            if (rand < sum)
            { // 합계가 난숫값을 웃돌면.
                break; // 루프를 빠져나온다.
            }
        }
        color = (Block.COLOR)i; // i번째 색을 반환한다.
        return (color);
    }

    public void BombEffect(Block.iPosition iPosition)
    {
        switch (Random.Range(1, 4))
        {
            case 1:
                foreach (BlockControl block in blocks)
                {
                    if (block.i_pos.x == iPosition.x)
                    {
                        block.toVanishing();
                        colorQueue.Enqueue(this.blocks[block.i_pos.x, block.i_pos.y].color);
                    }
                }
                break;
            case 2:
                foreach (BlockControl block in blocks)
                {
                    if (block.i_pos.y == iPosition.y)
                    {
                        block.toVanishing();
                        colorQueue.Enqueue(this.blocks[block.i_pos.x, block.i_pos.y].color);
                    }
                }
                break;
            case 3:
                foreach (BlockControl block in blocks)
                {
                    if ((block.i_pos.y == iPosition.y)|| (block.i_pos.x == iPosition.x))
                    {
                        block.toVanishing();
                        colorQueue.Enqueue(this.blocks[block.i_pos.x, block.i_pos.y].color);
                    }
                }
                break;
        }
       

    }





    void Start()
    {
        this.main_camera =
        GameObject.FindGameObjectWithTag("MainCamera");
        // 카메라로부터 마우스 커서를 통과하는 광선을 쏘기 위해서 필요

        this.score_counter = this.gameObject.GetComponent<ScoreCounter>();

        colorQueue = new Queue<Block.COLOR>();
        underBlockQueue = new Queue<UnderBlock>();
    }
    // 마우스 좌표와 겹치는지 체크한다.
    // 잡을 수 있는 상태의 블록을 잡는다.
    void Update()
    {
        Vector3 mouse_position; // 마우스 위치.
        this.unprojectMousePosition( // 마우스 위치를 가져온다.
        out mouse_position, Input.mousePosition);
        // 가져온 마우스 위치를 하나의 Vector2로 모은다.
        Vector2 mouse_position_xy =
        new Vector2(mouse_position.x, mouse_position.y);
        if (this.grabbed_block == null)
        { // 잡은 블록이 비었으면.
          // 나중에 주석 해제
           if(!this.is_has_falling_block()) {
            if (Input.GetMouseButtonDown(0))
            {
                // 마우스 버튼이 눌렸으면
                // blocks 배열의 모든 요소를 차례로 처리한다.
                foreach (BlockControl block in this.blocks)
                {
                    if (!block.isGrabbable())
                        { // 블록을 잡을 수 없다면.ignite_count
                          //  Debug.Log("not ");
                            continue; // 루프의 처음으로 점프한다.
                    } // 마우스 위치가 블록 영역 안이 아니면.
                    if (!block.isContainedPosition(mouse_position_xy))
                    {
                        continue;
                    } // 루프의 처음으로 점프한다.
                      // 처리 중인 블록을 grabbed_block에 등록한다.
                    this.grabbed_block = block;
                    // 잡았을 때의 처리를 실행한다.
                    this.grabbed_block.beginGrab();
                    break;
                }
            }
             }
        }
        else
        { // 블록을 잡았을 때.
            do
            {
                // 슬라이드할 곳의 블록을 가져온다.
                BlockControl swap_target =
                this.getNextBlock(grabbed_block, grabbed_block.slide_dir);
                // 슬라이드할 곳 블록이 비어 있으면.
                if (swap_target == null)
                {
                    break; // 루프 탈출.
                }
                // 슬라이드할 곳의 블록이 잡을 수 있는 상태가 아니라면.
                if (!swap_target.isGrabbable())
                {
                   // Debug.Log("not ");
                    break; // 루프 탈출.
                }
                // 현재 위치에서 슬라이드 위치까지의 거리를 얻는다.
                float offset = this.grabbed_block.calcDirOffset(
                mouse_position_xy, this.grabbed_block.slide_dir);
                // 수리 거리가 블록 크기의 절반보다 작다면.
                if (offset < Block.COLLISION_SIZE / 2.0f)
                {
                    break; // 루프 탈출.
                }
                // 블록을 교체한다.
                this.swapBlock(
                grabbed_block, grabbed_block.slide_dir, swap_target);
                this.grabbed_block = null; // 지금은 블록을 잡고 있지 않다.
            } while (false);

            if (!Input.GetMouseButton(0))
            { // 마우스 버튼이 눌려져 있지 않으면.
                this.grabbed_block.endGrab(); // 블록을 놨을 때의 처리를 실행.
                this.grabbed_block = null; // grabbed_block을 비우게 설정.
            }
        }

        if (this.is_has_falling_block() || this.is_has_sliding_block())
        {
            // 아무것도 하지 않는다.
            // 낙하 중도 슬라이드 중도 아니면.
        }
        else
        {
            int ignite_count = 0; // 불붙은 개수.
                                  // 그리드 안의 모든 블록에 대해서 처리.
            foreach (BlockControl block in this.blocks)
            {
                if (!block.isIdle())
                { // 대기 중이면 루프의 처음으로 점프하고.
                    continue; // 다음 블록을 처리한다.
                }
                // 세로 또는 가로에 같은 색 블록이 세 개 이상 나열했다면.
                if (this.checkConnection(block))
                {
                    ignite_count++; // 불붙은 개수를 증가.
                }
            }
            if (ignite_count > 0)
            {
                if (!this.is_vanishing_prev)
                {
                    // 연속 점화가 아니라면, 점화 횟수를 리셋.
                    this.score_counter.clearIgniteCount();
                }
                // 점화 횟수를 증가.
                this.score_counter.addIgniteCount(ignite_count);
                // 합계 점수 갱신.
                this.score_counter.updateTotalScore();


                // ＝한 군데라도 맞춰진 곳이 있으면.
                int block_count = 0; // 불붙는 중인 블록 수(다음 장에서 사용한다).
                                     // 그리드 내의 모든 블록에 대해서 처리.
                foreach (BlockControl block in this.blocks)
                {
                    if (block.isVanishing())
                    { // 타는 중이면.
                        block.rewindVanishTimer(); // 다시 점화!.
                        block_count++;
                    }
                }
            }
        }

        // 하나라도 연소 중인 블록이 있는가?.
        bool is_vanishing = this.is_has_vanishing_block();
        // 조건이 만족되면 블록을 떨어뜨리고 싶다.
        do
        {
            if (is_vanishing)
            { // 연소 중인 블록이 있다면.
                break; // 낙하 처리를 실행하지 않는다.
            }
            if (this.is_has_sliding_block())
            { // 교체 중인 블록이 있다면.
                break; // 낙하 처리를 실행하지 않는다.
            }
            for (int x = 0; x < Block.BLOCK_NUM_X; x++)
            {
                // 열에 교체 중인 블록이 있다면 그 열은 처리하지 않고 다음 열로 진행.
                if (this.is_has_sliding_block_in_column(x))
                {
                    continue;
                }
                // 그 열에 있는 블록을 위에서부터 검사한다.
                for (int y = 0; y < Block.BLOCK_NUM_Y - 1; y++)
                {
                    // 지정 블록이 비표시라면 다음 블록으로.
                    if (!this.blocks[x, y].isVacant())
                    {
                        continue;
                    }
                    // 지정 블록 아래에 있는 블록을 검사.
                    for (int y1 = y + 1; y1 < Block.BLOCK_NUM_Y; y1++)
                    {
                        // 아래에 있는 블록이 비표시라면 다음 블록으로.
                        if (this.blocks[x, y1].isVacant())
                        {
                            continue;
                        }
                        // 블록을 교체한다.
                        this.fallBlock(this.blocks[x, y], Block.DIR4.UP,
                        this.blocks[x, y1]);
                        break;
                    }
                }
            }

            // 보충처리.
            for (int x = 0; x < Block.BLOCK_NUM_X; x++)
            {
                int fall_start_y = Block.BLOCK_NUM_Y;
                for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
                {
                    // 비표시 블록이 아니라면 다음 블록으로.
                    if (!this.blocks[x, y].isVacant())
                    {
                        continue;
                    }
                    this.blocks[x, y].beginRespawn(fall_start_y); // 블록 부활.
                    fall_start_y++;
                }
            }
        } while (false);
        this.is_vanishing_prev = is_vanishing;





       

        createQueueBlock();

        moveQueueBlock();


    }
    public void moveQueueBlock()
    {
        foreach (UnderBlock ub in underBlockQueue)
        {
             ub.instance.transform.position = new Vector3(ub.instance.transform.position.x+0.03f, ub.instance.transform.position.y, ub.instance.transform.position.z);
            
        }
    }

    public void createQueueBlock()
    {
        GameObject go;
        if(colorQueue.Count == 0)
        {
            return;
        }
        if (underBlockQueue.Count != 0)
        {
            
            if (underBlockQueue.ToArray()[underBlockQueue.Count - 1].instance.transform.position.x < 1)
            {
                return;
            }
        }
        
        
             go = Instantiate(queueBlockPrefeb) as GameObject;

          Color color_value;
        Block.COLOR color;
        switch (color = colorQueue.Dequeue())
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
            case Block.COLOR.BLACK:
                color_value = Color.black;
                break;
        }

        go.GetComponent<Renderer>().materials[0].color = color_value;

        underBlockQueue.Enqueue(new UnderBlock(color, go));


    }

    // 블록을 만들어 내고 가로 9칸, 세로 9칸에 배치한다.
    public void initialSetUp()
    {
        // 그리드의 크기  
        this.blocks =
        new BlockControl[Block.BLOCK_NUM_X, Block.BLOCK_NUM_Y];
        // 블록의 색 번호.
        int color_index = 0;
        Block.COLOR color = Block.COLOR.FIRST;

        for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
        { // 처음~마지막행
            for (int x = 0; x < Block.BLOCK_NUM_X; x++)
            { // 왼쪽~오른쪽
              // BlockPrefab의 인스턴스를 씬에 만든다.
                GameObject game_object =
                Instantiate(this.BlockPrefab) as GameObject;
                // 위에서 만든 블록의 BlockControl 클래스를 가져온다.
                BlockControl block =
                game_object.GetComponent<BlockControl>();
               // game_object.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
              
                // 블록을 그리드에 저장한다.
                this.blocks[x, y] = block;
                // 블록의 위치 정보(그리드 좌표)를 설정한다.
                block.i_pos.x = x;
                block.i_pos.y = y;
                // 각 BlockControl이 연계할 GameRoot는 자신이라고 설정한다.
                block.block_root = this;
                // 그리드 좌표를 실제 위치(씬의 좌표)로 변환한다.
                Vector3 position = BlockRoot.calcBlockPosition(block.i_pos);
                // 씬의 블록 위치를 이동한다.
                block.transform.position = position;
                // 블록의 색을 변경한다.
                //block.setColor((Block.COLOR)color_index);
                // 블록의 이름을 설정(후술)한다. 나중에 블록 정보 확인때 필요.
                color = this.selectBlockColor();
                block.setColor(color);

                block.name = "block(" + block.i_pos.x.ToString() +
                "," + block.i_pos.y.ToString() + ")";
                // 전체 색 중에서 임의로 하나의 색을 선택한다.
                color_index =
                Random.Range(0, (int)Block.COLOR.NORMAL_COLOR_NUM);
            }
        }




    }
    // BlockRoot.cs: BlockRoot class
    // 지정된 그리드 좌표로 씬에서의 좌표를 구한다.
    public static Vector3 calcBlockPosition(Block.iPosition i_pos)
    {
        // 배치할 왼쪽 위 구석 위치를 초기값으로 설정한다.
        Vector3 position = new Vector3(-(Block.BLOCK_NUM_X / 2.0f - 0.5f), -(Block.BLOCK_NUM_Y / 2.0f - 0.5f), 0.0f);
        // 초깃값 + 그리드 좌표 × 블록 크기.
        position.x += (float)i_pos.x * Block.COLLISION_SIZE;
        position.y += (float)i_pos.y * Block.COLLISION_SIZE;
        return (position); // 씬에서의 좌표를 반환한다.
    }
    public bool unprojectMousePosition(
out Vector3 world_position, Vector3 mouse_position)
    // ref는 초기화된 변수만, out은 초기화되지 않은 변수를 전달 가능
    {
        bool ret;
        // 판을 작성한다. 이 판은 카메라에 대해서 뒤로 향해서(Vector3.back).
        // 블록의 절반 크기만큼 앞에 둔다.
        Plane plane = new Plane(Vector3.back, new Vector3(
        0.0f, 0.0f, -Block.COLLISION_SIZE / 2.0f));
        // 카메라와 마우스를 통과하는 빛을 만든다.
        Ray ray = this.main_camera.GetComponent<Camera>().ScreenPointToRay(
        mouse_position);
        float depth;
        // 광선(ray)이 판(plane)에 닿았다면,
        if (plane.Raycast(ray, out depth))
        { // depth에 정보가 기록된다.
          // 인수 world_position을 마우스 위치로 덮어쓴다.
            world_position = ray.origin + ray.direction * depth;
            ret = true;
            // 닿지 않았다면.
        }
        else
        {
            // 인수 world_position을 0인 벡터로 덮어쓴다.
            world_position = Vector3.zero;
            ret = false;
        }
        return (ret); // 카메라를 통과하는 광선이 블록에 닿았는지를 반환
    }

    public BlockControl getNextBlock(BlockControl block, Block.DIR4 dir)
    {
        BlockControl next_block = null; // 슬라이드할 곳의 블록을 여기에 저장.
        switch (dir)
        {
            case Block.DIR4.RIGHT:
                if (block.i_pos.x < Block.BLOCK_NUM_X - 1)
                { // 그리드 안이라면.
                    next_block = this.blocks[block.i_pos.x + 1, block.i_pos.y];
                }
                break;
            case Block.DIR4.LEFT:
                if (block.i_pos.x > 0)
                { // 그리드 안이라면.
                    next_block = this.blocks[block.i_pos.x - 1, block.i_pos.y];
                }
                break;
            case Block.DIR4.UP:
                if (block.i_pos.y < Block.BLOCK_NUM_Y - 1)
                { // 그리드 안이라면.
                    next_block = this.blocks[block.i_pos.x, block.i_pos.y + 1];
                }
                break;
            case Block.DIR4.DOWN:
                if (block.i_pos.y > 0)
                { // 그리드 안이라면.
                    next_block = this.blocks[block.i_pos.x, block.i_pos.y - 1];
                }
                break;
        }
        return (next_block);
    }

    public static Vector3 getDirVector(Block.DIR4 dir)
    {
        Vector3 v = Vector3.zero;
        switch (dir)
        {
            case Block.DIR4.RIGHT: v = Vector3.right; break; // 오른쪽으로 1단위 이동.
            case Block.DIR4.LEFT: v = Vector3.left; break; // 왼쪽으로 1단위 이동.
            case Block.DIR4.UP: v = Vector3.up; break; // 위로 1단위 이동.
            case Block.DIR4.DOWN: v = Vector3.down; break; // 아래로 1단위 이동.
        }
        v *= Block.COLLISION_SIZE; // 블록의 크기를 곱한다.
        return (v);
    }
    // 인수로 지정된 방향의 반대 방향을 반환한다.
    public static Block.DIR4 getOppositDir(Block.DIR4 dir)
    {
        Block.DIR4 opposit = dir;
        switch (dir)
        {
            case Block.DIR4.RIGHT: opposit = Block.DIR4.LEFT; break;
            case Block.DIR4.LEFT: opposit = Block.DIR4.RIGHT; break;
            case Block.DIR4.UP: opposit = Block.DIR4.DOWN; break;
            case Block.DIR4.DOWN: opposit = Block.DIR4.UP; break;
        }
        return (opposit);
    }

    public void swapBlock(BlockControl block0, Block.DIR4 dir, BlockControl block1)
    {
        // 각각의 블록 색을 기억해 둔다.
        Block.COLOR color0 = block0.color;
        Block.COLOR color1 = block1.color;
        // 각각의 블록의 확대율을 기억해 둔다.
        Vector3 scale0 = block0.transform.localScale;
        Vector3 scale1 = block1.transform.localScale;
        // 각각의 블록의 '사라지는 시간'을 기억해 둔다.
        float vanish_timer0 = block0.vanish_timer;
        float vanish_timer1 = block1.vanish_timer;
        // 각각의 블록의 이동할 곳을 구한다.
        Vector3 offset0 = BlockRoot.getDirVector(dir);
        Vector3 offset1 = BlockRoot.getDirVector(BlockRoot.getOppositDir(dir));
        // 색을 교체한다.
        block0.setColor(color1);
        block1.setColor(color0);
        // 확대율을 교체한다.
        block0.transform.localScale = scale1;
        block1.transform.localScale = scale0;
        // '사라지는 시간'을 교체한다.
        block0.vanish_timer = vanish_timer1;
        block1.vanish_timer = vanish_timer0;
        block0.beginSlide(offset0); // 원래 블록 이동을 시작한다.
        block1.beginSlide(offset1); // 이동할 위치의 블록 이동을 시작한다.
    }

    public bool checkConnection(BlockControl start)
    {
        bool ret = false;
        int normal_block_num = 0;
        // 인수인 블록이 불붙은 다음이 아니면.
        if (!start.isVanishing())
        {
            normal_block_num = 1;
        }
        // 그리드 좌표를 기억해 둔다.
        int rx = start.i_pos.x;
        int lx = start.i_pos.x;
        // 블록의 왼쪽을 검사.
        for (int x = lx - 1; x > 0; x--)
        {
            BlockControl next_block = this.blocks[x, start.i_pos.y];
            if (next_block.color != start.color)
            { // 색이 다르면.
                break; // 루프를 빠져나간다.
            }
            if (next_block.step == Block.STEP.FALL || // 낙하 중이면.
            next_block.next_step == Block.STEP.FALL)
            {
                break; // 루프를 빠져나간다.
            }
            if (next_block.step == Block.STEP.SLIDE || // 슬라이드 중이면.
            next_block.next_step == Block.STEP.SLIDE)
            {
                break; // 루프를 빠져나간다.
            }
            if (!next_block.isVanishing())
            { // 불붙은 상태가 아니면.
                normal_block_num++; // 검사용 카운터를 증가.
            }
            lx = x;
        }
        // 블록의 오른쪽을 검사.
        for (int x = rx + 1; x < Block.BLOCK_NUM_X; x++)
        {
            BlockControl next_block = this.blocks[x, start.i_pos.y];
            if (next_block.color != start.color)
            {
                break;
            }
            if (next_block.step == Block.STEP.FALL ||
            next_block.next_step == Block.STEP.FALL)
            {
                break;
            }
            if (next_block.step == Block.STEP.SLIDE ||
            next_block.next_step == Block.STEP.SLIDE)
            {
                break;
            }
            if (!next_block.isVanishing())
            {
                normal_block_num++;
            }
            rx = x;
        }
        do
        {
            // 오른쪽 블록의 그리드 번호 - 왼쪽 블록의 그리드 번호 +.
            // 중앙 블록(1)을 더한 수가 3 미만이면.
            if (rx - lx + 1 < 3)
            {
                break; // 루프 탈출.
            }
            if (normal_block_num == 0)
            {// 불붙지 않은 블록이 하나도 없으면.
                break; // 루프 탈출.
            }
            for (int x = lx; x < rx + 1; x++)
            {
                // 나열된 같은 색 블록을 불붙은 상태로.
                this.blocks[x, start.i_pos.y].toVanishing();
                
                    colorQueue.Enqueue(this.blocks[x, start.i_pos.y].color);
                
                
                ret = true;
            }
        } while (false);
        normal_block_num = 0;
        if (!start.isVanishing())
        {
            normal_block_num = 1;
        }
        int uy = start.i_pos.y;
        int dy = start.i_pos.y;
        // 블록의 위쪽을 검사.
        for (int y = dy - 1; y > 0; y--)
        {
            BlockControl next_block = this.blocks[start.i_pos.x, y];
            if (next_block.color != start.color)
            {
                break;
            }
            if (next_block.step == Block.STEP.FALL ||
            next_block.next_step == Block.STEP.FALL)
            {
                break;
            }
            if (next_block.step == Block.STEP.SLIDE ||
            next_block.next_step == Block.STEP.SLIDE)
            {
                break;
            }
            if (!next_block.isVanishing())
            {
                normal_block_num++;
            }
            dy = y;
        }
       
// 블록의 아래쪽을 검사.
for (int y = uy + 1; y < Block.BLOCK_NUM_Y; y ++)
        {
            BlockControl next_block = this.blocks[start.i_pos.x, y];
            if (next_block.color != start.color)
            {
                break;
            }
            if (next_block.step == Block.STEP.FALL ||
            next_block.next_step == Block.STEP.FALL)
            {
                break;
            }
            if (next_block.step == Block.STEP.SLIDE ||
            next_block.next_step == Block.STEP.SLIDE)
            {
                break;
            }
            if (!next_block.isVanishing())
            {
                normal_block_num++;
            }
            uy = y;
        }
        do
        {
            if (uy - dy + 1 < 3)
            {
                break;
            }
            if (normal_block_num == 0)
            {
                break;
            }
            for (int y = dy; y < uy + 1; y++)
            {
                this.blocks[start.i_pos.x, y].toVanishing();

                
                    colorQueue.Enqueue(this.blocks[start.i_pos.x, y].color);
                

                //blockQueue.Enqueue(this.blocks[start.i_pos.x,y]);
                ret = true;
            }
        } while (false);

        if (start.vanish_timer<0)
     {

        

        if ((start.i_pos.y+2 > Block.BLOCK_NUM_Y)|| (start.i_pos.y - 1 < 0))
        {
            return (ret);
        }
        if ((start.i_pos.x + 2 > Block.BLOCK_NUM_X ) || (start.i_pos.x - 1 < 0))
        {
            return (ret);
        }
       
        BlockControl leftTop = this.blocks[start.i_pos.x-1, start.i_pos.y-1];

        BlockControl rightBottom = this.blocks[start.i_pos.x +1, start.i_pos.y +1];

        if ((leftTop.color == start.color) && (start.color == rightBottom.color))
        {
            leftTop.toVanishing();
                
                    colorQueue.Enqueue(leftTop.color);
                
                //blockQueue.Enqueue(leftTop);
                start.toVanishing();
                
                    colorQueue.Enqueue(start.color);
                
                //blockQueue.Enqueue(start);
                rightBottom.toVanishing();
                
                    colorQueue.Enqueue(rightBottom.color);
                
                //blockQueue.Enqueue(rightBottom);
            }
     }

        if (start.vanish_timer < 0)
        {



            if ((start.i_pos.y + 2 > Block.BLOCK_NUM_Y) || (start.i_pos.y - 1 < 0))
            {
                return (ret);
            }
            if ((start.i_pos.x + 2 > Block.BLOCK_NUM_X) || (start.i_pos.x - 1 < 0))
            {
                return (ret);
            }

            BlockControl leftBottom = this.blocks[start.i_pos.x - 1, start.i_pos.y + 1];

            BlockControl rightTop = this.blocks[start.i_pos.x + 1, start.i_pos.y - 1];

            if ((leftBottom.color == start.color) && (start.color == rightTop.color))
            {
                leftBottom.toVanishing();
                
                    colorQueue.Enqueue(leftBottom.color);
                
                //blockQueue.Enqueue(leftBottom);
                start.toVanishing();

                colorQueue.Enqueue(start.color);
                
                // blockQueue.Enqueue(start);
                rightTop.toVanishing();

                colorQueue.Enqueue(rightTop.color);
                
                // blockQueue.Enqueue(rightTop);
            }
        }

        return (ret);
    }


    private bool is_has_vanishing_block()
    {
        bool ret = false;
        foreach(BlockControl block in this.blocks) {
            if(block.vanish_timer > 0.0f) {
                ret = true;
                break;
            }
        }
        return(ret);
    }
    // 슬라이드 중인 블록이 하나라도 있으면 true를 반환한다.
    private bool is_has_sliding_block()
    {
        bool ret = false;
        foreach (BlockControl block in this.blocks)
        {
            if(block.step == Block.STEP.SLIDE) {
                ret = true;
                break;
            }
        }
        return (ret);
    }
    // 낙하 중인 블록이 하나라도 있으면 true를 반환한다.
    private bool is_has_falling_block()
    {
        bool ret = false;
        foreach (BlockControl block in this.blocks)
        {
            if(block.step == Block.STEP.FALL) {
                ret = true;
                break;
            }
        }
        return (ret);
    }

    public void fallBlock(BlockControl block0, Block.DIR4 dir, BlockControl block1)
    {
        // block0과 block1의 색, 크기, 사라질 때까지 걸리는 시간, 표시, 비표시, 상태
        // 를 기록.
        Block.COLOR color0 = block0.color;
        Block.COLOR color1 = block1.color;
        Vector3 scale0 = block0.transform.localScale;
        Vector3 scale1 = block1.transform.localScale;
        float vanish_timer0 = block0.vanish_timer;
        float vanish_timer1 = block1.vanish_timer;
        bool visible0 = block0.isVisible();
        bool visible1 = block1.isVisible();
        Block.STEP step0 = block0.step;
        Block.STEP step1 = block1.step;
        // block0과 block1의 각종 속성을 교체한다.
        block0.setColor(color1);
        block1.setColor(color0);
        block0.transform.localScale = scale1;
        block1.transform.localScale = scale0;
        block0.vanish_timer = vanish_timer1;
        block1.vanish_timer = vanish_timer0;
        block0.setVisible(visible1);
        block1.setVisible(visible0);
        block0.step = step1;
        block1.step = step0;
        block0.beginFall(block1);
    }
    // 지정된 그리드 좌표의 열(세로 줄)에 슬라이드 중인 블록이 하나라도 있으면
    // true를 반환한다.
    private bool is_has_sliding_block_in_column(int x)
    {
        bool ret = false;
        for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
        {
            // 슬라이드 중인 블록이 있으면,
            if (this.blocks[x, y].isSliding())
            {
                ret = true; // true를 반환한다.
                break;
            }
        }
        return (ret);
    }
}

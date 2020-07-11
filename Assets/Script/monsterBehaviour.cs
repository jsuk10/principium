﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHaviour : MonoBehaviour
{
    [SerializeField] public int dmg;
    [Tooltip("1은 바닥체크해서 턴하는 방법, 2는 웨이포인트 이용")]
    [SerializeField] int type;
    [SerializeField] int hp;
    [SerializeField] int jumpDelay;

    [SerializeField] float speed = 1;
    [SerializeField] float jumpPower = 5f;
    [SerializeField] float direction = 1;

    [SerializeField] bool isJumpRay;
    [SerializeField] bool isTurnRay;
    [SerializeField] bool lookAtPlayer;
    [SerializeField] bool trakingPlayer;
    [SerializeField] protected bool mobMove = true;

    [SerializeField] Rigidbody2D mobRB;
    [SerializeField] Transform mobTR;
    [Tooltip("0이 왼쪽 1이 오른쪽, 몬스터는 0부터 시작함")]
    [SerializeField] GameObject[] wayPoint = new GameObject[2];
    [SerializeField] GameObject target;
    int moveDirection = 1;
    bool directionCanChange = true;
    int layerMaskO = 1 << 8;
    int goToWay = 0;
    Vector3 targetPosition;

    // Start is called before the first frame update
    void Awake()
    {
        LookForward();
    }

    // 상속을 위한 클래스 이므로 아래에 전부 오버라이딩 해줘야함.
    private void Update()
    {
        if (mobMove == true)
        {
            Patten();
        }
        if (lookAtPlayer == true) {
            LookTarget();
        }
    }

    //virtual은 상속을 위한 함수로 새로운 스크립트에서 오버라이딩 해줘야함.
    public virtual void Skill() {

    }

    public void LookTarget()
    {
        if (target.transform.position.x >= transform.position.x)
            mobTR.rotation = Quaternion.Euler(0, 180, 0);
        else
            mobTR.rotation = Quaternion.Euler(0, 0, 0);
    }



    // 이동 모션을 의미함
    public void Patten()
    {
        switch (type)
        {
            //바닥을 만나면 턴 하는 패턴
            case 1:
                Move();
                if (isTurnRay == true)
                    TurnRay();
                if (isJumpRay == true)
                    JunpRay();
                break;

            //웨이포인트 따라가는 패턴
            case 2:
                MoveToWayPoint();
                break;
        }
    }


    //<움직임에 관한 함수>

    //wayPoint를 왕복하는 함수.
    void MoveToWayPoint() {
        Vector3 dirctoinV = new Vector3(direction, 0, 0);
        transform.Translate(dirctoinV * speed * Time.smoothDeltaTime, Space.World);
        //방향 바꿔주는 함수
        if ((wayPoint[0].transform.position.x >= transform.position.x  || transform.position.x >= wayPoint[1].transform.position.x))
        {
            Turn();
        }

    }

    //움직이는 함수임 velocity(Rb의 속도)를 이용하여 방향을 바꿔줌. 단순히 이동만 생각하여서 MoveDirection을 직접적으로 변경 하지 않음. 
    //rotation을 이용해 몹이 보는 방향을 변경해줌.
    void Move()
    {
        if (direction == 1)
        {
            mobRB.velocity = new Vector2(speed, mobRB.velocity.y);
        }

        if (direction == -1)
        {
            mobRB.velocity = new Vector2(-speed, mobRB.velocity.y);
        }

    }

    //MoveDirection을 바꿔주는 함수.
    void Turn()
    {
        direction *= -1;

        if (lookAtPlayer == false)
        {
            LookForward();
        }
    }

    void LookForward() {
        if (direction == 1)
            mobTR.rotation = Quaternion.Euler(0, 180, 0);
        if (direction == -1)
            mobTR.rotation = Quaternion.Euler(0, 0, 0);
    }

    //점프하는 함수이며 장애물의 크기에 따라 점프 파워 혹은 뒤에 붙은 상수를 변경 하면 됨.
    void Jump()
    {
        mobRB.AddForce(Vector2.up * jumpPower * 5);
        isJumpRay = false;
        StartCoroutine(stopJump());
    }


    //<콜라이더 충돌 체크>

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //player hp감소 혹은 죽음 넣기
            lookAtPlayer = true;
            LookTarget();
            Debug.Log("플레이어 인식함");
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //player hp감소 혹은 죽음 넣기
            lookAtPlayer = false;
            LookForward();
            Debug.Log("플레이어 나감");
        }
    }

    //<레이 케스트 함수들>
    void TurnRay()
    {
        //frontBec는 내가 앞에 쏴줄 레이어 즉 충돌 판정할곳 정해줌 
        Vector2 frontBec = new Vector2(mobRB.position.x + direction * mobTR.localScale.x, mobRB.position.y - 0.5f);

        //Debug.DrawRay(x,y,z,r)x는 기준점, y는 방향 z는 색,r은 유지시간
        Debug.DrawRay(frontBec, Vector2.down, Color.red, 1);

        //Physics2D.Raycast(x,y,r) 변수는 위와 동일
        RaycastHit2D rayHit = Physics2D.Raycast(frontBec, Vector3.down, 1, layerMaskO);

        if (rayHit.collider == null)
        {
            Turn();
            Debug.Log("turn");
        }
    }

    void JunpRay()
    {
        //frontBec는 내가 앞에 쏴줄 레이어 즉 충돌 판정할곳 정해줌 
        Vector2 front = new Vector2(mobRB.position.x + direction * mobTR.localScale.x, mobRB.position.y + 1f);

        //Debug.DrawRay(x,y,z,r)x는 기준점, y는 방향 z는 색,r은 유지시간
        Debug.DrawRay(front, Vector2.down, Color.red, 1);

        //Physics2D.Raycast(x,y,r) 변수는 위와 동일
        RaycastHit2D rayHit = Physics2D.Raycast(front, Vector2.down, 1, layerMaskO);

        if (rayHit.collider != null)
        {
            Jump();
            Debug.Log("jump");
        }
    }


    //코루틴을 이용해 1초의 대기 시간을 준뒤 점프를 풀어주었음. 이를 통해 연속 점프하는 현상이 해결됨.
    IEnumerator stopJump()
    {
        yield return new WaitForSeconds(1f);
        isJumpRay = true;
    }
}

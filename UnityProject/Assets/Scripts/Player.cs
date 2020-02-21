using UnityEngine;
using System.Linq;      // 引用 查詢語法 API

public class Player : MonoBehaviour
{
    [Header("玩家資料")]
    public PlayerData data;
    [Header("武器")]
    public GameObject bullet;

    private Joystick joystick;              // 虛擬搖桿類別
    private Rigidbody rig;                  // 剛體
    private Animator ani;                   // 動畫控制器
    private Transform target;               // 目標
    private LevelManager levelManager;      // 關卡管理器
    private HpValueManager hpDamManager;    // 血量數值管理器
    private float timer;                    // 計時器
    private Enemy[] enemys;                 // 儲存所有敵人陣列
    private float[] enemysDistance;         // 儲存所有敵人距離陣列

    private void Start()
    {
        rig = GetComponent<Rigidbody>();                                    // 取得元件<泛型>() (取得相同屬性面板)
        ani = GetComponent<Animator>();
        joystick = GameObject.Find("虛擬搖桿").GetComponent<Joystick>();    // 遊戲物件.尋找("物件名稱").取得元件<泛型>()

        // target = GameObject.Find("目標").GetComponent<Transform>();       // 原本寫法
        target = GameObject.Find("目標").transform;                          // 簡寫 - GetComponent<Transform>() 可以直接寫成 transform
        //levelManager = GameObject.Find("遊戲管理器").GetComponent<LevelManager>();
        levelManager = FindObjectOfType<LevelManager>();                    // 僅限於一個
        hpDamManager = GetComponentInChildren<HpValueManager>();            // 取得子物件的元件 (僅限於子物件內只有一個)
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "傳送區域")
        {
            //levelManager.NextLevel();
            levelManager.StartCoroutine("NextLevel");
        }
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        float v = joystick.Vertical;                // 垂直
        float h = joystick.Horizontal;              // 水平

        rig.AddForce(-h * data.speed, 0, -v * data.speed);    // 推力(-水平，0，-垂直)

        ani.SetBool("跑步開關", v != 0 || h != 0);  // 動畫控制器.設定布林值("參數"，垂直不等於 0 或者 水平不等於 0)

        Vector3 posPlayer = transform.position;                                         // 玩家座標 = 玩家.座標
        Vector3 posTarget = new Vector3(posPlayer.x - h, 0.26f, posPlayer.z - v);       // 目標座標 = 新 三維向量(玩家座標.X - 水平，0.26f，玩家座標.Z - 垂直)
        target.position = posTarget;    // 目標物件.座標 = 計算後的座標
        posTarget.y = posPlayer.y;      // 目標 Y = 玩家 Y (避免吃土)
        transform.LookAt(posTarget);    // 看著(目標座標)

        if (v == 0 && h == 0) Attack(); // 如果 垂直 與 水平 都是 0 就進入攻擊狀態
    }

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="damage">接收傷害值</param>
    public void Hit(float damage)
    {
        data.hp -= damage;
        hpDamManager.UpdateHpBar(data.hp, data.hpMax);

        // 啟動協程(血量傷害值管理器.顯示數值(傷害值，-，1，白色))
        StartCoroutine(hpDamManager.ShowValue(damage, "-", Vector3.one, Color.white));

        if (data.hp <= 0) Dead();
    }

    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        ani.SetBool("死亡開關", true);                   // 死亡動畫
        enabled = false;                                // 取消此類別運作

        StartCoroutine(levelManager.ShowRevival());     // 啟動協程(關卡管理器.顯示復活介面())
    }

    /// <summary>
    /// 復活
    /// </summary>
    public void Revival()
    {
        enabled = true;
        ani.SetBool("死亡開關", false);
        data.hp = data.hpMax;
        hpDamManager.UpdateHpBar(data.hp, data.hpMax);
        levelManager.CloseRevival();
    }

    /// <summary>
    /// 攻擊
    /// </summary>
    private void Attack()
    {
        if (timer < data.cd)
        {
            timer += Time.deltaTime;
        }
        else
        {
            // 1. 搜尋所有的敵人
            enemys = FindObjectsOfType<Enemy>();        // 搜尋所有敵人
            enemysDistance = new float[enemys.Length];  // 指定敵人距離陣列長度

            if (enemys.Length == 0)
            {
                levelManager.Pass();    // 關卡管理器.過關
                return;                 // 如果 怪物數量為零 就跳出
            }

            timer = 0;                                  // 計時器歸零
            ani.SetTrigger("攻擊觸發");                  // 攻擊動畫

            // 2. 檢查誰的距離最近
            for (int i = 0; i < enemys.Length; i++)
            {
                enemysDistance[i] = Vector3.Distance(transform.position, enemys[i].transform.position);
            }

            float min = enemysDistance.Min();  // 取得最小值
            
            // 一般陣列無法使用，轉為清單 List (集合)
            int index = enemysDistance.ToList().IndexOf(min);   // 取得最小值的編號

            // 3. 面向最近的敵人
            Vector3 posEnemy = enemys[index].transform.position;
            posEnemy.y = transform.position.y;
            transform.LookAt(posEnemy);

            // 發射子彈
            // 座標 = 本身座標 + 上方 * 單位 + 前方 * 單位
            Vector3 pos = transform.position + transform.up * 1 + transform.forward * 1.5f;
            // 角度 = 四元.歐拉(本身.X，本身.Y + 單位，本身.Z + 單位)
            Quaternion qua = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 180, transform.eulerAngles.z);
            // 生成(物件，座標，角度)
            GameObject temp = Instantiate(bullet, pos, qua);
            temp.GetComponent<Rigidbody>().AddForce(transform.forward * data.power);
            temp.AddComponent<Bullet>();                            // 添加元件<泛型>
            temp.GetComponent<Bullet>().damage = data.attack;       // 設定子彈傷害 = 資料.攻擊力
            temp.GetComponent<Bullet>().playerBullet = true;        // 設定子彈
        }
    }
}

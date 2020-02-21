using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("玩家資料")]
    public PlayerData data;

    public void BuyHp1000()
    {
        data.hpMax += 1000;
        data.hp = data.hpMax;
    }

    public void BuyCD0_01()
    {
        data.cd -= 0.01f;
    }

    public void BuyAttack100()
    {
        data.attack += 100;
    }

    public void BuySpeed1()
    {
        data.speed += 1;
    }

    public void StartGame()
    {
        data.hp = data.hpMax;
        SceneManager.LoadScene(1);
    }
}

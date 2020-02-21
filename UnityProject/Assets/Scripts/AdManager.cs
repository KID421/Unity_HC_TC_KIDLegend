using UnityEngine;
using UnityEngine.Advertisements;   // 引用廣告 API

// 繼承 (僅能繼承一個)
// 介面 (可以無限多個)：像裝備，裝上之後就擁有裝備的功能
public class AdManager : MonoBehaviour, IUnityAdsListener
{
    private string googlePlay = "3457913";          // 廣告 ID Google Play
    private string placementRevival = "revival";    // 廣告名稱 復活
    private Player player;

    private void Start()
    {
        Advertisement.Initialize(googlePlay, false);     // 廣告.初始化(廣告服務 ID，啟動測試模式)
        Advertisement.AddListener(this);                // 廣告.增加監聽者(此腳本)
        player = FindObjectOfType<Player>();
    }

    /// <summary>
    /// 顯示復活廣告
    /// </summary>
    public void ShowRevivalAd()
    {
        if (Advertisement.IsReady(placementRevival))    // 如果 廣告準備完成 (廣告名稱)
        {
            Advertisement.Show(placementRevival);       // 顯示廣告 (廣告名稱)
        }
    }

    public void OnUnityAdsReady(string placementId)
    {
    }

    public void OnUnityAdsDidError(string message)
    {
    }

    public void OnUnityAdsDidStart(string placementId)
    {
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (placementId == placementRevival)
        {
            switch (showResult)                 // switch 判斷式 (要判斷的值)
            {
                case ShowResult.Failed:         // 第一種可能 case 廣告失敗
                    print("廣告失敗");
                    break;
                case ShowResult.Skipped:        // 第二種可能 case 廣告略過
                    print("廣告略過");
                    break;
                case ShowResult.Finished:       // 第三種可能 case 廣告完成
                    print("廣告完成");
                    GameObject.Find("玩家").GetComponent<Player>().Revival();
                    break;
            }
        }
    }
}

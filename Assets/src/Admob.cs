using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using GoogleMobileAds.Api;
using UnityEngine;

public class Admob : MonoBehaviour
{
    public static Admob Instance;
    private string APPID = "ca-app-pub-2067638816975525~6627038865";
    private string BUNNER = "ca-app-pub-2067638816975525/6533392299";
    private string VIDEO = "ca-app-pub-2067638816975525/5930757063";



    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            StartCoroutine(AStart());
        } 
    }

    public IEnumerator AStart()
    {
        yield return new WaitForSeconds(1);

       // Debug.LogError("InAPP.isRemoveAds = " + InAPP.isRemoveAds);
        Instance = this;

       // if (!InAPP.isRemoveAds)
        {
            // Initialize the Google Mobile Ads SDK.
            yield return new WaitForSeconds(0.1f);
            MobileAds.Initialize(APPID);
            yield return new WaitForSeconds(0.1f);
            // for (int i = 0; i < 1000; i++)
            {
                RequestBanner();
            }
            yield return new WaitForSeconds(0.1f);

            this.rewardBasedVideo = RewardBasedVideoAd.Instance;
           //   for (int i = 0; i < 1000; i++)
            {
                RequestRewardBasedVideo();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private BannerView bannerView;
    private RewardBasedVideoAd rewardBasedVideo;

   



    public void RequestBanner()
    {
        return;
      //   if (InAPP.isRemoveAds)
      //       return;

        
        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(BUNNER, AdSize.Banner, AdPosition.Bottom);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        
        // Load the banner with the request.
        bannerView.LoadAd(request);
       
        bannerView.Show();
    }

    public void HideBunner()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
        }
    }

    public void ShowBunner()
    {
        if (bannerView != null)
        {
            bannerView.Show();
        }
    }

    public void RequestRewardBasedVideo()
    {
      //  if (InAPP.isRemoveAds)
      //      return;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.rewardBasedVideo.LoadAd(request, VIDEO);
    }


    public void RemoveAds()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
            
        }
        else
        {

        }
    }


   
}


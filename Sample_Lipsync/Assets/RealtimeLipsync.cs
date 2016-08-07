using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RealtimeLipsync : MonoBehaviour {
    const int checkVoiceNumberMax = 1400;

    Animator anim;
    List<AudioClip> audioClip1st = new List<AudioClip>();//!< Check present & loading.
    AudioClip[] audioClip;//!< Voice clip list.
    AudioSource audioSource;

    void PreLoadVoice()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        GameObject.DontDestroyOnLoad(this.gameObject);

        for (int i=0; i< checkVoiceNumberMax; i++) {
            String s = "Voice/univ" + i.ToString().PadLeft(4, '0');
            AudioClip ac = Resources.Load<AudioClip>(s);
            if(ac!=null) {
                audioClip1st.Add(ac);
                Debug.Log("loaded: " + s);
            }
            else {
                Debug.Log("load fail: " + s);
            }
        }
        audioClip = audioClip1st.ToArray();
        
    }


    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();
        anim.SetLayerWeight(1, 1);//Layer index(0-n), Layerweight(0-1.0)


        /*
        audioClip = Resources.Load<AudioClip>(SNDNAME);
        audioSource = gameObject.AddComponent<AudioSource>();
        GameObject.DontDestroyOnLoad(this.gameObject);

        audioSource.clip = audioClip; //音色(wav)をチャンネルに紐付け
        audioSource.volume = 1.0f;  //ボリューム設定。0～1.0範囲
        audioSource.Play(); //発声
        */
        PreLoadVoice();


    }

    int modeState = 0;//!<遷移モード
    float t;//!<Laptime計測
    float clipperIntervaltime;
    int playVoiceNumber = 0;//!<現在再生している読み込み済みボイスの番号
    float[] voiceSampleData = new float[2];//!<再生中ボイスの音量サンプル取得用バッファ
    float playVoiceLaptime;  //!<再生中ボイスの長さ[sec]
    void Update () {
        t += Time.deltaTime;

        if(modeState==0) {//Start to play voice
            //Debug.Log("modeState: " + modeState);
            audioSource.clip = audioClip[playVoiceNumber]; //音色(wav)をチャンネルに紐付け
            playVoiceLaptime = audioClip[playVoiceNumber].length;
            audioSource.volume = 1.0f;  //ボリューム設定。0～1.0範囲

            audioSource.Play(); //発声
            modeState++;
            t = 0;
            clipperIntervaltime = 999f;
        }

        if (modeState == 1) {
            //const float CLIPER_TIME = 0.1f; //Normal
            //const float CLIPER_TIME = 0.05f;//little quick
            const float CLIPER_TIME = 0.1f;
            //Debug.Log("modeState: " + modeState);
            clipperIntervaltime += Time.deltaTime;
            if (t > playVoiceLaptime) {
                anim.CrossFade("default@sd_hmd", CLIPER_TIME);//Clipper終了。
                modeState = 2;
                t = 0;
                Debug.Log("*** default@sd_hmd");
            }
            if (clipperIntervaltime> CLIPER_TIME) {
                clipperIntervaltime = 0f;
                audioSource.GetOutputData(voiceSampleData, 1);
                if(voiceSampleData[0]>0.005f) {
                    anim.CrossFade("mth_a@sd_hmd", CLIPER_TIME);//CrossFade時間がClipper終了の口閉じアニメより長いと、こちらのアニメが残るので注意。 
                }
                else {
                    anim.CrossFade("default@sd_hmd", CLIPER_TIME);
                }
                Debug.Log("voiceSampleData: " + voiceSampleData[0] + ", " + voiceSampleData[1]);
            }
        }
        if (modeState == 2)
        {
            //anim.CrossFade("default@sd_hmd", 0.1f);//Clipper終了。

            //Debug.Log("modeState: " + modeState);
            if (t > 2.0f) {
                modeState = 0;
                if (++playVoiceNumber > audioClip.Length) playVoiceNumber = 0;
            }
        }
    }
}

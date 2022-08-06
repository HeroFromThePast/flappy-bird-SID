using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HttpManager : MonoBehaviour
{
    [SerializeField] Text leaderboard;
    [SerializeField]
    private string URL;

    private void Start()
    {
        leaderboard.enabled = false;
    }

    public void ClickGetScores()
    {
        StartCoroutine(GetScores());
    }

    IEnumerator GetScores()
    {
        string url = URL + "/leaders";
        UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www.SendWebRequest();

        leaderboard.enabled = true;

        leaderboard.text += "Leaderboard: \n";

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR " + www.error);
        }
        else if (www.responseCode == 200)
        {
            //Debug.Log(www.downloadHandler.text);
            Scores resData = JsonUtility.FromJson<Scores>(www.downloadHandler.text);

            foreach (ScoreData score in resData.scores)
            {
                Debug.Log(score.userId + " | " + score.value);
               
            }

           
            for (int i = 0; i < resData.scores.Length ; i++)
            {
                leaderboard.text +=  resData.scores[i].userId + " | " + resData.scores[i].value  +"\n ";
            }
        }
        else
        {
            Debug.Log(www.error);
        }
    }

}


[System.Serializable]
public class ScoreData
{
    public string userId;
    public int value;

}

[System.Serializable]
public class Scores
{
    public ScoreData[] scores;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HttpManager : MonoBehaviour
{
    [SerializeField] Text leaderboard;
    [SerializeField]
    private string URL;
    private string Token;
    private string Username;
    private int HighScore;

    // Start is called before the first frame update

    void Start()
    {
        if (Token != null)
        {
            StartCoroutine(GetPerfil());
        }
        leaderboard.enabled = false;
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");
        HighScore = PlayerPrefs.GetInt("highScore");

        Debug.Log("token:" + Token);

        
    }

    public void ClickGetScores()
    {
        StartCoroutine(GetScores());
    }

    public void ClickLogIn()
    {

        AuthData data = new AuthData();

        data.username = GameObject.Find("InputFieldUsername").GetComponent<InputField>().text;
        data.password = GameObject.Find("InputFieldPassword").GetComponent<InputField>().text;

        string postData = JsonUtility.ToJson(data);
        StartCoroutine(LogIn(postData));
    }

    public void ClickSignUp()
    {

        AuthData data = new AuthData();

        data.username = GameObject.Find("InputFieldUsername").GetComponent<InputField>().text;
        data.password = GameObject.Find("InputFieldPassword").GetComponent<InputField>().text;

        string postData = JsonUtility.ToJson(data);
        StartCoroutine(SignUp(postData));
    }

    IEnumerator GetScores()
    {
        string url = URL + "/api/usuarios?limit=5&sort=true";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-token", Token);
        www.SetRequestHeader("content-type", "application/json");

        yield return www.SendWebRequest();

        leaderboard.enabled = true;

        leaderboard.text += "Leaderboard: \n";

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR " + www.error);
        }
        else if (www.responseCode == 200)
        {
            string response = www.downloadHandler.text;
            
            Usuarios resData = JsonConvert.DeserializeObject<Usuarios>(response);
            //  Scores resData = JsonUtility.FromJson<Scores>(www.downloadHandler.text);

            Debug.Log(www.downloadHandler.text);
            foreach (UserData usuario in resData.usuarios)
            {
                Debug.Log(usuario.username + " | " + usuario.score);
            }

          
            /*
            foreach (UserData usuario in resData.puntajes)
            {
                Debug.Log( usuario.username + " | " + usuario.score);
            }
            */
            for (int i = 0; i < resData.usuarios.Count; i++)
            {
                leaderboard.text += resData.usuarios[i].username + " | " + resData.usuarios[i].score + " \n";
            }            
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    IEnumerator SignUp(string postData)
    {
        Debug.Log(postData);


        string url = URL + "/api/usuarios";
        UnityWebRequest www = UnityWebRequest.Put(url, postData);
        www.method = "POST";
        www.SetRequestHeader("content-type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR " + www.error);
            Debug.Log(www.downloadHandler.text);
        }
        else if (www.responseCode == 200)
        {
            //Debug.Log(www.downloadHandler.text);
            AuthData resData = JsonUtility.FromJson<AuthData>(www.downloadHandler.text);

            Debug.Log("Bienvenido " + resData.usuario.username + ", id:" + resData.usuario._id);

            StartCoroutine(LogIn(postData));
            
        }
        else
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }
    }

    IEnumerator LogIn(string postData)
    {
        Debug.Log(postData);


        string url = URL + "/api/auth/login";
        UnityWebRequest www = UnityWebRequest.Put(url, postData);
        www.method = "POST";
        www.SetRequestHeader("content-type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR " + www.error);
            Debug.Log(www.downloadHandler.text);
        }
        else if (www.responseCode == 200)
        {
            //Debug.Log(www.downloadHandler.text);
            AuthData resData = JsonUtility.FromJson<AuthData>(www.downloadHandler.text);
            Debug.Log("Autenticado");
            Debug.Log("token: " + resData.token);
            PlayerPrefs.SetString("token", resData.token);
            PlayerPrefs.SetString("username", resData.usuario.username);
            SceneManager.LoadScene("Game");

        }
        else
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }
    }
    IEnumerator GetPerfil()
    {
        string url = URL + "/api/usuarios" + Username;
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-token",  Token);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR " + www.error);
            Debug.Log(www.downloadHandler.text);
        }
        else if (www.responseCode == 200)
        {
            //Debug.Log(www.downloadHandler.text);
            AuthData resData = JsonUtility.FromJson<AuthData>(www.downloadHandler.text);

            Debug.Log("token valido " + resData.usuario.username + ", id:" + resData.usuario._id);
            SceneManager.LoadScene("Game");
        }
        else
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }
    }

    public void UpdateHighScore()
    {
        if (PlayerPrefs.GetInt("highScore") < Score.score)
        {
            ScoreData newScore = new ScoreData();
            newScore.score = Score.score;
            newScore.username = PlayerPrefs.GetString("username");
            string postData = JsonUtility.ToJson(newScore);
            PlayerPrefs.SetInt("highscore", Score.score);
            StartCoroutine(UpdateScore(postData));

        }
    }
    
    public IEnumerator UpdateScore(string postData)
    {
        Debug.Log("patch score");


        string url = URL + "/api/usuarios";
        UnityWebRequest www = UnityWebRequest.Put(url, postData);
        www.method = "PATCH";
        www.SetRequestHeader("content-type", "application/json");
        www.SetRequestHeader("x-token", Token);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR " + www.error);
            Debug.Log(www.downloadHandler.text);
        }
        else if (www.responseCode == 200)
        {
            Debug.Log(www.downloadHandler.text);
            UserData resData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);

           // Debug.Log("Puntaje: " + resData.username +" | " + resData.score); no funciona, no entiendo por qué

        }
        else
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }
    }
    
}


[System.Serializable]
public class ScoreData
{
    public int score;
    public string username;

}

[System.Serializable]
public class Scores
{
    public UserData[] scores;
}

[System.Serializable]
public class AuthData
{
    public string username;
    public string password;
    public UserData usuario;
    public string token;
}

[System.Serializable]

public class Usuarios
{
    public List<UserData> usuarios = new List<UserData>();
}
public class UserData
{
    public string _id;
    public string username;
    public bool estado;
    public int score;
}

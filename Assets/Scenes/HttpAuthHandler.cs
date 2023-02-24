using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using System.Linq;
using static User;
using TMPro;

public class HttpAuthHandler : MonoBehaviour
{

    [SerializeField]
    private string ServerApiURL;

    [SerializeField] Text leaderboard;

    public string Token { get; set; }
    public string Username { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");
        leaderboard.enabled = false;

        if (string.IsNullOrEmpty(Token))
        {
            Debug.Log("No hay token");
            Debug.Log("highscore: " + PlayerPrefs.GetInt("highscore"));
            //Ir a Login
        }
        else
        {
            Debug.Log(Token);
            Debug.Log(Username);
            StartCoroutine(GetPerfil());
            Debug.Log("highscore: " + PlayerPrefs.GetInt("highscore"));
        }
    }

    public void ClickGetScores()
    {
        StartCoroutine(GetScores());
    }

    IEnumerator GetScores()
    {
        string url = ServerApiURL + "/api/usuarios?limit=5&sort=true";
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

            UserList scoreList = JsonUtility.FromJson<UserList>(www.downloadHandler.text);
            List<User> sortedScoreList = scoreList.usuarios.OrderByDescending(u => u.data.score).ToList<User>();

            foreach (User item in sortedScoreList)
            {
                leaderboard.text += item.username + " | " + item.data.score + " \n";
            }



            /*
            foreach (UserData usuario in resData.usuarios)
            {
                Debug.Log(usuario.username + " | " + usuario.score);
            }
            
            foreach (UserData usuario in resData.puntajes)
            {
                Debug.Log( usuario.username + " | " + usuario.score);
            }
            
            for (int i = 0; i < resData.usuarios.Count; i++)
            {
                leaderboard.text += resData.usuarios[i].username + " | " + resData.usuarios[i].score + " \n";
            }
            */
        }
        else
        {
            Debug.Log(www.error);
        }
    }


    public void UpdateHighScore()
    {
        if (PlayerPrefs.GetInt("highScore") < Score.score)
        {
            PlayerPrefs.SetInt("highscore", Score.score);
            User user = new User();

            user.username = PlayerPrefs.GetString("username");
            user.data = new UserScore(Score.score);
            //user.data.score = Score.score;

            string postData = JsonUtility.ToJson(user);
            Debug.Log("Json: " + postData);
            StartCoroutine(UpdateScore(postData));


            //ScoreData newScore = new ScoreData();
            //newScore.score = Score.score;
            //newScore.username = PlayerPrefs.GetString("username");
            //string postData = JsonUtility.ToJson(newScore);
            //PlayerPrefs.SetInt("highscore", Score.score);
            //StartCoroutine(UpdateScore(postData));

        }
    }

    public IEnumerator UpdateScore(string postData)
    {
        Debug.Log("patch score");
        Debug.Log("highscore: " + PlayerPrefs.GetInt("highscore"));

        Debug.Log("post data score for updating" + postData); 

        string url = ServerApiURL + "/api/usuarios";
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
            AuthJsonData JsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

            Debug.Log("succesfully patched score");

        }
        else
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }
    }

    public void Registrar()
    {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<InputField>().text;
        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Registro(postData));
    }

    public void Ingresar()
    {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<InputField>().text; 
        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Login(postData));
    }
    IEnumerator Registro(string postData)
    {
        Debug.Log(postData);
        PlayerPrefs.SetInt("highscore", 0);
        Debug.Log(PlayerPrefs.GetInt("highscore"));
        string url = ServerApiURL + "/api/usuarios";
        UnityWebRequest www = UnityWebRequest.Put(url, postData);
        //UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/usuarios", postData);
        www.method = "POST";
        www.SetRequestHeader("content-type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                

                Debug.Log(jsonData.usuario.username + " se regitro con id " + jsonData.usuario._id);

                

                StartCoroutine(Login(postData));

                //Proceso de autenticacion
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }
    IEnumerator Login(string postData)
    {

        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/auth/login", postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " inicio sesion");

                Token = jsonData.token;
                Username = jsonData.usuario.username;

                PlayerPrefs.SetString("token", Token);
                PlayerPrefs.SetString("username", Username);

                Debug.Log("highscore: " + PlayerPrefs.GetInt("highscore"));

                SceneManager.LoadScene("Game"); 

            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }
    IEnumerator GetPerfil()
    {
        UnityWebRequest www = UnityWebRequest.Get(ServerApiURL + "/api/usuarios/"+Username);
        www.SetRequestHeader("x-token", Token);


        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " Sigue con la sesion inciada");
                //Cambiar de escena
            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }
}

[System.Serializable]
public class User
{
    public string _id;
    public string username;
    public string password;
    //public int score;
    public UserScore data;
    public User() { }

    [System.Serializable]
    public class UserScore
    {
        public int score;
        public UserScore(int score)
        {
            this.score = score;
        }
    }
    public User(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}

[System.Serializable]
public class UserList
{
    public List<User> usuarios;
}

[System.Serializable]
public class ScoreData
{
    public int score;
    public string username;

}
public class AuthJsonData
{
    public User usuario;
    public string token;
}
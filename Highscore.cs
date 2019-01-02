using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

[System.Serializable]
public class Highscore
{
    public System.DateTime date;
    public string colonyName;
    public double score;
    public GameEndingType endType;

    public const int SAVED_HIGHSCORES_COUNT = 10;

    public Highscore(string i_cname, double i_score, GameEndingType i_endType )
    {
        date = System.DateTime.UtcNow;
        colonyName = i_cname;
        score = i_score;
        endType = i_endType;
    }

    public static void AddHighscore(Highscore h)
    {
        var highscores = GetHighscores();
        Highscore[] newHighscores;
        if (highscores == null)
        {
            newHighscores = new Highscore[] { h };
        }
        else
        {            
            int hcount = highscores.Length;
            if (hcount < SAVED_HIGHSCORES_COUNT )
            {
                newHighscores = new Highscore[hcount + 1];
                if (h.score > highscores[0].score)
                {//FIRST
                    newHighscores[0] = h;
                    for (int i = 0; i < hcount; i++)
                    {
                        newHighscores[i + 1] = highscores[i];
                    }
                }
                else
                {
                    if (h.score < highscores[hcount - 1].score)
                    {//LAST
                        for (int i = 0; i < hcount; i++)
                        {
                            newHighscores[i] = highscores[i];
                        }
                        newHighscores[hcount] = h;
                    }
                    else
                    {//INSIDE
                        int i = 1, oldArrayIndex = 1;           
                        newHighscores[0] = highscores[0];
                        while ( oldArrayIndex < hcount)
                        {
                            if (highscores[i].score > h.score)
                            {
                                newHighscores[i] = highscores[oldArrayIndex];
                                i++;
                                oldArrayIndex++;
                            }
                            else
                            {
                                newHighscores[i] = h;
                                i++;
                                break;
                            }
                        }
                        for (; oldArrayIndex < hcount; oldArrayIndex++)
                        {
                            newHighscores[i] = highscores[oldArrayIndex];
                            i++;
                        }
                    }
                }
            }
            else
            {
                newHighscores = new Highscore[SAVED_HIGHSCORES_COUNT];
                if (highscores[0].score < h.score)
                { //FIRST
                    newHighscores[0] = h;
                    for (int i = 1; i < SAVED_HIGHSCORES_COUNT; i++)
                    {
                        newHighscores[i] = highscores[i - 1];
                    }
                }
                else
                {
                    if (highscores[SAVED_HIGHSCORES_COUNT - 1].score > h.score)
                    {//LAST
                        return;
                    }
                    else
                    {//INSIDE                        
                        newHighscores[0] = highscores[0];
                        int i = 1, oldArrayIndex = 1;
                        while (oldArrayIndex < SAVED_HIGHSCORES_COUNT)
                        {
                            if (highscores[i].score > h.score)
                            {
                                newHighscores[i] = highscores[oldArrayIndex];
                                i++;
                                oldArrayIndex++;
                            }
                            else
                            {
                                newHighscores[i] = h;
                                i++;
                                break;
                            }
                        }
                        for (; i < SAVED_HIGHSCORES_COUNT; i++)
                        {
                            newHighscores[i] = highscores[oldArrayIndex];
                            oldArrayIndex++;
                        }
                    }
                }
            }
        }

        FileStream fs = File.Create(Application.persistentDataPath + "/Highscores.lwhs");
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, newHighscores);
        fs.Close();
    }

    public static Highscore[] GetHighscores()
    {
        string path = Application.persistentDataPath + "/Highscores.lwhs";
        if (File.Exists(path))
        {
            FileStream file = File.Open(path, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            Highscore[] hs = (Highscore[])bf.Deserialize(file);
            file.Close();
            return hs;
        }
        else
        {
            return null;
        }
    }
}

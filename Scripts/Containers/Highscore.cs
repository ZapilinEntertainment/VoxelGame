using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

[System.Serializable]
public class Highscore
{
    public readonly int gameID;
    public readonly string colonyName;
    public readonly ulong score;
    public readonly GameEndingType endType;

    public const int MAX_HIGHSCORES_COUNT = 10;

    public Highscore(int i_gameID, string i_cname, ulong i_score, GameEndingType i_endType )
    {
        gameID = i_gameID;
        colonyName = i_cname;
        score = i_score;
        endType = i_endType;
    }

    public static void AddHighscore(Highscore h)
    {
        var highscores = GetHighscores();
        Highscore[] newHighscores;
        if (highscores == null || highscores.Length == 0)
        {
            newHighscores = new Highscore[] { h };
        }
        else
        {            
            foreach (var fh in highscores)
            {
                if (fh.gameID == h.gameID && fh.endType == h.endType) return;
            }

            int hcount = highscores.Length;
            if (hcount < MAX_HIGHSCORES_COUNT )
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
                newHighscores = new Highscore[MAX_HIGHSCORES_COUNT];
                if (highscores[0].score < h.score)
                { //FIRST
                    newHighscores[0] = h;
                    for (int i = 1; i < MAX_HIGHSCORES_COUNT; i++)
                    {
                        newHighscores[i] = highscores[i - 1];
                    }
                }
                else
                {
                    if (highscores[MAX_HIGHSCORES_COUNT - 1].score > h.score)
                    {//LAST
                        return;
                    }
                    else
                    {//INSIDE                        
                        newHighscores[0] = highscores[0];
                        int i = 1, oldArrayIndex = 1;
                        while (oldArrayIndex < MAX_HIGHSCORES_COUNT)
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
                        for (; i < MAX_HIGHSCORES_COUNT; i++)
                        {
                            newHighscores[i] = highscores[oldArrayIndex];
                            oldArrayIndex++;
                        }
                    }
                }
            }
        }

        highscores = newHighscores;
        FileStream fs = File.Create(Application.persistentDataPath + "/Highscores.lwhs");
        fs.WriteByte((byte)highscores.Length);
        byte[] data;
        int bytesCount = 0;
        foreach (var hs in highscores)
        {
            data = System.Text.Encoding.Default.GetBytes(hs.colonyName);
            bytesCount = data.Length;
            fs.Write(System.BitConverter.GetBytes(bytesCount), 0, 4); //  количество байтов, не длина строки
            fs.Write(data, 0, bytesCount);
            data = System.BitConverter.GetBytes(hs.score);
            fs.Write(data, 0, data.Length);
            fs.Write(System.BitConverter.GetBytes(hs.gameID),0,4);
            fs.WriteByte((byte)hs.endType);
        }
        fs.Close();
    }

    public static Highscore[] GetHighscores()
    {
        string path = Application.persistentDataPath + "/Highscores.lwhs";
        if (File.Exists(path))
        {
            FileStream fs = File.Open(path, FileMode.Open);
            int count = fs.ReadByte();
            if (count == 0)
            {
                fs.Close();
                return null;
            }
            else
            {
                if (count > MAX_HIGHSCORES_COUNT) count = MAX_HIGHSCORES_COUNT;
                var hsa = new Highscore[count];
                string name = "highscore";
                var data = new byte[4];
                System.Text.Decoder decoder = System.Text.Encoding.Default.GetDecoder();
                char[] chars;
                int bytesCount = 0;
                for (int i = 0; i < count; i++)
                {
                    fs.Read(data, 0, 4);
                    bytesCount = System.BitConverter.ToInt32(data, 0);
                    if (bytesCount > 0 && bytesCount < 100000)
                    {
                        data = new byte[bytesCount];
                        fs.Read(data, 0, bytesCount);
                        chars = new char[decoder.GetCharCount(data, 0, bytesCount)];
                        decoder.GetChars(data, 0, bytesCount, chars, 0, true);
                        name = new string(chars);
                    }
                    else name = "highscore";
                    //
                    bytesCount = 13;
                    data = new byte[bytesCount];
                    fs.Read(data, 0, bytesCount);
                    hsa[i] = new Highscore(System.BitConverter.ToInt32(data, 9), name, System.BitConverter.ToUInt64(data, 0), (GameEndingType)data[bytesCount - 1]);
                }
                fs.Close();
                return hsa;
            }                       
        }
        else
        {
            return null;
        }
    }
}

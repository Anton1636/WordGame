using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WordLevel : MonoBehaviour
{
    public int levelNum;
    public int longWordIndex;
    public string word;
    public Dictionary<char, int> charDictionary;
    public List<string> subWords;

    static public Dictionary<char,int> MakeCharDictionary(string w)
    {
        Dictionary<char, int> dictionary = new Dictionary<char, int>();
        char c;

        for (int i = 0; i < w.Length; i++)
        {
            c = w[i];

            if(dictionary.ContainsKey(c))
            {
                dictionary[c]++;
            }
            else
            {
                dictionary.Add(c, 1);
            }
        }
        return (dictionary);
    }

    public static bool CheckWordInLevel(string str, WordLevel level)
    {
        Dictionary<char, int> counts = new Dictionary<char, int>();

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];

            if (level.charDictionary.ContainsKey(c))
            {
                if (!counts.ContainsKey(c))
                {
                    counts.Add(c, 1);
                }
                else
                {
                    counts[c]++;
                }
                if (counts[c] > level.charDictionary[c])
                {
                    return (false);
                }
            }
            else
            {
                return (false);
            }
        }
        return (true);
    }
}

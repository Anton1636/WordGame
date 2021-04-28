using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameMode
{
    preGame,
    loading,
    makeLevel,
    levelPrep,
    inLevel
}

public class WordGame : MonoBehaviour
{
    static public WordGame S;

    [Header("Set in Inspector")]
    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true;
    public float bigLetterSize = 4f;
    public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color bigColorSelected = new Color(1f, 0.9f, 0.7f);
    public Vector3 bigLetterCenter = new Vector3(0, -16, 0);
    public Color[] wyrdPalette;

    [Header("Set Dynamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLettersActive;
    public string testWorld;
    private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private Transform letterAnchor;
    private Transform bigLetterAnchor;

    private void Awake()
    {
        S = this;
        letterAnchor = new GameObject("LetterAnchor").transform;
        bigLetterAnchor = new GameObject("BigLetterAnchor").transform;
    }

    private void Start()
    {
        mode = GameMode.loading;
        WordList.INIT();
    }

    public void WordListParseComplete()
    {
        mode = GameMode.makeLevel;
        currLevel = MakeWordLevel();
    }

    public  WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();

        if(levelNum == -1)
        {
            level.longWordIndex = UnityEngine.Random.Range(0, WordList.LONG_WORD_COUNT);
        }
        else
        {

        }
        level.levelNum = levelNum;
        level.word = WordList.GET_LONG_WORDS(level.longWordIndex);
        level.charDictionary = WordLevel.MakeCharDictionary(level.word);
        StartCoroutine(FindSubWordsCoroutine(level));
        return (level);
    }

    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.subWords = new List<string>();
        string str;
        List<string> words = WordList.GET_WORDS();

        for (int i = 0; i < WordList.WORD_COUNT; i++)
        {
            str = words[i];

            if(WordLevel.CheckWordInLevel(str,level))
            {
                level.subWords.Add(str);
            }
            if(i % WordList.NUM_TO_PARSE_BEFORE_YIELD ==0)
            {
                yield return null;
            }
        }
        level.subWords.Sort();
        level.subWords = SortWordsByLength(level.subWords).ToList();

        SubWordSearchComplete();
    }

    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> subWords)
    {
        subWords = subWords.OrderBy(s => s.Length);
        return subWords;
    }

    private void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
        Layout();
    }

    private void Layout()
    {
        wyrds = new List<Wyrd>();

        GameObject gameObject;
        Letter letter;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color color;
        Wyrd wyrd;

        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);

        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];
            columnWidth = Mathf.Max(columnWidth, word.Length);

            for (int j = 0; j < word.Length; j++)
            {
                c = word[j];

                gameObject = Instantiate<GameObject>(prefabLetter);
                gameObject.transform.SetParent(letterAnchor);

                letter = gameObject.GetComponent<Letter>();
                letter.c = c;

                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);
                pos.y -= (i % numRows) * letterSize;

                letter.posImmediate = pos + Vector3.up * (20 + i % numRows);
                letter.pos = pos;
                letter.timeStart = Time.time + i * 0.05f;

                gameObject.transform.localScale = Vector3.one * letterSize;

                wyrd.Add(letter);
            }           

            if(showAllWyrds)
            {
                wyrd.visible = true;
            }

            wyrd.color = wyrdPalette[word.Length - WordList.WORD_LENGTH_MIN];
            wyrds.Add(wyrd);

            if(i % numRows == numRows -1)
            {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }

        bigLetters = new List<Letter>();
        bigLettersActive = new List<Letter>();

        for (int i = 0; i < currLevel.word.Length; i++)
        {
            c = currLevel.word[i];

            gameObject = Instantiate<GameObject>(prefabLetter);
            gameObject.transform.SetParent(bigLetterAnchor);

            letter = gameObject.GetComponent<Letter>();
            letter.c = c;

            gameObject.transform.localScale = Vector3.one * bigLetterSize;
            pos = new Vector3(0, -100, 0);
            
            letter.posImmediate = pos;
            letter.pos = pos;
            letter.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
            letter.easingCuve = Easing.Sin + "-0.18";

            color = bigColorDim;

            letter.color = color;
            letter.visible = true;
            bigLetters.Add(letter);
        }

        bigLetters = ShuffleLetter(bigLetters);
        ArrangeBigLetters();

        mode = GameMode.inLevel;
    }    

    private List<Letter> ShuffleLetter(List<Letter> bigLetters)
    {
        List<Letter> newL = new List<Letter>();
        int index;

        while(bigLetters.Count > 0)
        {
            index = UnityEngine.Random.Range(0, bigLetters.Count);
            newL.Add(bigLetters[index]);
            bigLetters.RemoveAt(index);
        }
        return (newL);
    }

    private void ArrangeBigLetters()
    {
        float halfWidth = ((float)bigLetters.Count) / 2f - 0.5f;
        Vector3 pos;

        for (int i = 0; i < bigLetters.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            pos.y += bigLetterSize * 1.25f;
            bigLettersActive[i].pos = pos;
        }
    }

    private void Update()
    {
        Letter ltr;
        char c;

        switch(mode)
        {
            case GameMode.inLevel:
                foreach (char cIt in Input.inputString)
                {
                    c = System.Char.ToUpperInvariant(cIt);

                    if(upperCase.Contains(c))
                    {
                        ltr = FindNextLetterByChar(c);

                        if(ltr != null)
                        {
                            testWorld += c.ToString();
                            bigLettersActive.Add(ltr);
                            bigLetters.Remove(ltr);
                            ltr.color = bigColorSelected;
                            ArrangeBigLetters();
                        }
                    }
                    if(c == '\b')
                    {
                        if(bigLettersActive.Count == 0)
                        {
                            return;
                        }
                        if(testWorld.Length > 1)
                        {
                            testWorld = testWorld.Substring(0, testWorld.Length - 1);
                        }
                        else
                        {
                            testWorld = "";
                        }

                        ltr = bigLettersActive[bigLettersActive.Count - 1];
                        bigLettersActive.Remove(ltr);
                        bigLetters.Add(ltr);
                        ltr.color = bigColorDim;
                        ArrangeBigLetters();
                    }
                    if(c == '\n' || c == '\r')
                    {
                        CheckWorld();
                    }
                    if(c == ' ')
                    {
                        bigLetters = ShuffleLetter(bigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
        }
    }

    private Letter FindNextLetterByChar(char c)
    {
        foreach (Letter ltr in bigLetters)
        {
            if(ltr.c == c)
            {
                return (ltr);
            }
        }
        return (null);
    }

    private void CheckWorld()
    {
        string subWorld;
        bool foundTestWorld = false;
        List<int> containedWorlds = new List<int>();

        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            if(wyrds[i].found)
            {
                continue;
            }

            subWorld = currLevel.subWords[i];

            if(string.Equals(testWorld,subWorld))
            {
                HighlightWyrd(i);
                ScoreManager.SCORE(wyrds[i], 1);
                foundTestWorld = true;
            }
            else if(testWorld.Contains(subWorld))
            {
                containedWorlds.Add(i);
            }
        }

        if(foundTestWorld)
        {
            int numContained = containedWorlds.Count;
            int index;

            for (int i = 0; i < containedWorlds.Count; i++)
            {
                index = numContained - i - 1;
                HighlightWyrd(containedWorlds[index]);
                ScoreManager.SCORE(wyrds[containedWorlds[index]], i + 2);
            }
        }

        ClearBigLettersActive();
    }

    private void HighlightWyrd(int i)
    {
        wyrds[i].found = true;
        wyrds[i].color = (wyrds[i].color + Color.white) / 2f;
        wyrds[i].visible = true;
    }

    private void ClearBigLettersActive()
    {
        testWorld = "";

        foreach (Letter ltr in bigLettersActive)
        {
            bigLetters.Add(ltr);
            ltr.color = bigColorDim;
        }
        bigLettersActive.Clear();
        ArrangeBigLetters();
    }
}

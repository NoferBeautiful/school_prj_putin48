using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject FieldOfImages, Navalny, Background;
    public Sprite putin0, putin1, putin2, putin3, putin4, putin5, putin6, putin7, putin8, putin9, putin10, putin11;
    public Sprite putin120, putin20, putin30, putin40, putin50, putin60, putin70, putin80, putin90, putin100, putin110;
    float timeToSwap, timeToSwapTemp;
    bool swapped = false, canBeReturnedNow=false, showNumbers=false;
    List<List<int>> field, finalField, whereToGo, lastField;
    List<List<bool>> wasUpgradedOnThisSwipe;
    List<List<Vector3>> fixedPositions;
    List<Sprite> images, imagesAlt;
    public Vector2 startPos, direction;
    public int score, lastScore, maxScore;
    string key="0";
    public Text scoreText;
    public Button buttonRestart;
    int scoreToAdd = 0, maxNumbetAddedOnThisTurn = 0, lost = 0;

    void fOnApplicationQuit()
    {
        PlayerPrefs.SetInt("score", score);
        PlayerPrefs.SetInt("maxScore", maxScore);
        PlayerPrefs.SetInt("lost", lost);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                PlayerPrefs.SetInt("field[" + i.ToString() + "][" + j.ToString() + "]", field[i][j]);
            }

        }
        //PlayerPrefs.Save();
        //Debug.Log(PlayerPrefs.GetInt("field[0][0]", -1));
    }
    void OnApplicationPause()
    {
        PlayerPrefs.SetInt("score", score);
        PlayerPrefs.SetInt("maxScore", maxScore);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                PlayerPrefs.SetInt("field[" + i.ToString() + "][" + j.ToString() + "]", field[i][j]);
            }

        }
        //PlayerPrefs.Save();
        //Debug.Log(PlayerPrefs.GetInt("field[0][0]", -1));
    }


    Vector3 FindPosition(Vector3 from, Vector3 to, float val)
    {
        return new Vector3(from.x + val * (to.x - from.x), from.y + val * (to.y - from.y), from.z - 0.001f);
    }

    string specifyOkonchanieForScore(int b)
    {
        if ((b / 10) % 10 == 1)
            return " сроков";
        b %= 10;
        if (b == 1)
            return " срок";
        else if (b >= 2 && b <= 4)
            return " срока";
        else
            return " сроков";
    }

    List<List<int>> CopyList(List<List<int>> a)
    {
        List<List<int>> b = new List<List<int>>();
        for (int i=0; i<a.Count; i++)
        {
            b.Add(new List<int>());
            for (int j = 0; j < a[i].Count; j++)
                b[i].Add(a[i][j]);
        }
        return b;
    }

    int FindScoreOnField()
    {
        int a = 0;
        for (int i=0; i<4; i++)
        {
            for (int j=0; j<4; j++)
            {
                a += field[i][j];
            }
        }
        return a;
    }

    void SpawnPutinInRandomPlace()
    {
        int freePlacesAmount = -1;
        for (int i=0; i<4; i++)
        {
            for (int j=0; j<4; j++)
            {
                if (field[i][j] == 0)
                    freePlacesAmount++;
            }
        }
        int foundPlace = Random.Range(0, freePlacesAmount);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (field[i][j] == 0)
                    foundPlace--;
                if (foundPlace == -1)
                {
                    field[i][j] = 1;
                    if (Random.Range(0, 100) < 18)
                        field[i][j]++;
                    scoreToAdd += field[i][j];
                    //FieldOfImages.transform.GetChild(i * 4 + j).gameObject.GetComponent<SpriteRenderer>().sprite = putin1;
                    return;
                }
            }
        }
    }

    void DrawField()
    {
        for (int i=0; i<4; i++)
        {
            //Debug.Log(field[i][0].ToString() + " " + field[i][1].ToString() + " " + field[i][2].ToString() + " " + field[i][3].ToString());
            for (int j=0; j<4; j++)
            {
                FieldOfImages.transform.GetChild(i * 4 + j).gameObject.transform.position = fixedPositions[i][j];
                //Debug.Log(i.ToString() + " " + j.ToString() + " " + field[i][j].ToString());
                if (showNumbers)
                    FieldOfImages.transform.GetChild(i * 4 + j).gameObject.GetComponent<SpriteRenderer>().sprite = imagesAlt[field[i][j]];
                else
                    FieldOfImages.transform.GetChild(i * 4 + j).gameObject.GetComponent<SpriteRenderer>().sprite = images[field[i][j]];
            }
        }
    }


    void SwipeToRight()
    {
        scoreToAdd = 0;
        finalField = new List<List<int>>();
        whereToGo = new List<List<int>>();
        wasUpgradedOnThisSwipe = new List<List<bool>>();
        //lastField = CopyList(field);
        finalField = CopyList(field);
        for (int i = 0; i < 4; i++)
        {
            whereToGo.Add(new List<int>() { i * 4 + 0, i * 4 + 1, i * 4 + 2, i * 4 + 3 });
            wasUpgradedOnThisSwipe.Add(new List<bool>() { false, false, false, false });
        }
        swapped = false;
        for (int i=0; i<4; i++)
        {
            for (int j=2; j>-1; j--)
            {
                if (field[i][j] == 0)
                    continue;
                int jtemp = j;
                while (jtemp != 3)
                {
                    if (finalField[i][jtemp + 1] == 0)
                    {
                        swapped = true;
                        finalField[i][jtemp + 1] = finalField[i][jtemp];
                        finalField[i][jtemp] = 0;
                        jtemp++;
                    }
                    else if (finalField[i][jtemp + 1] == finalField[i][jtemp] && !wasUpgradedOnThisSwipe[i][jtemp + 1] && !wasUpgradedOnThisSwipe[i][jtemp] && finalField[i][jtemp] != 11)
                    {
                        maxNumbetAddedOnThisTurn = Mathf.Max(maxNumbetAddedOnThisTurn, field[i][jtemp] + 1);
                        scoreToAdd += finalField[i][jtemp] + 1;
                        swapped = true;
                        wasUpgradedOnThisSwipe[i][jtemp + 1] = true;
                        wasUpgradedOnThisSwipe[i][jtemp] = false;
                        finalField[i][jtemp + 1] = finalField[i][jtemp] + 1;
                        finalField[i][jtemp] = 0;
                        jtemp++;
                    }
                    else
                        break;
                }
                whereToGo[i][j] = i * 4 + jtemp;
            }
        }
    }
    void SwipeToLeft()
    {
        int scoreToAdd = 0;
        finalField = new List<List<int>>();
        whereToGo = new List<List<int>>();
        wasUpgradedOnThisSwipe = new List<List<bool>>();
        finalField = CopyList(field);
        for (int i = 0; i < 4; i++)
        {
            whereToGo.Add(new List<int>() { i * 4 + 0, i * 4 + 1, i * 4 + 2, i * 4 + 3 });
            wasUpgradedOnThisSwipe.Add(new List<bool>() { false, false, false, false });
        }
        swapped = false;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (field[i][j] == 0)
                    continue;
                int jtemp = j;
                while (jtemp != 0)
                {
                    if (finalField[i][jtemp - 1] == 0)
                    {
                        swapped = true;
                        finalField[i][jtemp - 1] = finalField[i][jtemp];
                        finalField[i][jtemp] = 0;
                        jtemp--;
                    }
                    else if (finalField[i][jtemp - 1] == finalField[i][jtemp] && !wasUpgradedOnThisSwipe[i][jtemp - 1] && !wasUpgradedOnThisSwipe[i][jtemp] && finalField[i][jtemp] != 11)
                    {
                        maxNumbetAddedOnThisTurn = Mathf.Max(maxNumbetAddedOnThisTurn, field[i][jtemp] + 1);
                        scoreToAdd += finalField[i][jtemp] + 1;
                        swapped = true;
                        wasUpgradedOnThisSwipe[i][jtemp - 1] = true;
                        wasUpgradedOnThisSwipe[i][jtemp] = false;
                        finalField[i][jtemp - 1] = finalField[i][jtemp] + 1;
                        finalField[i][jtemp] = 0;
                        jtemp--;
                    }
                    else
                        break;
                }
                whereToGo[i][j] = i * 4 + jtemp;
            }
        }
    }
    void SwipeToUp()
    {
        int scoreToAdd = 0;
        finalField = new List<List<int>>();
        whereToGo = new List<List<int>>();
        wasUpgradedOnThisSwipe = new List<List<bool>>();
        finalField = CopyList(field);
        for (int i = 0; i < 4; i++)
        {
            whereToGo.Add(new List<int>() { i * 4 + 0, i * 4 + 1, i * 4 + 2, i * 4 + 3 });
            wasUpgradedOnThisSwipe.Add(new List<bool>() { false, false, false, false });
        }
        swapped = false;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (field[i][j] == 0)
                    continue;
                int itemp = i;
                while (itemp != 0)
                {
                    if (finalField[itemp - 1][j] == 0)
                    {
                        swapped = true;
                        finalField[itemp - 1][j] = finalField[itemp][j];
                        finalField[itemp][j] = 0;
                        itemp--;
                    }
                    else if (finalField[itemp - 1][j] == finalField[itemp][j] && !wasUpgradedOnThisSwipe[itemp - 1][j] && !wasUpgradedOnThisSwipe[itemp][j] && finalField[itemp][j] != 11)
                    {
                        maxNumbetAddedOnThisTurn = Mathf.Max(maxNumbetAddedOnThisTurn, field[itemp][j] + 1);
                        scoreToAdd += finalField[itemp][j] + 1;
                        swapped = true;
                        wasUpgradedOnThisSwipe[itemp - 1][j] = true;
                        wasUpgradedOnThisSwipe[itemp][j] = false;
                        finalField[itemp - 1][j] = finalField[itemp][j] + 1;
                        finalField[itemp][j] = 0;
                        itemp--;
                    }
                    else
                        break;
                }
                whereToGo[i][j] = itemp * 4 + j;
            }
        }
    }
    void SwipeToDown()
    {
        int scoreToAdd = 0;
        finalField = new List<List<int>>();
        whereToGo = new List<List<int>>();
        wasUpgradedOnThisSwipe = new List<List<bool>>();
        finalField = CopyList(field);
        for (int i = 0; i < 4; i++)
        {
            whereToGo.Add(new List<int>() { i * 4 + 0, i * 4 + 1, i * 4 + 2, i * 4 + 3 });
            wasUpgradedOnThisSwipe.Add(new List<bool>() { false, false, false, false });
        }
        swapped = false;
        for (int i = 2; i > -1; i--)
        {
            for (int j = 0; j < 4; j++)
            {
                if (field[i][j] == 0)
                    continue;
                int itemp = i;
                while (itemp != 3)
                {
                    if (finalField[itemp + 1][j] == 0)
                    {
                        swapped = true;
                        finalField[itemp + 1][j] = finalField[itemp][j];
                        finalField[itemp][j] = 0;
                        itemp++;
                    }
                    else if (finalField[itemp + 1][j] == finalField[itemp][j] && !wasUpgradedOnThisSwipe[itemp + 1][j] && !wasUpgradedOnThisSwipe[itemp][j] && finalField[itemp][j] != 11)
                    {
                        maxNumbetAddedOnThisTurn = Mathf.Max(maxNumbetAddedOnThisTurn, field[itemp][j] + 1);
                        scoreToAdd += finalField[itemp][j] + 1;
                        swapped = true;
                        wasUpgradedOnThisSwipe[itemp + 1][j] = true;
                        wasUpgradedOnThisSwipe[itemp][j] = false;
                        finalField[itemp + 1][j] = finalField[itemp][j] + 1;
                        finalField[itemp][j] = 0;
                        itemp++;
                    }
                    else
                        break;
                }
                whereToGo[i][j] = itemp * 4 + j;
                //Debug.Log(i.ToString() + " " + j.ToString() + " " + itemp.ToString());
            }
        }
    }

    void RefleshUI()
    {
        scoreText.text = "Очки: " + score.ToString() + specifyOkonchanieForScore(score) + "\r\n" + "Рекорд: " + maxScore.ToString() + specifyOkonchanieForScore(maxScore);
    }
    public void returnTurn()
    {
        if (swapped || !canBeReturnedNow || lost==1)
            return;
        canBeReturnedNow = false;
        field = CopyList(lastField);
        DrawField();
        score = lastScore;
        RefleshUI();
    }
    public void restartGame()
    {
        if (swapped)
            return;
        if (lost==1)
        {
            Navalny.transform.position = new Vector3(7f, Navalny.transform.position.y, Navalny.transform.position.z);
            key = "0";
            lost = 0;
        }
        PlayerPrefs.SetInt("maxScore", maxScore);
        Awake();
        Awake();
    }
    public void ChangeMode()
    {
        if (swapped)
            return;
        showNumbers = !showNumbers;
        DrawField();
    }
    int didILost()
    {
        List<List<int>> check = new List<List<int>>() { };
        check.Add(new List<int>() { -1, -1, -1, -1, -1, -1 });
        check.Add(new List<int>() { -1, field[0][0], field[0][1], field[0][2], field[0][3], -1 });
        check.Add(new List<int>() { -1, field[1][0], field[1][1], field[1][2], field[1][3], -1 });
        check.Add(new List<int>() { -1, field[2][0], field[2][1], field[2][2], field[2][3], -1 });
        check.Add(new List<int>() { -1, field[3][0], field[3][1], field[3][2], field[3][3], -1 });
        check.Add(new List<int>() { -1, -1, -1, -1, -1, -1 });
        for (int i=1; i<5; i++)
        {
            for (int j=1; j<5; j++)
            {
                if (check[i][j] == 0)
                    continue;
                if ((check[i - 1][j] == 0 || (check[i - 1][j] == check[i][j] && check[i][j] != 11)) && check[i - 1][j] != -1)
                    return 0;
                if ((check[i + 1][j] == 0 || (check[i - 1][j] == check[i][j] && check[i][j] != 11)) && check[i + 1][j] != -1)
                    return 0;
                if ((check[i][j - 1] == 0 || (check[i][j - 1] == check[i][j] && check[i][j] != 11)) && check[i][j - 1] != -1)
                    return 0;
                if ((check[i][j + 1] == 0 || (check[i][j + 1] == check[i][j] && check[i][j] != 11)) && check[i][j + 1] != -1)
                    return 0;
            }
        }
        return 1;
    }

    void Awake()
    {
        field = new List<List<int>>();
        lastField = new List<List<int>>();
        images = new List<Sprite>() { putin0, putin1, putin2, putin3, putin4, putin5, putin6, putin7, putin8, putin9, putin10, putin11 };
        imagesAlt = new List<Sprite>() { putin0, putin120, putin20, putin30, putin40, putin50, putin60, putin70, putin80, putin90, putin100, putin110 };
        scoreToAdd = 0;
        //Debug.Log(PlayerPrefs.GetInt("field[0][0]", -1));
        maxScore = PlayerPrefs.GetInt("maxScore", 0);
        score = PlayerPrefs.GetInt("score", 0);
        lost = PlayerPrefs.GetInt("lost", 0);
        for (int i = 0; i < 4; i++)
        {
            field.Add(new List<int>() { });
            for (int j = 0; j < 4; j++)
            {
                field[i].Add(PlayerPrefs.GetInt("field[" + i.ToString() + "][" + j.ToString() + "]", 0));
                PlayerPrefs.SetInt("field[" + i.ToString() + "][" + j.ToString() + "]", 0);
            }
        }

        fixedPositions = new List<List<Vector3>>();
        for (int i = 0; i < 4; i++)
            fixedPositions.Add(new List<Vector3> { FieldOfImages.transform.GetChild(i * 4 + 0).gameObject.transform.position, FieldOfImages.transform.GetChild(i * 4 + 1).gameObject.transform.position, FieldOfImages.transform.GetChild(i * 4 + 2).gameObject.transform.position, FieldOfImages.transform.GetChild(i * 4 + 3).gameObject.transform.position });
        swapped = false;
        canBeReturnedNow = false; //передать в сохранение
        if (PlayerPrefs.GetInt("score", 0) == 0)
        {
            SpawnPutinInRandomPlace();
            score += scoreToAdd;
        }
        DrawField();
        maxScore = Mathf.Max(maxScore, score);
        scoreToAdd = 0;
        RefleshUI();
        PlayerPrefs.SetInt("score", 0);
        PlayerPrefs.Save();

        if (lost==1)
        {
            Navalny.transform.position = new Vector3(0f, Navalny.transform.position.y, Navalny.transform.position.z);
            lost = 1;
        }
    }
    
    void Update()
    {
        if (swapped)
        {
            timeToSwapTemp += Time.deltaTime;
            if (timeToSwapTemp >= timeToSwap * 1.02f)
            {
                field = CopyList(finalField);
                swapped = false;
                if (maxNumbetAddedOnThisTurn < 8)
                    SpawnPutinInRandomPlace();
                DrawField();
                lastScore = score;
                score += scoreToAdd;
                maxScore = Mathf.Max(maxScore, score); //сохранить  maxscore
                scoreToAdd = 0;
                RefleshUI();
                canBeReturnedNow = true;
                if (didILost()==1)
                {
                    Navalny.transform.position = new Vector3(0f, Navalny.transform.position.y, Navalny.transform.position.z);
                    lost = 1;
                }
                return;
            }
            for (int i=0; i<4; i++)
            {
                for (int j=0; j<4; j++)
                {
                    if (field[i][j] == 0)
                        continue;
                    FieldOfImages.transform.GetChild(i * 4 + j).gameObject.transform.position = FindPosition(fixedPositions[i][j], fixedPositions[whereToGo[i][j]/4][whereToGo[i][j]%4], timeToSwapTemp/timeToSwap);
                }
            }
            return;
        }


        if (Input.GetKeyDown(KeyCode.D))
            key = "D";
        else if (Input.GetKeyDown(KeyCode.A))
            key = "A";
        else if (Input.GetKeyDown(KeyCode.S))
            key = "S";
        else if (Input.GetKeyDown(KeyCode.W))
            key = "W";

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (!(touch.position.y < 3.5f))
                return;
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startPos = touch.position;
                    break;
                case TouchPhase.Moved:
                    direction = touch.position - startPos;
                    break;
                case TouchPhase.Ended:
                    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                    {
                        if (direction.x > 0)
                            key = "D";
                        else if (direction.x < 0)
                            key = "A";
                    }
                    else if (Mathf.Abs(direction.x) < Mathf.Abs(direction.y))
                    {
                        if (direction.y > 0)
                            key = "W";
                        else if (direction.y < 0)
                            key = "S";
                    }
                    break;
            }
        }


        if (key == "D")
        {
            SwipeToRight();
            if (!swapped)
            {
                key = "0";
                return;
            }
            lastField = CopyList(field);
            timeToSwap = 0.2f;
            timeToSwapTemp = 0f;
            maxNumbetAddedOnThisTurn = 0;
            swapped = true;
        }
        else if (key == "A")
        {
            SwipeToLeft();
            if (!swapped)
            {
                key = "0";
                return;
            }
            lastField = CopyList(field);
            timeToSwap = 0.2f;
            timeToSwapTemp = 0f;
            maxNumbetAddedOnThisTurn = 0;
            swapped = true;
        }
        else if (key == "W")
        {
            SwipeToUp();
            if (!swapped)
            {
                key = "0";
                return;
            }
            lastField = CopyList(field);
            timeToSwap = 0.2f;
            timeToSwapTemp = 0f;
            maxNumbetAddedOnThisTurn = 0;
            swapped = true;
        }
        else if (key == "S")
        {
            SwipeToDown();
            if (!swapped)
            {
                key = "0";
                return;
            }
            lastField = CopyList(field);
            timeToSwap = 0.2f;
            timeToSwapTemp = 0f;
            maxNumbetAddedOnThisTurn = 0;
            swapped = true;
        }
        key = "0";
    }
}
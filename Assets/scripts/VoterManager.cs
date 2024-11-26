using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VoterManager : MonoBehaviour
{
    public static VoterManager instance;
    public string[] levelNames;
    public Canvas canvas;
    public GameObject levelSelectParent;
    public GameObject voterPrefab;
    public GameObject boundaryPrefab;
    //public GameObject panel;
    public GameObject selectButton;
    public GameObject howPanel;
    public GameObject redistrictButton;
    public Transform voterParent;
    public TMP_Text topText;
    public TMP_Text statusText;
    public TMP_Text buttonText;
    public float voterSize;
    Voter[,] voters;
    District[] districts;
    bool[,] voterParties;
    string state;
    int level;
    int districtCount;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //Debug.Log(voterParties.Length + " " + voterParties.GetLength(1) + " " + voters.Length + " " + voters.GetLength(1));
        districts = new District[76];

        for(int a = 0; levelSelectParent.transform.childCount > a; a++)
        {
            levelSelectParent.transform.GetChild(a).GetChild(0).GetComponent<TMP_Text>().text = levelNames[a].Replace(' ', '\n');
        }

        SetState("selecting");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Redistrict()
    {
        if ("playing" == state)
        {
            for (int a = 0; voters.GetLength(0) > a; a++)
            {
                for (int b = 0; voters.GetLength(1) > b; b++)
                {
                    if (!voters[a,b].InDistrict())
                    {
                        //Debug.Log("about to create district " + districtCount);
                        districts[districtCount] = new District(voters[a,b], districtCount);
                        districtCount++;
                    }
                }
            }

            if(1 < districtCount)
            {
                for(int a = 0; districtCount > a + 1; a++)
                {
                    if (districts[a].GetSize() != districts[a + 1].GetSize())
                    {
                        statusText.text = districts[a].GetSize() + " and " + districts[a + 1].GetSize() + " were two of your district sizes. All your districts must be the same size.";
                        SetState("lost");
                        return;
                    }
                }
            }

            int blockDistricts = 0;
            int circleDistricts = 0;

            for (int a = 0; districtCount > a; a++)
            {
                int districtVote = districts[a].CountVotes();

                if(0 == districtVote)
                {
                    circleDistricts++;
                }
                else if(2 == districtVote)
                {
                    blockDistricts++;
                }
                else if(1 != districtVote)
                {
                    Debug.Log("CountVotes returned " + districtVote + " for some reason.");
                }
            }

            if(circleDistricts >= blockDistricts)
            {
                statusText.text = "The results are " + blockDistricts + " to " + circleDistricts + ". Blocks did not win the election.";
                SetState("lost");
                return;
            }

            SetState("won");
        }
        else if ("lost" == state)
        {
            SetState("playing");
        }
        else
        {
            Debug.Log("the redistrict button shouldn't be active in the " + state + " state.");
        }
    }

    public void StartLevel(int inLevel)
    {
        level = inLevel;
        voterParties = GetVoterParties();
        voters = new Voter[voterParties.GetLength(0), voterParties.GetLength(1)];

        if(8 > level)
        {
            voterSize = 72;
        }
        else
        {
            voterSize = 60;
        }

        float halfX = voterParties.GetLength(1) * voterSize / 2;
        float halfY = voterParties.GetLength(0) * voterSize / 2;

        for (int a = 0; voterParties.GetLength(0) > a; a++)
        {
            for (int b = 0; voterParties.GetLength(1) > b; b++)
            {
                GameObject voter = Instantiate(voterPrefab);
                voter.transform.SetParent(voterParent, false);
                voters[a, b] = voter.GetComponent<Voter>();
                voters[a, b].Initialize((b + 0.5f) * voterSize - halfX, (a + 0.5f) * voterSize - halfY, voterSize, voterParties[a, b]);
            }
        }

        for (int a = 0; voterParties.GetLength(0) > a + 1; a++)
        {
            for (int b = 0; voterParties.GetLength(1) > b; b++)
            {
                GameObject boundary = Instantiate(boundaryPrefab);
                boundary.transform.SetParent(voterParent, false);
                boundary.GetComponent<Boundary>().Initialize(voters[a, b], voters[a + 1, b], voterSize, true);
            }
        }

        for (int a = 0; voterParties.GetLength(0) > a; a++)
        {
            for (int b = 0; voterParties.GetLength(1) > b + 1; b++)
            {
                GameObject boundary = Instantiate(boundaryPrefab);
                boundary.transform.SetParent(canvas.transform, false);
                boundary.GetComponent<Boundary>().Initialize(voters[a, b], voters[a, b + 1], voterSize, false);
            }
        }

        SetState("playing");
    }

    public void ResetVariables()
    {
        districtCount = 0;
        
        for(int a = 0; districts.Length > a; a++)
        {
            districts[a] = null;
        }

        for(int a = 0; voters.GetLength(0) > a; a++)
        {
            for(int b = 0; voters.GetLength(1) > b; b++)
            {
                voters[a,b].ResetVariables();
            }
        }
    }

    public void SetState(string inState)
    {
        state = inState;

        /*if("how" != state)
        {
            howPanel.SetActive(false);
        }*/
        if ("selecting" == state)
        {
            topText.text = "Select a country";

            foreach (Transform child in voterParent)
            {
                Destroy(child.gameObject);
            }
        }

        if("playing" == state)
        {
            ResetVariables();
            topText.text = levelNames[level];
            buttonText.text = "Redistrict";
        }

        if("lost" == state)
        {
            buttonText.text = "Try Again";
        }
        else
        {
            statusText.text = "";
        }

        if("won" == state)
        {
            statusText.text = "You win.";
        }

        levelSelectParent.SetActive("selecting" == state);
        selectButton.SetActive("selecting" != state);
        redistrictButton.SetActive("selecting" != state && "won" != state);
    }

    public void SetHowActive(bool isActive)
    {
        howPanel.SetActive(isActive);
    }

    public string GetState()
    {
        return state;
    }

    public bool[,] GetVoterParties()
    {
        bool f = false;
        bool t = true;

        if (0 == level)
        {
            bool[,] output = {
                { f, t, t },
                { t, f, f },
                { t, f, f }
            };
        
            return output;
        }
        else if (1 == level)
        {
            bool[,] output = {
                { t, f, t, f },
                { f, f, f, t },
                { t, t, t, f }
            };

            return output;
        }
        else if (2 == level)
        {
            bool[,] output = {
                { f, f, t, t, f },
                { t, f, t, f, t },
                { t, f, f, f, f }
            };

            return output;
        }
        else if (3 == level)
        {
            bool[,] output = {
                { f, t, t, f },
                { t, t, f, t },
                { t, f, f, t },
                { f, f, t, f}
            };

            return output;
        }
        else if (4 == level)
        {
            bool[,] output = {
                { t, f, f, t, f, t },
                { f, t, t, f, f, f },
                { f, f, t, t, t, f }
            };

            return output;
        }
        else if (5 == level)
        {
            bool[,] output = {
                { f, f, t, f, t },
                { f, t, f, t, f },
                { f, f, t, f, f },
                { t, t, t, t, f }
            };

            return output;
        }
        else if (6 == level)
        {
            bool[,] output = {
                { f, t, f, t, f, f, f },
                { f, f, t, t, f, t, f },
                { t, f, t, f, f, f, t }
            };

            return output;
        }
        else if (7 == level)
        {
            bool[,] output = {
                { f, f, t, f, t, f },
                { t, t, t, t, f, f },
                { f, f, f, t, f, t },
                { f, f, t, t, t, f }
            };

            return output;
        }
        else if (8 == level)
        {
            bool[,] output = {
                { f, t, f, t, t },
                { f, f, f, f, f },
                { t, f, t, f, t },
                { f, t, t, f, f },
                { f, t, f, f, f }
            };

            return output;
        }
        else if (9 == level)
        {
            bool[,] output = {
                { f, f, t, f, f, f, t, t, f },
                { t, f, t, f, t, t, t, f, f },
                { f, t, f, t, f, t, f, t, f }
            };

            return output;
        }
        else if (10 == level)
        {
            bool[,] output = {
                { f, t, f, f, f, f, t },
                { f, t, t, f, t, t, f },
                { f, f, t, t, f, f, f },
                { f, t, f, t, t, t, f }
            };

            return output;
        }
        else if (11 == level)
        {
            bool[,] output = {
                { f, t, t, f, f, f },
                { f, t, f, t, f, t },
                { t, f, f, t, f, t },
                { t, f, f, f, t, f },
                { f, f, t, f, t, f }
            };

            return output;
        }

        bool[,] output1 = { { f } };
        return output1;
    }
}

public class District
{
    Voter[] voters;
    int size;
    int searchIndex;
    int districtNumber;
    
    public District(Voter starter, int inDistrictNumber)
    {
        //Debug.Log("creating district " + inDistrictNumber);
        size = 0;
        searchIndex = 0;
        districtNumber = inDistrictNumber;
        voters = new Voter[44];
        Add(starter);

        while(null != voters[searchIndex])
        {
            for(int a = 0; 4 > a; a++)
            {
                Boundary bound = voters[searchIndex].GetBound(a);
                //Debug.Log("bound exists: " + (null != bound));

                if(null != bound)
                {
                    //Debug.Log("bound isn't district boundary: " + !bound.IsActive());
                    if (!bound.IsActive())
                    {
                        Voter addition = bound.GetOtherVoter(voters[searchIndex]);
                        //Debug.Log("voter is in district " + addition.GetDistrictNumber());

                        if (!addition.InDistrict())
                        {
                            Add(addition);
                        }
                    }
                }
            }

            searchIndex++;
        }
    }

    public int CountVotes()
    {
        int blockVotes = 0;
        int circleVotes = 0;
        int result = 0;
        
        for(int a = 0; size > a; a++)
        {
            if (voters[a].GetParty())
            {
                blockVotes++;
            }
            else
            {
                circleVotes++;
            }
        }

        if(circleVotes > blockVotes)
        {
            result = 0;
        }
        else if(circleVotes == blockVotes)
        {
            result = 1;
        }
        else if(circleVotes < blockVotes)
        {
            result = 2;
        }
        else
        {
            Debug.Log(circleVotes + " and " + blockVotes + " broke trichotomy property somehow.");
        }

        for(int a = 0; size > a; a++)
        {
            voters[a].SetColor(result + 1);
        }

        return result;
    }

    public void Add(Voter addition)
    {
        voters[size] = addition;
        addition.SetDistrict(this);
        size++;
    }

    public int GetNumber()
    {
        return districtNumber;
    }

    public int GetSize()
    {
        return size;
    }
}
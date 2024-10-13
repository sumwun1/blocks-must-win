using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VoterManager : MonoBehaviour
{
    public static VoterManager instance;
    public bool[,] voterParties = { { false, true, true }, { true, false, false }, { true, false, false } };
    public Canvas canvas;
    public GameObject voterPrefab;
    public GameObject boundaryPrefab;
    public GameObject panel;
    public GameObject howPanel;
    public GameObject redistrictButton;
    public TMP_Text statusText;
    public TMP_Text buttonText;
    public float voterSize;
    Voter[,] voters;
    District[] districts;
    string state;
    int districtCount;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        SetState("how");
        voters = new Voter[voterParties.GetLength(0),voterParties.GetLength(1)];
        //Debug.Log(voterParties.Length + " " + voterParties.GetLength(1) + " " + voters.Length + " " + voters.GetLength(1));
        districts = new District[44];
        float halfX = voterParties.GetLength(0) * voterSize / 2;
        float halfY = voterParties.GetLength(1) * voterSize / 2;

        for (int a = 0; voterParties.GetLength(0) > a; a++)
        {
            for (int b = 0; voterParties.GetLength(1) > b; b++)
            {
                GameObject voter = Instantiate(voterPrefab);
                voter.transform.SetParent(canvas.transform, false);
                voters[a,b] = voter.GetComponent<Voter>();
                voters[a,b].Initialize((a + 0.5f) * voterSize - halfX, (b + 0.5f) * voterSize - halfY, voterParties[a,b]);
            }
        }

        for (int a = 0; voterParties.GetLength(0) > a + 1; a++)
        {
            for (int b = 0; voterParties.GetLength(1) > b; b++)
            {
                GameObject boundary = Instantiate(boundaryPrefab);
                boundary.transform.SetParent(canvas.transform, false);
                boundary.GetComponent<Boundary>().Initialize(voters[a,b], voters[a + 1,b], false);
            }
        }

        for (int a = 0; voterParties.GetLength(0) > a; a++)
        {
            for (int b = 0; voterParties.GetLength(1) > b + 1; b++)
            {
                GameObject boundary = Instantiate(boundaryPrefab);
                boundary.transform.SetParent(canvas.transform, false);
                boundary.GetComponent<Boundary>().Initialize(voters[a,b], voters[a,b + 1], true);
            }
        }

        ResetVariables();
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
                        statusText.text = districts[a].GetSize() + " and " + districts[a + 1].GetSize() + " were 2 of your district sizes. All your districts must be the same size.";
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

        if("how" != state)
        {
            howPanel.SetActive(false);
        }

        if("playing" == state)
        {
            ResetVariables();
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
            redistrictButton.SetActive(false);
        }
    }

    public string GetState()
    {
        return state;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boundary : MonoBehaviour
{
    public Sprite thick;
    public Sprite dotted;
    Image image;
    Voter voter0;
    Voter voter1;
    bool isVertical;
    bool isActive;

    // Start is called before the first frame update
    void Start()
    {
        isActive = false;
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(Voter inVoter0, Voter inVoter1, bool inIsVertical)
    {
        voter0 = inVoter0;
        voter1 = inVoter1;
        isVertical = inIsVertical;
        RectTransform rect = GetComponent<RectTransform>();
        transform.SetParent(VoterManager.instance.panel.transform);
        rect.anchoredPosition = (voter0.GetPos() + voter1.GetPos()) / 2;

        if (isVertical)
        {
            //transform.eulerAngles += new Vector3(0, 0, 90);
            voter0.SetBound(this, 1);
            voter1.SetBound(this, 3);
        }
        else
        {
            voter0.SetBound(this, 0);
            voter1.SetBound(this, 2);
        }
    }

    public Voter GetOtherVoter(Voter voter)
    {
        if(System.Object.ReferenceEquals(voter0, voter1))
        {
            Debug.Log("boundary is surrounded by the same voter on both sides.");
            return null;
        }

        if(System.Object.ReferenceEquals(voter0, voter) && !System.Object.ReferenceEquals(voter1, voter))
        {
            return voter0;
        }

        if (!System.Object.ReferenceEquals(voter0, voter) && System.Object.ReferenceEquals(voter1, voter))
        {
            return voter1;
        }

        Debug.Log("neither voter matched the input.");
        return null;
    }

    public void OnClick()
    {
        if("playing" == VoterManager.instance.GetState())
        {
            isActive = !isActive;

            if (isActive)
            {
                image.sprite = thick;
            }
            else
            {
                image.sprite = dotted;
            }
        }
    }

    public bool IsActive()
    {
        return isActive;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Voter : MonoBehaviour
{
    public Color[] colors;
    public Sprite blockSprite;
    public Sprite circleSprite;
    Boundary[] bounds;
    Image image;
    Vector2 pos;
    District district;
    bool isBlock;
    
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(float posX, float posY, bool inIsBlock)
    {
        isBlock = inIsBlock;
        RectTransform rect = GetComponent<RectTransform>();
        transform.SetParent(VoterManager.instance.panel.transform);
        pos = new Vector2(posX, posY);
        rect.anchoredPosition = pos;
        SpriteRenderer childRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        if (isBlock)
        {
            childRenderer.sprite = blockSprite;
        }
        else
        {
            childRenderer.sprite = circleSprite;
        }
    }

    public void SetBound(Boundary boundary, int index)
    {
        if(null == bounds)
        {
            bounds = new Boundary[4];
        }

        bounds[index] = boundary;
    }

    public void SetColor(int index)
    {
        image.color = colors[index];
    }

    public void SetDistrict(District inDistrict)
    {
        district = inDistrict;
    }

    public void ResetVariables()
    {
        district = null;
        image.color = colors[0];
    }

    public Boundary GetBound(int index)
    {
        return bounds[index];
    }

    public Vector2 GetPos()
    {
        return pos;
    }

    public bool InDistrict()
    {
        return null != district;
    }

    public bool GetParty()
    {
        return isBlock;
    }
}

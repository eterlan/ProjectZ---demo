using System.Collections;
using System.Collections.Generic;
using Tests.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

public class ImageBuilder : TestDataBuilder<Image>
{
    // TDB Guild #1 : Field <- Constructor
    private float m_fillAmount;

    public ImageBuilder(int fillAmount)
    {
        m_fillAmount = fillAmount;
    }
    
    // TDB Guild #2 : Initialize its instance variable to commonly used value or safe value
    public ImageBuilder() : this(0){}
    
    // TDB Guild #3 : Has a "BUILD" Method that create a new Object using the values in its instance variables
    public override Image Build()
    {
        var image = new GameObject().AddComponent<Image>();
        image.fillAmount = m_fillAmount;
        return image;
    }
    
    // TDB Guild #4 : Has "chainable" method for overriding the values in its instance variables.
    //    "chainable" : return object instead of 
    public ImageBuilder WithFillAmount(float fillAmount)
    {
        m_fillAmount = fillAmount;
        return this;
    }
}

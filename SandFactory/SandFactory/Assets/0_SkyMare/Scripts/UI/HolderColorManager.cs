using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolderColorManager : MonoBehaviour
{
    public MeshRenderer liquid;
    public MeshRenderer cap;
    public MeshRenderer cap_Border;
    public ParticleSystem pourEffect;
    public ParticleSystem cloneFX;

    [Header("Test")]
    public ColorID colors;
    int colorID = 0;
    [SerializeField]private bool testColor = false;
    public void Update()
    {
        if(!testColor) return;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            colorID++;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            colorID--;
        }
        if (colorID >= 0 && colorID < colors.colorWithIDs.Count)
        {
            ChangeColor(colors.colorWithIDs[colorID]);
        }
    }
    public void ChangeColor(ColorWithID color)
    {
        //.material.SetColor("_LiquidColor" "_SurfaceColor" _PresenalColor _OutLine_Color)
        liquid.material.SetColor("_LiquidColor", color.liquidColor);
        liquid.material.SetColor("_SurfaceColor", color.surfaceColor);
        liquid.material.SetColor("_PresenalColor", color.gradiantColor);

        cap.materials[0].color = color.capColor;
        cap.materials[1].SetColor("_OutLine_Color", color.capOutlineColor);

        cloneFX = Instantiate(pourEffect);//.main; // Get the module from the ParticleSystem instance
        cloneFX.startColor = color.liquidColor;
        cloneFX.gameObject.transform.SetParent(pourEffect.transform.parent);
        cloneFX.gameObject.transform.localPosition = pourEffect.transform.localPosition;

        cap_Border.material.color = color.capColor;
    }
}

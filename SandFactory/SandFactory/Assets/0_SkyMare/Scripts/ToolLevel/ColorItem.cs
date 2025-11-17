using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolLevel
{
    public class ColorItem : MonoBehaviour
    {
        public SpriteRenderer render;
        public Color color;
        public int colorID;

        public void OnInitialize(ColorWithID clix)
        {
            this.color= clix.color;
            this.colorID = clix.ID;
            render.color = this.color;
        }

        public ColorItem clone;
        
    }
}
 

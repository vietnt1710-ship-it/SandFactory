using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public LayerMask collision;
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, collision))
            {
                ObjectTile tile = hit.collider.gameObject.GetComponentInParent<ObjectTile>();
                if(tile!= null)
                {
                    tile.OnClick();
                }
            }
        }
    }
}

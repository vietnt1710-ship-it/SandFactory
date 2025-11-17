using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpManger : Singleton<PopUpManger>
{
    public ClaimTokenEffect m_claimToken;
    public List<PopUp> popups;

    private void Start()
    {
        for (int i = 0; i < popups.Count; i++)
        {
            popups[i].SubEvent();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState_GS
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardGolfSolitaire : Card_GS
{
    [Header("Set Dynamically: CardGolfSolitaire")]
    public eCardState_GS state = eCardState_GS.drawpile;
    public List<CardGolfSolitaire> hiddenBy = new List<CardGolfSolitaire>();
    public int layoutID;
    public SlotDef_GS slotDef;

    public override void OnMouseUpAsButton()
    {
        GolfSolitaire.S.CardClicked(this); 
        base.OnMouseUpAsButton();
    }

}

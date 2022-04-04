using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardGolfSolitaire : Card
{
    [Header("Set Dynamically: CardGolfSolitaire")]
    public eCardState state = eCardState.drawpile;
    public List<CardGolfSolitaire> hiddenBy = new List<CardGolfSolitaire>();
    public int layoutID;
    public SlotDef slotDef;

    public override void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
    
}

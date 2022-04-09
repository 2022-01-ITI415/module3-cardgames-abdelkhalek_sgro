using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GolfSolitaire : MonoBehaviour
{
    static public GolfSolitaire S;

    [Header("Set in Inspector")]
    public TextAsset deckXML_GS;
    public TextAsset layoutXML_GS;
    public float xOffset = 3;
    public float yOffset = -2 / 5f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(.5f, .9f);
    public Vector2 fsPosRun = new Vector2(.5f, .75f);
    public Vector2 fsPosMid2 = new Vector2(.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(.5f, .95f);
    public float reloadDelay = 1f;
    public Text gameOverText, roundResultText;

    [Header("Set Dynamically")]
    public Deck_GS deck;
    public Layout_GS layout;
    public List<CardGolfSolitaire> drawPile;
    public Transform layoutAnchor;
    public CardGolfSolitaire target;
    public List<CardGolfSolitaire> tableau;
    public List<CardGolfSolitaire> discardPile;

    void Awake()
    {
        S = this;
        SetUpUITexts();
    }

    void SetUpUITexts()
    {
        GameObject go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }
        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }
        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }

    void Start()
    {

        deck = GetComponent<Deck_GS>();
        deck.InitDeck(deckXML_GS.text);
        Deck_GS.Shuffle(ref deck.cards);

        layout = GetComponent<Layout_GS>();
        layout.ReadLayout(layoutXML_GS.text);

        drawPile = ConvertListCardsToListCardGolfSolitaire(deck.cards);

        LayoutGame();
    }
    List<CardGolfSolitaire> ConvertListCardsToListCardGolfSolitaire(List<Card_GS> lCD)
    {
        List<CardGolfSolitaire> lCP_GS = new List<CardGolfSolitaire>();
        CardGolfSolitaire tCP_GS;
        foreach (Card_GS tCD_GS in lCD)
        {
            tCP_GS = tCD_GS as CardGolfSolitaire;
            lCP_GS.Add(tCP_GS);
        }
        return (lCP_GS);
    }

    CardGolfSolitaire Draw()
    {

        CardGolfSolitaire cd = drawPile[0];
        drawPile.RemoveAt(0);
        return (cd);
    }
    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }
        CardGolfSolitaire cp;
        foreach (SlotDef_GS tSD in layout.slotDefs)
        {
            cp = Draw();
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID);
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eCardState_GS.tableau;
            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);
        }
        foreach (CardGolfSolitaire tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    CardGolfSolitaire FindCardByLayoutID(int layoutID)
    {
        foreach (CardGolfSolitaire tCP in tableau)
        {
            if (tCP.layoutID == layoutID)
            {
                return (tCP);
            }
        }
        return (null);
    }

    void SetTableauFaces()
    {
        foreach (CardGolfSolitaire cd in tableau)
        {
            bool faceUp = true;
            foreach (CardGolfSolitaire cover in cd.hiddenBy)
            {
                if (cover.state == eCardState_GS.tableau)
                {
                    faceUp = true;
                }
            }
            cd.faceUp = faceUp;
        }
    }

    void MoveToDiscard(CardGolfSolitaire cd)
    {
        cd.state = eCardState_GS.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + .5f);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    void MoveToTarget(CardGolfSolitaire cd)
    {
        if (target != null) MoveToDiscard(target);
        target = cd;
        cd.state = eCardState_GS.target;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    void UpdateDrawPile()
    {
        CardGolfSolitaire cd;
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x), layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + .1f * i);
            cd.faceUp = false;
            cd.state = eCardState_GS.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public void CardClicked(CardGolfSolitaire cd)
    {
        switch (cd.state)
        {
            case eCardState_GS.target:
                break;
            case eCardState_GS.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();

                break;
            case eCardState_GS.tableau:
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target))
                {
                    validMatch = false;
                }
                if (!validMatch) return;
                tableau.Remove(cd);
                MoveToTarget(cd);
                SetTableauFaces();

                break;
        }
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (tableau.Count == 0)
        {
            GameOver(true);
            return;
        }
        if (drawPile.Count > 0)
        {
            return;
        }
        foreach (CardGolfSolitaire cd in tableau)
        {
            if (AdjacentRank(cd, target))
            {
                return;
            }
        }
        GameOver(false);
    }

    void GameOver(bool won)
    {

        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round!";
            ShowResultsUI(true);
            //print("Game Over. You Won! :");

        }
        else
        {
            gameOverText.text = "Game Over";
            ShowResultsUI(true);
            //print("Game Over. You Lost. :");

        }
        SceneManager.LoadScene("GolfSolitaire");
        Invoke("ReloadLevel", reloadDelay);
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene("GolfSolitaire");
    }

    public bool AdjacentRank(CardGolfSolitaire c0, CardGolfSolitaire c1)
    {
        if (!c0.faceUp || !c1.faceUp) return (false);

        if (Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return (true);
        }
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);
        return (false);
    }
}
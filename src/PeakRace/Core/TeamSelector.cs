using PeakRace.Patch;
using UnityEngine;


namespace PeakRace.src;

public class TeamSelector : MonoBehaviour, IInteractibleConstant, IInteractible
{
    float switchTime = .25f;
    string teamName;
    int team;
    public bool holdOnFinish => false;
    private Renderer mainRenderer;

    //TODO localize text?
    //LocalizedText.GetText(teamName)
    void Awake()
    {
        mainRenderer = GetComponentInChildren<Renderer>();
        GameObject selector = this.gameObject;
        //Describe location
    }

    public void setTeam(int teamInt)
    {
        team = teamInt;
        teamName = Plugin.teamList[teamInt].Item1.ToUpper();
    }

    public string GetInteractionText()
    {
       return "JOIN";
    }

    public void Interact_CastFinished(Character interactor)
    {
        CharacterTeamInfo teamInfo = interactor.GetComponent<CharacterTeamInfo>();
        Debug.Log($"[RaceToThePeak] Updated {teamInfo.myChar.name} to team {team}");
        teamInfo.changeTeam(team);
    }

    public void Interact(Character interactor)
    {
    }

    public void CancelCast(Character interactor)
    {
    }

    public void ReleaseInteract(Character interactor)
    {
    }

    public void HoverEnter()
    {
    }

    public void HoverExit()
    {
    }

    public Vector3 Center()
    {
        return mainRenderer.bounds.center;
    }

    public bool IsInteractible(Character interactor)
    {
        return true;
    }
    public bool IsConstantlyInteractable(Character interactor)
    {
        return true;
    }

    public float GetInteractTime(Character interactor)
    {
        return switchTime;
    }
    public string GetName()
    {

        return teamName;
    }
    public Transform GetTransform()
    {
        return base.transform;
    }
}


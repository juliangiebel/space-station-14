using Content.Shared.Survey;
using Content.Shared.Survey.Prototypes;
using Robust.Client.UserInterface;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Survey;

public sealed class SurveyManager
{
    [Dependency] private readonly IClientNetManager _netManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;


    private Control? _popupContainer;
    // this is copy pasted from vote manager, for future proofing with custom surveys that admins can call midround.
    private readonly Dictionary<int, SurveyPopup> _surveyPopups = new();
    private List<ActiveSurvey> _surveys = new();
    private int _ids = 0;

    public void Initialize()
    {
        _netManager.RegisterNetMessage<SurveyStartedMsg>(ReceiveSurveyStartedMsg);
        _netManager.RegisterNetMessage<SurveyRespondedMsg>();
    }

    private void ReceiveSurveyStartedMsg(SurveyStartedMsg message)
    {
        if (!_prototypeManager.TryIndex<SurveyPrototype>(message.PrototypeId, out var prototype))
        {
            return;
        }

        _surveys.Add(new ActiveSurvey(prototype, true, message.EndTime, _ids++, message.StartTime));
        SetVoteData();
    }

    public void ClearPopupContainer()
    {
        if (_popupContainer == null)
            return;

        if (!_popupContainer.Disposed)
        {
            foreach (var popup in _surveyPopups.Values)

            {
                popup.Orphan();
            }
        }

        _surveyPopups.Clear();
        _popupContainer = null;
    }

    public void SetPopupContainer(Control container)
    {
        if (_popupContainer != null)
        {
            ClearPopupContainer();
        }

        _popupContainer = container;
        SetVoteData();
    }

    private void SetVoteData()
    {
        if (_popupContainer == null)
            return;

        foreach (var survey in _surveys)
        {
            var popup = new SurveyPopup(survey);

            _surveyPopups.Add(survey.Id, popup);
            _popupContainer.AddChild(popup);
        }
    }

    public void CloseSurvey(int surveyId)
    {
        if (!_surveyPopups.TryGetValue(surveyId, out var popup))
        {
            return;
        }

        popup.Orphan();
        _surveyPopups.Remove(surveyId);
    }
}

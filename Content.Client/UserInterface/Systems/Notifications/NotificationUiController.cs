using Content.Client.Gameplay;
using Content.Client.Notifications;
using Content.Client.UserInterface.Controls;
using Content.Shared.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;

namespace Content.Client.UserInterface.Systems.Notifications;

public sealed class NotificationUiController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    private NotificationsWindow _notificationsWindow = null!;

    private MenuButton? NotificationsButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.NotificationsButton;

    public override void Initialize()
    {
        _notificationsWindow = UIManager.CreateWindow<NotificationsWindow>();
    }

    public void OnStateEntered(GameplayState state)
    {
        _notificationsWindow.OnClose += DeactivateButton;
        _notificationsWindow.OnOpen += ActivateButton;

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenNotifications,
                InputCmdHandler.FromDelegate(_ => ToggleNotificationsMenu()))
            .Register<NotificationUiController>();
    }

    public void OnStateExited(GameplayState state)
    {
        _notificationsWindow.OnClose -= DeactivateButton;
        _notificationsWindow.OnOpen -= ActivateButton;

        _notificationsWindow.Close();
        CommandBinds.Unregister<NotificationUiController>();
    }

    public void UnloadButton()
    {
        if (NotificationsButton == null)
            return;

        NotificationsButton.OnPressed -= ActionButtonPressed;
    }

    public void LoadButton()
    {
        if (NotificationsButton == null)
            return;

        NotificationsButton.OnPressed += ActionButtonPressed;
    }

    private void ActionButtonPressed(BaseButton.ButtonEventArgs args)
    {
        ToggleNotificationsMenu();
    }

    private void ToggleNotificationsMenu()
    {
        if (_notificationsWindow.IsOpen)
        {
            _notificationsWindow.Close();
        }
        else
        {
            _notificationsWindow.OpenCentered();
        }
    }

    private void ActivateButton()
    {
        NotificationsButton?.SetClickPressed(true);
    }

    private void DeactivateButton()
    {
        NotificationsButton?.SetClickPressed(false);
    }
}

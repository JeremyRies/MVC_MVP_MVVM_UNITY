using System;
using UnityEngine.UI;

public abstract class AbstractButtonView
{
    protected readonly Action Action;

    protected AbstractButtonView(Action action)
    {
        Action = action;
    }

    public abstract bool ButtonActive { get; set; }
}

public class MockButtonView : AbstractButtonView
{
    public MockButtonView(Action action) : base(action)
    {
    }

    public override bool ButtonActive { get; set; }

    public void SimulateButtonPress()
    {
        Action();
    }
}

public class ButtonView : AbstractButtonView
{
    private readonly Button _button;

    public ButtonView(Action action, Button button) : base(action)
    {
        _button = button;
        _button.onClick.AddListener(() => Action());
    }

    public override bool ButtonActive
    {
        set { _button.interactable = value; }
        get { return _button.interactable; }
    }
}
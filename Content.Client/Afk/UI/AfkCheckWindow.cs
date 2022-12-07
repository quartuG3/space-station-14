using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Localization;
using static Robust.Client.UserInterface.Controls.BoxContainer;
using Robust.Shared.Timing;

namespace Content.Client.Afk.UI
{
    public sealed class AfkCheckWindow : DefaultWindow
    {
        private float _timer;
        public readonly Button ImOkButton;

        public AfkCheckWindow()
        {
            _timer = 60;

            Title = Loc.GetString("afk-system");

            Contents.AddChild(new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Children =
                {
                    new BoxContainer
                    {
                        Orientation = LayoutOrientation.Vertical,
                        Children =
                        {
                            (new Label()
                            {
                                Text = Loc.GetString("afk-system-kick-warning")
                            }),
                            new BoxContainer
                            {
                                Orientation = LayoutOrientation.Horizontal,
                                Align = AlignMode.Center,
                                Children =
                                {
                                    (ImOkButton = new Button
                                    {
                                        Text = Loc.GetString("afk-system-button-im-here-timer", ("time", $"{_timer:0.0}")),
                                    })
                                }
                            },
                        }
                    },
                }
            });
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);
            if (ImOkButton.Disabled) return;
            if (_timer > 0.0f)
            {
                _timer -= args.DeltaSeconds;
                ImOkButton.Text = Loc.GetString("afk-system-button-im-here-timer", ("time", $"{_timer:0.0}"));
            }
            else
            {
                ImOkButton.Disabled = true;
                ImOkButton.Text = Loc.GetString("afk-system-button-im-here");
            }
        }
    }
}

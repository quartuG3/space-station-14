using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Localization;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.Afk.UI
{
    public sealed class AfkCheckWindow : DefaultWindow
    {
        public readonly Button ImOkButton;

        public AfkCheckWindow()
        {

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
                                        Text = Loc.GetString("afk-system-button-im-here"),
                                    })
                                }
                            },
                        }
                    },
                }
            });
        }
    }
}

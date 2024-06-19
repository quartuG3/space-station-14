using Robust.Shared.Prototypes;

namespace Content.Server.AruMoon.StationGoal
{
    [Serializable, Prototype("stationGoal")]
    public sealed class StationGoalPrototype : IPrototype
    {
        [IdDataFieldAttribute]
        public string ID { get; } = default!;

        [DataField]
        public string Text { get; set; } = string.Empty;

        [DataField]
        public int? MinPlayers;

        [DataField]
        public int? MaxPlayers;

    }
}

using Terraria.ModLoader.Config;

namespace NeoLightBeams {

    public class NeoConfigServer : ModConfig {

        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Label("Demo Mode")]
        [Tooltip("Give the mod a test drive.")]
        [ReloadRequired]
        public bool DemoMode { get; set; }

    }

}

using SoG.Modding.Core;

namespace SoG.Modding.Utils
{
    public static class NetTools
    {
        public static bool IsLocalOrServer => Globals.Game.xNetworkInfo.enCurrentRole != NetworkHelperInterface.NetworkRole.Client;

        public static bool IsClient => Globals.Game.xNetworkInfo.enCurrentRole == NetworkHelperInterface.NetworkRole.Client;
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Valve.VR
{
    using System;
    using UnityEngine;
    
    
    public partial class SteamVR_Actions
    {
        
        private static SteamVR_Input_ActionSet_player_controller p_player_controller;
        
        public static SteamVR_Input_ActionSet_player_controller player_controller
        {
            get
            {
                return SteamVR_Actions.p_player_controller.GetCopy<SteamVR_Input_ActionSet_player_controller>();
            }
        }
        
        private static void StartPreInitActionSets()
        {
            SteamVR_Actions.p_player_controller = ((SteamVR_Input_ActionSet_player_controller)(SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_player_controller>("/actions/player_controller")));
            Valve.VR.SteamVR_Input.actionSets = new Valve.VR.SteamVR_ActionSet[] {
                    SteamVR_Actions.player_controller};
        }
    }
}

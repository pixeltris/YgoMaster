using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YgoMaster
{
    // TODO: Maybe remove this? Operation.Dialog only seems to actually show in special cases (i.e. error code response for System.Info)
    static class Dialogs
    {
        // Tid suffix will use text from YgoGame.TextIDs
        // e.g. "titleTid":"IDS_SYS_MAINTE_TITLE" maps to YgoGame.TextIDs.IDS_SYS.MAINTE_TITLE

        // See YgomSystem.UI.SystemDialog
        // title, upperMessage, lowerMessage, lowerMessageScrollHeight, time, imagePath, imageScale, imageVisible, buttonLabel, buttonLabel2, action, action2, rebootVisible, windowWidth, opense

        public static void OpenMaintenance(GameServerWebRequest request)
        {
            request.Response["Operation"] = new Dictionary<string, object>() {
                { "Dialog", new Dictionary<string, object>() {
                    { "titleTid", "IDS_SYS_MAINTE_TITLE" },
                    { "buttonLabelTid", "IDS_SYS_OK" },
                    { "buttonUrl", "" },
                    { "upperMessageTid", "IDS_SYS_MAINTE_UPPER_MESSAGE_ALL" },
                    { "lowerMessageTid", "IDS_SYS_MAINTE_LOWER_MESSAGE_ALL" },
                    //{ "lowerMessage", "Yu-Gi-Oh! MASTER DUEL is currently preparing for the release. We appreciate your patience until the launch." },
                }}
            };
        }

        public static void OpenYesNo(GameServerWebRequest request, string title, string lowerMessage, string upperMessage = null, string yesUrl = null, string noUrl = null)
        {
            request.Response["Operation"] = new Dictionary<string, object>() {
                { "Dialog", new Dictionary<string, object>() {
                    { "title", title },
                    { "buttonLabelTid", "IDS_SYS_YES" },
                    { "buttonUrl", yesUrl },
                    { "buttonLabelTid2", "IDS_SYS_NO" },
                    { "buttonUrl2", noUrl },
                    { "lowerMessage", lowerMessage },
                    { "upperMessageTid", upperMessage },
                    //{ "imagePath", "Images/Consume/GemShopIcon07" }
                }}
            };
        }

        public static void OpenOK(GameServerWebRequest request, string title, string lowerMessage, string upperMessage = null)
        {
            request.Response["Operation"] = new Dictionary<string, object>() {
                { "Dialog", new Dictionary<string, object>() {
                    { "title", title },
                    { "buttonLabelTid", "IDS_SYS_OK" },
                    { "lowerMessage", lowerMessage },
                    { "upperMessage", upperMessage }
                }}
            };
        }
    }
}

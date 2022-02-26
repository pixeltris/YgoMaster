using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2CPP;
using YgomGame.Dialog.CommonDialog;

namespace YgomGame.Menu
{
    unsafe static class CommonDialogViewController
    {
        static IL2Method methodOpenConfirmationDialog;
        static IL2Method methodOpenConfirmationDialogScroll;
        static IL2Method methodOpenYesNoConfirmationDialogScroll;
        static IL2Method methodOpenErrorDialog;
        static IL2Method methodOpenAlertDialog;
        static IL2Method methodOpenConfirmationPartDialog;
        static IL2Method methodOpenYesNoConfirmationDialog;
        static IL2Method methodOpenNoticeYesNoDialog;
        static IL2Method methodOpenItemConfirmDialog1;
        static IL2Method methodOpenItemConfirmDialog2;
        static IL2Method methodOpenCheckBoxDialog;

        static CommonDialogViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("CommonDialogViewController", "YgomGame.Menu");
            methodOpenConfirmationDialog = classInfo.GetMethod("OpenConfirmationDialog");
            methodOpenConfirmationDialogScroll = classInfo.GetMethod("OpenConfirmationDialogScroll");
            methodOpenYesNoConfirmationDialogScroll = classInfo.GetMethod("OpenYesNoConfirmationDialogScroll");
            methodOpenErrorDialog = classInfo.GetMethod("OpenErrorDialog");
            methodOpenAlertDialog = classInfo.GetMethod("OpenAlertDialog");
            methodOpenConfirmationPartDialog = classInfo.GetMethod("OpenConfirmationPartDialog");
            methodOpenYesNoConfirmationDialog = classInfo.GetMethod("OpenYesNoConfirmationDialog");
            methodOpenNoticeYesNoDialog = classInfo.GetMethod("OpenNoticeYesNoDialog");
            methodOpenItemConfirmDialog1 = classInfo.GetMethod("OpenItemConfirmDialog", x => x.GetParameters().FirstOrDefault(y => y.Name == "isPeriod") == null);
            methodOpenItemConfirmDialog2 = classInfo.GetMethod("OpenItemConfirmDialog", x => x.GetParameters().FirstOrDefault(y => y.Name == "isPeriod") != null);
            methodOpenCheckBoxDialog = classInfo.GetMethod("OpenCheckBoxDialog");
        }

        static IntPtr GetArgsPtr(Dictionary<string, object> args)
        {
            IntPtr argsPtr = IntPtr.Zero;
            if (args != null && args.Count > 0)
            {
                argsPtr = YgomMiniJSON.Json.Deserialize(MiniJSON.Json.Serialize(args));
            }
            return argsPtr;
        }

        public static void OpenConfirmationDialog(
            string title,
            string message,
            string buttonLabel,
            Action action,
            Dictionary<string, object> args = null,
            bool allowCancel = true,
            CommonDialogTitleWidget.IconType iconType = CommonDialogTitleWidget.IconType.None,
            CommonDialogButtonGroupWidget.ButtonType buttonType = CommonDialogButtonGroupWidget.ButtonType.Positive)
        {
            methodOpenConfirmationDialog.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr, new IL2String(message).ptr, new IL2String(buttonLabel).ptr,
                UnityEngine.Events._UnityAction.CreateAction(action), GetArgsPtr(args), new IntPtr(&allowCancel),
                new IntPtr(&iconType), new IntPtr(&buttonType)
            });
        }

        public static void OpenConfirmationDialogScroll(
            string title,
            string message,
            string buttonLabel,
            Action action,
            Dictionary<string, object> args = null,
            bool allowCancel = true,
            int height = 120,
            CommonDialogTitleWidget.IconType iconType = CommonDialogTitleWidget.IconType.None,
            CommonDialogButtonGroupWidget.ButtonType buttonType = CommonDialogButtonGroupWidget.ButtonType.Positive)
        {
            methodOpenConfirmationDialogScroll.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr, new IL2String(message).ptr, new IL2String(buttonLabel).ptr,
                UnityEngine.Events._UnityAction.CreateAction(action), GetArgsPtr(args), new IntPtr(&allowCancel), new IntPtr(&height),
                new IntPtr(&iconType), new IntPtr(&buttonType)
            });
        }

        public static void OpenYesNoConfirmationDialogScroll(
            string title,
            string message,
            Action action,
            Action noAction = null,
            string yesLabel = null,
            string noLabel = null,
            bool allowCancel = true,
            int height = 120,
            Dictionary<string, object> args = null,
            CommonDialogTitleWidget.IconType iconType = CommonDialogTitleWidget.IconType.None,
            CommonDialogButtonGroupWidget.ButtonType yesButtonType = CommonDialogButtonGroupWidget.ButtonType.Positive,
            bool selectedNoButton = false)
        {
            methodOpenYesNoConfirmationDialogScroll.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr, new IL2String(message).ptr,
                UnityEngine.Events._UnityAction.CreateAction(action), UnityEngine.Events._UnityAction.CreateAction(noAction),
                new IL2String(yesLabel).ptr, new IL2String(noLabel).ptr, new IntPtr(&allowCancel), new IntPtr(&height),
                GetArgsPtr(args), new IntPtr(&iconType), new IntPtr(&yesButtonType), new IntPtr(&selectedNoButton)
            });
        }

        public static void OpenErrorDialog(
            string title,
            string message,
            string buttonLabel,
            Action action,
            Dictionary<string, object> args = null,
            bool allowCancel = true,
            CommonDialogTitleWidget.IconType iconType = CommonDialogTitleWidget.IconType.None,
            CommonDialogButtonGroupWidget.ButtonType buttonType = CommonDialogButtonGroupWidget.ButtonType.Positive)
        {
            methodOpenErrorDialog.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr, new IL2String(message).ptr, new IL2String(buttonLabel).ptr,
                UnityEngine.Events._UnityAction.CreateAction(action), GetArgsPtr(args), new IntPtr(&allowCancel),
                new IntPtr(&iconType), new IntPtr(&buttonType)
            });
        }

        public static void OpenAlertDialog(
            string title,
            string message,
            Action action,
            string buttonLabel = null,
            Dictionary<string, object> args = null,
            bool allowCancel = true,
            CommonDialogButtonGroupWidget.ButtonType buttonType = CommonDialogButtonGroupWidget.ButtonType.Positive)
        {
            methodOpenAlertDialog.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr, new IL2String(message).ptr, UnityEngine.Events._UnityAction.CreateAction(action),
                new IL2String(buttonLabel).ptr, GetArgsPtr(args), new IntPtr(&allowCancel), new IntPtr(&buttonType)
            });
        }

        public static void OpenConfirmationPartDialog(
            string title,
            string message,
            string buttonLabel,
            Action action,
            Dictionary<string, object> args = null,
            bool allowCancel = true,
            CommonDialogTitleWidget.IconType iconType = CommonDialogTitleWidget.IconType.None,
            CommonDialogButtonGroupWidget.ButtonType buttonType = CommonDialogButtonGroupWidget.ButtonType.Positive)
        {
            methodOpenConfirmationPartDialog.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr, new IL2String(message).ptr, new IL2String(buttonLabel).ptr,
                UnityEngine.Events._UnityAction.CreateAction(action), GetArgsPtr(args), new IntPtr(&allowCancel),
                new IntPtr(&iconType), new IntPtr(&buttonType)
            });
        }

        public static void OpenYesNoConfirmationDialog(
            string title,
            string message,
            Action action,
            Action noAction = null,
            Dictionary<string, object> args = null,
            string yesLabel = null,
            string noLabel = null,
            bool allowCancel = true,
            CommonDialogTitleWidget.IconType iconType = CommonDialogTitleWidget.IconType.None,
            CommonDialogButtonGroupWidget.ButtonType yesButtonType = CommonDialogButtonGroupWidget.ButtonType.Positive,
            bool selectedNoButton = false)
        {
            methodOpenYesNoConfirmationDialog.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr, new IL2String(message).ptr,
                UnityEngine.Events._UnityAction.CreateAction(action), UnityEngine.Events._UnityAction.CreateAction(noAction),
                GetArgsPtr(args), new IL2String(yesLabel).ptr, new IL2String(noLabel).ptr, new IntPtr(&allowCancel),
                new IntPtr(&iconType), new IntPtr(&yesButtonType), new IntPtr(&selectedNoButton)
            });
        }

        public static void OpenNoticeYesNoDialog(
            string title,
            string message,
            Action action,
            Action noAction = null,
            string yesLabel = null,
            string noLabel = null,
            Dictionary<string, object> args = null,
            bool allowCancel = true,
            CommonDialogButtonGroupWidget.ButtonType yesButtonType = CommonDialogButtonGroupWidget.ButtonType.Positive)
        {
            methodOpenNoticeYesNoDialog.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr, new IL2String(message).ptr,
                UnityEngine.Events._UnityAction.CreateAction(action), UnityEngine.Events._UnityAction.CreateAction(noAction),
                new IL2String(yesLabel).ptr, new IL2String(noLabel).ptr, GetArgsPtr(args), new IntPtr(&allowCancel),
                new IntPtr(&yesButtonType)
            });
        }

        public static void OpenItemConfirmDialog(
            string title,
            string message,
            int itemId,
            Action action,
            string buttonLabel = null,
            string itemMessage = null,
            Dictionary<string, object> args = null)
        {
            methodOpenItemConfirmDialog1.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr, new IL2String(message).ptr, new IntPtr(&itemId),
                UnityEngine.Events._UnityAction.CreateAction(action),
                new IL2String(buttonLabel).ptr, new IL2String(itemMessage).ptr, GetArgsPtr(args),
            });
        }

        public static void OpenItemConfirmDialog(
            string title,
            string message,
            bool isPeriod,
            int itemCategory,
            int itemId,
            Action action,
            string buttonLabel = null,
            string itemMessage = null,
            Dictionary<string, object> args = null)
        {
            methodOpenItemConfirmDialog2.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr, new IL2String(message).ptr,
                new IntPtr(&isPeriod), new IntPtr(&itemCategory), new IntPtr(&itemId),
                UnityEngine.Events._UnityAction.CreateAction(action),
                new IL2String(buttonLabel).ptr, new IL2String(itemMessage).ptr, GetArgsPtr(args),
            });
        }

        // TODO
        /*public static void OpenCheckBoxDialog(
            string title,
            string message,
            List<EntryCheckBoxListData.EntryCheckBoxData> checkBoxList,
            bool isEnableMulti,
            Action<List<bool>> action,
            Action noAction,
            string buttonLabel,
            string noButtonLabel,
            bool interactable = false,
            Dictionary<string, object> args)
        {
            methodOpenCheckBoxDialog.Invoke(new IntPtr[]
            {
                ...
            });
        }*/
    }
}

namespace YgomGame.Dialog.CommonDialog
{
    unsafe static class CommonDialogTitleWidget
    {
        public enum IconType
        {
            None,
            Notice,
            Alert
        }
    }

    unsafe static class CommonDialogButtonGroupWidget
    {
        public enum ButtonType
        {
            Positive,
            Destructive,
            Disable,
            Highlight
        }
    }
}
using IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YgoMasterClient;

namespace YgomGame.Menu
{
    unsafe static class ActionSheetViewController
    {
        static IL2Method methodOpen;

        static ActionSheetViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("ActionSheetViewController", "YgomGame.Menu");
            methodOpen = classInfo.GetMethod("Open", x => x.GetParameters().Length == 3);
        }

        public static void Open(string title, string[] entries, Action<IntPtr, int> callback)
        {
            IL2Class stringClass = typeof(string).GetClass();
            IL2ListExplicit entriesList = new IL2ListExplicit(IntPtr.Zero, stringClass, true);
            foreach (string str in entries)
            {
                entriesList.Add(new IL2String(str).ptr);
            }
            IntPtr entriesReadOnlyList = entriesList.MethodAsReadOnly();

            methodOpen.Invoke(new IntPtr[]
            {
                new IL2String(title).ptr,
                entriesReadOnlyList,
                UnityEngine.Events._UnityAction.CreateAction<int>(callback)
            });
        }
    }
}

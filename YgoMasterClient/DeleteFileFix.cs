using IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMasterClient
{
    /// <summary>
    /// Prevents the client from deleting files when LocalSave doesn't exist
    /// </summary>
    unsafe static class DeleteFileFix
    {
        delegate void Del_DeleteFile();
        static Hook<Del_DeleteFile> hookDLCList_DeleteFile;
        static Hook<Del_DeleteFile> hookDLCVersion_DeleteFile;

        static DeleteFileFix()
        {
            if (Program.IsLive)
            {
                return;
            }

            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");

            IL2Class dlcListClass = assembly.GetClass("DLCList", "YgomGame.Download");
            hookDLCList_DeleteFile = new Hook<Del_DeleteFile>(DeleteFile, dlcListClass.GetMethod("Delete"));

            IL2Class dlcVersionClass = assembly.GetClass("DLCVersion", "YgomGame.Download");
            hookDLCVersion_DeleteFile = new Hook<Del_DeleteFile>(DeleteFile, dlcVersionClass.GetMethod("Delete"));
        }

        static void DeleteFile()
        {
        }
    }
}

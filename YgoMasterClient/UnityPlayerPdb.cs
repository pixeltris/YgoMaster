using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

// https://github.com/adrianstone55/SymbolSort/blob/master/SymbolSort.cs <-- enumerating symbols
// https://github.com/microsoft/DirectXShaderCompiler/blob/main/tools/clang/tools/dotnetc/dia2.cs <-- interfaces
// https://github.com/CloudIDEaaS/hydra/tree/master/VisualStudioProvider/PDB/raw <-- more interfaces
// https://github.com/microsoft/binskim/blob/main/src/BinaryParsers/PEBinary/ProgramDatabase/MSDiaComWrapper.cs <-- load DIA dynamically

namespace YgoMasterClient
{
    static class UnityPlayerPdb
    {
        // Lastest UnityPlayer.dll build for Master Duel (as of 2024-05-07):
        // https://symbolserver.unity3d.com/UnityPlayer_Win64_player_il2cpp_x64.pdb/D7818DF19E8C487798AAA1D28C0B17A61/

        // Ways to get the PDB GUID:
        // - Check the symbol info in Visual Studio (attach to process, view modules window and get symbol info from UnityPlayer.dll)
        // - Load UnityPlayer.dll in x64dbg, view modules, view the log for where it tried to load symbols
        // - dumpbin UnityPlayer.dll /headers > headers.txt (ctrl+f for .pdb, GUID is to the left of that)

        // You can look up mangled functions with the x64dbg modules view (the mangled names are partial function names in many cases)
        static Dictionary<string, SymbolInfo> targetFunctions = new Dictionary<string, SymbolInfo>()
        {
            { "?AudioClip_CUSTOM_Construct_Internal@@YAPEAVScriptingBackendNativeObjectPtrOpaque@@XZ", new SymbolInfo("UnityPlayerRVA_AudioClip_CUSTOM_Construct_Internal") },
            { "?AudioClip_CUSTOM_CreateUserSound@@YAXPEAVScriptingBackendNativeObjectPtrOpaque@@PEAVScriptingBackendNativeStringPtrOpaque@@HHHE@Z", new SymbolInfo("UnityPlayerRVA_AudioClip_CUSTOM_CreateUserSound") },
            { "?AudioClip_CUSTOM_SetData@@YAEPEAVScriptingBackendNativeObjectPtrOpaque@@PEAVScriptingBackendNativeArrayPtrOpaque@@HH@Z", new SymbolInfo("UnityPlayerRVA_AudioClip_CUSTOM_SetData") },
            { "?DownloadHandlerTexture_CUSTOM_Create@@YAPEAXPEAVScriptingBackendNativeObjectPtrOpaque@@E@Z", new SymbolInfo("UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_Create") },
            { "?DownloadHandlerTexture_CUSTOM_InternalGetTextureNative@@YAPEAVScriptingBackendNativeObjectPtrOpaque@@PEAV1@@Z", new SymbolInfo("UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_InternalGetTextureNative") },
        };
        class SymbolInfo
        {
            public string SettingName;
            public uint RVA;

            public SymbolInfo(string settingName)
            {
                SettingName = settingName;
            }
        }

        public static void Update()
        {
            string pdbFile = "UnityPlayer_Win64_player_il2cpp_x64.pdb";
            if (!File.Exists(pdbFile))
            {
                Console.WriteLine("Couldn't find '" + pdbFile + "'");
                return;
            }

            Console.WriteLine("Loading PDB...");
            IDiaDataSource dataSource = MsdiaComWrapper.GetDiaSource();
            dataSource.loadDataFromPdb(pdbFile);
            IDiaSession session = dataSource.openSession();
            IDiaSymbol globalScope = session.get_globalScope();

            Console.WriteLine("Reading section info...");
            List<IDiaSectionContrib> sectionContribs = new List<IDiaSectionContrib>();
            BuildSectionContribTable(session, sectionContribs);

            Console.WriteLine("Reading public symbols... ");
            ReadSymbolsFromScope(globalScope, SymTagEnum.SymTagPublicSymbol, session, sectionContribs);

            StringBuilder sb = new StringBuilder();
            foreach (SymbolInfo symbol in targetFunctions.Values)
            {
                sb.AppendLine("    \"" + symbol.SettingName + "\": " + symbol.RVA + ",");
            }
            string filename = "UnityPlayerRVA.json";
            File.WriteAllText(filename, sb.ToString());
            Console.WriteLine("Generated '" + filename + "'");
        }

        private static void BuildSectionContribTable(IDiaSession session, List<IDiaSectionContrib> sectionContribs)
        {
            IDiaEnumSectionContribs enumSectionContribs = GetEnumSectionContribs(session);
            if (enumSectionContribs != null)
            {
                while (true)
                {
                    uint numFetched = 1;
                    IDiaSectionContrib diaSectionContrib;
                    enumSectionContribs.Next(numFetched, out diaSectionContrib, out numFetched);
                    if (diaSectionContrib == null || numFetched < 1)
                        break;

                    sectionContribs.Add(diaSectionContrib);

                }
            }
            sectionContribs.Sort(delegate (IDiaSectionContrib s0, IDiaSectionContrib s1)
            {
                return (int)s0.get_relativeVirtualAddress() - (int)s1.get_relativeVirtualAddress();
            });
        }

        private static IDiaEnumSectionContribs GetEnumSectionContribs(IDiaSession session)
        {
            IDiaEnumTables tableEnum = session.getEnumTables();
            while (true)
            {
                uint numFetched = 1;
                IDiaTable table = null;
                tableEnum.Next(numFetched, ref table, ref numFetched);
                if (table == null || numFetched < 1)
                    break;
                if (table is IDiaEnumSectionContribs)
                    return table as IDiaEnumSectionContribs;
            }
            return null;
        }

        private static void ReadSymbolsFromScope(IDiaSymbol parent, SymTagEnum type, IDiaSession diaSession, List<IDiaSectionContrib> sectionContribs)
        {
            IDiaEnumSymbols enumSymbols;
            parent.findChildren(type, null, 0, out enumSymbols);

            while (true)
            {
                uint numFetched = 1;
                IDiaSymbol diaSymbol;
                enumSymbols.Next(numFetched, out diaSymbol, out numFetched);
                if (diaSymbol == null || numFetched < 1)
                    break;

                if ((LocationType)diaSymbol.get_locationType() != LocationType.LocIsStatic)
                    continue;

                if (type == SymTagEnum.SymTagData)
                {
                    if (diaSymbol.get_type() == null)
                        continue;
                }
                else
                {
                    if (diaSymbol.get_length() == 0)
                        continue;
                }

                string name = diaSymbol.get_name();
                SymbolInfo symbolInfo;
                if (!string.IsNullOrEmpty(name) && targetFunctions.TryGetValue(name, out symbolInfo))
                {
                    symbolInfo.RVA = diaSymbol.get_relativeVirtualAddress();
                }
            }
        }

        class MsdiaComWrapper
        {
            delegate int Del_DllGetClassObject([In, MarshalAs(UnmanagedType.LPStruct)] Guid ClassId, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr ppvObject);
            static Del_DllGetClassObject DllGetClassObject;

            private static void CoCreateFromMsdia(Guid clsidOfServer, Guid riid, out IntPtr pvObject)
            {
                if (DllGetClassObject == null)
                {
                    pvObject = IntPtr.Zero;
                    try
                    {
                        string vsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft Visual Studio");
                        if (Directory.Exists(vsPath))
                        {
                            foreach (string subDir in Directory.GetDirectories(vsPath))
                            {
                                string diaSdkPath = Path.Combine(subDir, "Community", "DIA SDK", "bin", "amd64");
                                if (Directory.Exists(diaSdkPath))
                                {
                                    string[] dlls = Directory.GetFiles(diaSdkPath, "msdia*");// msdia140.dll
                                    if (dlls.Length > 0)
                                    {
                                        IntPtr handle = PInvoke.LoadLibrary(dlls[0]);
                                        if (handle != IntPtr.Zero)
                                        {
                                            IntPtr func = PInvoke.GetProcAddress(handle, "DllGetClassObject");
                                            if (func != IntPtr.Zero)
                                            {
                                                DllGetClassObject = (Del_DllGetClassObject)Marshal.GetDelegateForFunctionPointer(func, typeof(Del_DllGetClassObject));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                    if (DllGetClassObject == null)
                    {
                        return;
                    }
                }

                IntPtr pClassFactory = IntPtr.Zero;
                int hr = DllGetClassObject(clsidOfServer, new Guid("00000001-0000-0000-C000-000000000046"), out pClassFactory);
                if (hr != 0)
                {
                    throw new InvalidOperationException("Could not get class object.");
                }
                var classFactory = (IClassFactory)Marshal.GetObjectForIUnknown(pClassFactory);
                classFactory.CreateInstance(IntPtr.Zero, ref riid, out pvObject);
                Marshal.Release(pClassFactory);
                Marshal.ReleaseComObject(classFactory);
            }

            private const string IDiaDataSourceRiid = "79F1BB5F-B66E-48E5-B6A9-1545C323CA3D";
            private const string DiaSourceClsid = "E6756135-1E65-4D17-8576-610761398C3C";

            public static IDiaDataSource GetDiaSource()
            {
                IntPtr diaSourcePtr = IntPtr.Zero;
                CoCreateFromMsdia(new Guid(DiaSourceClsid), new Guid(IDiaDataSourceRiid), out diaSourcePtr);
                if (diaSourcePtr == IntPtr.Zero)
                {
                    return null;
                }
                object objectForIUnknown = Marshal.GetObjectForIUnknown(diaSourcePtr);
                var diaSourceInstance = objectForIUnknown as IDiaDataSource;
                return diaSourceInstance;
            }
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000001-0000-0000-C000-000000000046")]
        interface IClassFactory
        {
            [PreserveSig]
            int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);
        }

        [ComImport]
        [Guid("79F1BB5F-B66E-48e5-B6A9-1545C323CA3D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDiaDataSource
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_lastError();
            void loadDataFromPdb(string path);
            void loadAndValidateDataFromPdb(string path, ref Guid pcsig70, UInt32 sig, UInt32 age);
            void loadDataForExe(string executable, string searchPath, [MarshalAs(UnmanagedType.IUnknown)] object pCallback);
            void loadDataFromIStream(IStream pIStream);
            IDiaSession openSession();
        }

        [ComImport]
        [Guid("2F609EE1-D1C8-4E24-8288-3326BADCD211")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDiaSession
        {
            UInt64 get_loadAddress();
            void put_loadAddress(UInt64 value);
            IDiaSymbol get_globalScope();
            IDiaEnumTables getEnumTables();
            void getSymbolsByAddr();
            void findChildren();
            void findChildrenEx();
            void findChildrenExByAddr();
            void findChildrenExByVA();
            void findChildrenExByRVA();
            void findSymbolByAddr();
            void findSymbolByRVA();
            void findSymbolByVA();
            void findSymbolByToken();
            void symsAreEquiv();
            void symbolById();
            void findSymbolByRVAEx();
            void findSymbolByVAEx();
            void findFile();
            void findFileById();
            void findLines();
            void findLinesByAddr();
            void findLinesByRVA();
            void findLinesByVA();
            void findLinesByLinenum();
            object /*IDiaEnumInjectedSources*/ findInjectedSource(string srcFile);
            object /*IDiaEnumDebugStreams*/ getEnumDebugStreams();
            void findInlineFramesByAddr();
            void findInlineFramesByRVA();
            void findInlineFramesByVA();
            void findInlineeLines();
            void findInlineeLinesByAddr();
            void findInlineeLinesByRVA();
            void findInlineeLinesByVA();
            void findInlineeLinesByLinenum();
            void findInlineesByName();
            void findAcceleratorInlineeLinesByLinenum();
            void findSymbolsForAcceleratorPointerTag();
            void findSymbolsByRVAForAcceleratorPointerTag();
            void findAcceleratorInlineesByName();
            void addressForVA();
            void addressForRVA();
            void findILOffsetsByAddr();
            void findILOffsetsByRVA();
            void findILOffsetsByVA();
            void findInputAssemblyFiles();
            void findInputAssembly();
            void findInputAssemblyById();
            void getFuncMDTokenMapSize();
            void getFuncMDTokenMap();
            void getTypeMDTokenMapSize();
            void getTypeMDTokenMap();
            void getNumberOfFunctionFragments_VA();
            void getNumberOfFunctionFragments_RVA();
            void getFunctionFragments_VA();
            void getFunctionFragments_RVA();
            object /*IDiaEnumSymbols*/ getExports();
            object /*IDiaEnumSymbols*/ getHeapAllocationSites();
            void findInputAssemblyFile();
        }

        [ComImport]
        [Guid("cb787b2f-bd6c-4635-ba52-933126bd2dcd")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDiaSymbol
        {
            UInt32 get_symIndexId();
            UInt32 get_symTag();
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_name();
            IDiaSymbol get_lexicalParent();
            IDiaSymbol get_classParent();
            IDiaSymbol get_type();
            UInt32 get_dataKind();
            UInt32 get_locationType();
            UInt32 get_addressSection();
            UInt32 get_addressOffset();
            UInt32 get_relativeVirtualAddress();
            UInt64 get_virtualAddress();
            UInt32 get_registerId();
            Int32 get_offset();
            UInt64 get_length();
            UInt32 get_slot();
            bool get_volatileType();
            bool get_constType();
            bool get_unalignedType();
            UInt32 get_access();
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_libraryName();
            UInt32 get_platform();
            UInt32 get_language();
            bool get_editAndContinueEnabled();
            UInt32 get_frontEndMajor();
            UInt32 get_frontEndMinor();
            UInt32 get_frontEndBuild();
            UInt32 get_backEndMajor();
            UInt32 get_backEndMinor();
            UInt32 get_backEndBuild();
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_sourceFileName();
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_unused();
            UInt32 get_thunkOrdinal();
            Int32 get_thisAdjust();
            UInt32 get_virtualBaseOffset();
            bool get_virtual();
            bool get_intro();
            bool get_pure();
            UInt32 get_callingConvention();
            [return: MarshalAs(UnmanagedType.AsAny)] // VARIANT
            object get_value();
            UInt32 get_baseType();
            UInt32 get_token();
            UInt32 get_timeStamp();
            Guid get_guid();
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_symbolsFileName();
            bool get_reference();
            UInt32 get_count();
            UInt32 get_bitPosition();
            IDiaSymbol get_arrayIndexType();
            bool get_packed();
            bool get_constructor();
            bool get_overloadedOperator();
            bool get_nested();
            bool get_hasNestedTypes();
            bool get_hasAssignmentOperator();
            bool get_hasCastOperator();
            bool get_scoped();
            bool get_virtualBaseClass();
            bool get_indirectVirtualBaseClass();
            Int32 get_virtualBasePointerOffset();
            IDiaSymbol get_virtualTableShape();
            UInt32 get_lexicalParentId();
            UInt32 get_classParentId();
            UInt32 get_typeId();
            UInt32 get_arrayIndexTypeId();
            UInt32 get_virtualTableShapeId();
            bool get_code();
            bool get_function();
            bool get_managed();
            bool get_msil();
            UInt32 get_virtualBaseDispIndex();
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_undecoratedName();
            UInt32 get_age();
            UInt32 get_signature();
            bool get_compilerGenerated();
            bool get_addressTaken();
            UInt32 get_rank();
            IDiaSymbol get_lowerBound();
            IDiaSymbol get_upperBound();
            UInt32 get_lowerBoundId();
            UInt32 get_upperBoundId();
            void get_dataBytes(UInt32 cbData, out UInt32 pcbData, out byte[] pbData);
            void findChildren(SymTagEnum symtag, string name, uint compareFlags, out IDiaEnumSymbols ppResult);
            void findChildrenEx();
            void findChildrenExByAddr();
            void findChildrenExByVA();
            void findChildrenExByRVA();
            UInt32 get_targetSection();
            UInt32 get_targetOffset();
            UInt32 get_targetRelativeVirtualAddress();
            UInt64 get_targetVirtualAddress();
            UInt32 get_machineType();
            UInt32 get_oemId();
            UInt32 get_oemSymbolId();
            void get_types();
            void get_typeIds();
            IDiaSymbol get_objectPointerType();
            UInt32 get_udtKind();
            void get_undecoratedNameEx();
            bool get_noReturn();
            bool get_customCallingConvention();
            bool get_noInline();
            bool get_optimizedCodeDebugInfo();
            bool get_notReached();
            bool get_interruptReturn();
            bool get_farReturn();
            bool get_isStatic();
            bool get_hasDebugInfo();
            bool get_isLTCG();
            bool get_isDataAligned();
            bool get_hasSecurityChecks();
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_compilerName();
            bool get_hasAlloca();
            bool get_hasSetJump();
            bool get_hasLongJump();
            bool get_hasInlAsm();
            bool get_hasEH();
            bool get_hasSEH();
            bool get_hasEHa();
            bool get_isNaked();
            bool get_isAggregated();
            bool get_isSplitted();
            IDiaSymbol get_container();
            bool get_inlSpec();
            bool get_noStackOrdering();
            IDiaSymbol get_virtualBaseTableType();
            bool get_hasManagedCode();
            bool get_isHotpatchable();
            bool get_isCVTCIL();
            bool get_isMSILNetmodule();
            bool get_isCTypes();
            bool get_isStripped();
            UInt32 get_frontEndQFE();
            UInt32 get_backEndQFE();
            bool get_wasInlined();
            bool get_strictGSCheck();
            bool get_isCxxReturnUdt();
            bool get_isConstructorVirtualBase();
            bool get_RValueReference();
            IDiaSymbol get_unmodifiedType();
            bool get_framePointerPresent();
            bool get_isSafeBuffers();
            bool get_intrinsic();
            bool get_sealed();
            bool get_hfaFloat();
            bool get_hfaDouble();
            UInt32 get_liveRangeStartAddressSection();
            UInt32 get_liveRangeStartAddressOffset();
            UInt32 get_liveRangeStartRelativeVirtualAddress();
            UInt32 get_countLiveRanges();
            UInt64 get_liveRangeLength();
            UInt32 get_offsetInUdt();
            UInt32 get_paramBasePointerRegisterId();
            UInt32 get_localBasePointerRegisterId();
            bool get_isLocationControlFlowDependent();
            UInt32 get_stride();
            UInt32 get_numberOfRows();
            UInt32 get_numberOfColumns();
            bool get_isMatrixRowMajor();
            void get_numericProperties();
            void get_modifierValues();
            bool get_isReturnValue();
            bool get_isOptimizedAway();
            UInt32 get_builtInKind();
            UInt32 get_registerType();
            UInt32 get_baseDataSlot();
            UInt32 get_baseDataOffset();
            UInt32 get_textureSlot();
            UInt32 get_samplerSlot();
            UInt32 get_uavSlot();
            UInt32 get_sizeInUdt();
            UInt32 get_memorySpaceKind();
            UInt32 get_unmodifiedTypeId();
            UInt32 get_subTypeId();
            IDiaSymbol get_subType();
            UInt32 get_numberOfModifiers();
            UInt32 get_numberOfRegisterIndices();
            bool get_isHLSLData();
            bool get_isPointerToDataMember();
            bool get_isPointerToMemberFunction();
            bool get_isSingleInheritance();
            bool get_isMultipleInheritance();
            bool get_isVirtualInheritance();
            bool get_restrictedType();
            bool get_isPointerBasedOnSymbolValue();
            IDiaSymbol get_baseSymbol();
            UInt32 get_baseSymbolId();
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_objectFileName();
            bool get_isAcceleratorGroupSharedLocal();
            bool get_isAcceleratorPointerTagLiveRange();
            bool get_isAcceleratorStubFunction();
            UInt32 get_numberOfAcceleratorPointerTags();
            bool get_isSdl();
            bool get_isWinRTPointer();
            bool get_isRefUdt();
            bool get_isValueUdt();
            bool get_isInterfaceUdt();
            void findInlineFramesByAddr();
            void findInlineFramesByRVA();
            void findInlineFramesByVA();
            void findInlineeLines();
            void findInlineeLinesByAddr();
            void findInlineeLinesByRVA();
            void findInlineeLinesByVA();
            void findSymbolsForAcceleratorPointerTag();
            void findSymbolsByRVAForAcceleratorPointerTag();
            void get_acceleratorPointerTags();
            void getSrcLineOnTypeDefn();
            bool get_isPGO();
            bool get_hasValidPGOCounts();
            bool get_isOptimizedForSpeed();
            UInt32 get_PGOEntryCount();
            UInt32 get_PGOEdgeCount();
            UInt64 get_PGODynamicInstructionCount();
            UInt32 get_staticSize();
            UInt32 get_finalLiveStaticSize();
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_phaseName();
            bool get_hasControlFlowCheck();
            bool get_constantExport();
            bool get_dataExport();
            bool get_privateExport();
            bool get_noNameExport();
            bool get_exportHasExplicitlyAssignedOrdinal();
            bool get_exportIsForwarder();
            UInt32 get_ordinal();
            UInt32 get_frameSize();
            UInt32 get_exceptionHandlerAddressSection();
            UInt32 get_exceptionHandlerAddressOffset();
            UInt32 get_exceptionHandlerRelativeVirtualAddress();
            UInt64 get_exceptionHandlerVirtualAddress();
            void findInputAssemblyFile();
            UInt32 get_characteristics();
            IDiaSymbol get_coffGroup();
            UInt32 get_bindID();
            UInt32 get_bindSpace();
            UInt32 get_bindSlot();
        }

        [ComImport]
        [Guid("C65C2B0A-1150-4d7a-AFCC-E05BF3DEE81E")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDiaEnumTables
        {
            int get__NewEnum(out IntPtr pRetVal);
            uint get_Count();
            int Item(object index, ref IDiaTable table);
            int Next(uint celt, ref IDiaTable rgelt, ref uint pceltFetched);
            int Skip(uint celt);
            int Reset();
            int Clone(out IDiaEnumTables ppenum);
        }

        [ComImport, Guid("1994DEB2-2C82-4B1D-A57F-AFF424D54A68"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDiaEnumSectionContribs
        {
            int get__NewEnum(out IntPtr pRetVal);
            uint get_Count();
            IDiaSectionContrib Item(uint index);
            void Next(uint celt, out IDiaSectionContrib rgelt, out uint pceltFetched);
            void Skip(uint celt);
            void Reset();
            void Clone(out IDiaEnumSectionContribs ppenum);
        }

        [ComImport]
        [Guid("0CF4B60E-35B1-4c6c-BDD8-854B9C8E3857")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDiaSectionContrib
        {
            IDiaSymbol get_compiland();
            uint get_addressSection();
            uint get_addressOffset();
            uint get_relativeVirtualAddress();
            UInt64 get_virtualAddress();
            uint get_length();
            bool get_notPaged();
            bool get_code();
            bool get_initializedData();
            bool get_uninitializedData();
            bool get_remove();
            bool get_comdat();
            bool get_discardable();
            bool get_notCached();
            bool get_share();
            bool get_execute();
            bool get_read();
            bool get_write();
            uint get_dataCrc();
            uint get_relocationsCrc();
            uint get_compilandId();
            bool get_code16bit();
        }

        [ComImport]
        [Guid("4A59FB77-ABAC-469b-A30B-9ECC85BFEF14")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDiaTable // : IEnumUnknown - need to replay vtable
        {
            UInt32 Next(UInt32 count,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 2)]
            ref object[] tables, out UInt32 fetched);
            void Skip(UInt32 count);
            void Reset();
            IntPtr Clone();

            [return: MarshalAs(UnmanagedType.IUnknown)]
            object get__NewEnum();
            [return: MarshalAs(UnmanagedType.BStr)]
            string get_name();
            Int32 get_Count();
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object Item(UInt32 index);
        }

        [ComImport, Guid("CAB72C48-443B-48f5-9B0B-42F0820AB29A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDiaEnumSymbols
        {
            int get__NewEnum(out IntPtr pRetVal);
            int get_Count();
            int Item(object index, out IDiaSymbol table);
            int Next(uint celt, out IDiaSymbol rgelt, out uint pceltFetched);
            int Skip(uint celt);
            int Reset();
            int Clone(out IDiaEnumTables ppenum);
        }

        enum SymTagEnum
        {
            SymTagNull,
            SymTagExe,
            SymTagCompiland,
            SymTagCompilandDetails,
            SymTagCompilandEnv,
            SymTagFunction,
            SymTagBlock,
            SymTagData,
            SymTagAnnotation,
            SymTagLabel,
            SymTagPublicSymbol,
            SymTagUDT,
            SymTagEnum,
            SymTagFunctionType,
            SymTagPointerType,
            SymTagArrayType,
            SymTagBaseType,
            SymTagTypedef,
            SymTagBaseClass,
            SymTagFriend,
            SymTagFunctionArgType,
            SymTagFuncDebugStart,
            SymTagFuncDebugEnd,
            SymTagUsingNamespace,
            SymTagVTableShape,
            SymTagVTable,
            SymTagCustom,
            SymTagThunk,
            SymTagCustomType,
            SymTagManagedType,
            SymTagDimension,
            SymTagCallSite,
            SymTagInlineSite,
            SymTagBaseInterface,
            SymTagVectorType,
            SymTagMatrixType,
            SymTagHLSLType,
            SymTagCaller,
            SymTagCallee,
            SymTagExport,
            SymTagHeapAllocationSite,
            SymTagCoffGroup,
            SymTagMax
        }

        enum LocationType
        {
            LocIsNull,
            LocIsStatic,
            LocIsTLS,
            LocIsRegRel,
            LocIsThisRel,
            LocIsEnregistered,
            LocIsBitField,
            LocIsSlot,
            LocIsIlRel,
            LocInMetaData,
            LocIsConstant,
            LocTypeMax
        }
    }
}
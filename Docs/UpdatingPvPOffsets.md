## Updating PvP offsets (Settings.json - MultiplayerPvpClientDoCommandUserOffset / MultiplayerPvpClientRunDialogUserOffset)

- Download x64dbg
- Open `Yu-Gi-Oh!  Master Duel/masterduel_Data/Plugins/x86_64/duel.dll` in x64dbg
- Press the "Run" button until duel.dll is loaded into x64dbg
- Click View -> Modules -> select duel.dll on the left panel
- On the right panel there will be a list of DLL_ functions. You can filter and double click these functions to go to the code
- You are interested in DLL_DuelListInitString / DLL_DUELCOMGetRecommendSide
- Locate the respective MultiplayerPvpClientDoCommandUserOffset / MultiplayerPvpClientRunDialogUserOffset from the disassembly and convert the hexadecimal value to decimal (a regular number) (3C28 = 15400) (3C90 = 15504)

```
DLL_DuelListInitString
Header RVA: 0x0000000180CEDEC0 (Master Duel v2.5.0)
00007FF9B629DEC0 | 48:83EC 28                    | sub rsp,28                               |
00007FF9B629DEC4 | 4C:8B05 F5CD3000              | mov r8,qword ptr ds:[7FF9B65AACC0]       |
00007FF9B629DECB | 41:0FBF50 02                  | movsx edx,word ptr ds:[r8+2]             |
00007FF9B629DED0 | 44:8BCA                       | mov r9d,edx                              |
00007FF9B629DED3 | 8D42 FF                       | lea eax,qword ptr ds:[rdx-1]             |
00007FF9B629DED6 | 3D 88000000                   | cmp eax,88                               |
00007FF9B629DEDB | 0F87 B2000000                 | ja duel-2-5-0.7FF9B629DF93               |
00007FF9B629DEE1 | 4C:8D15 182131FF              | lea r10,qword ptr ds:[7FF9B55B0000]      |
00007FF9B629DEE8 | 48:98                         | cdqe                                     |
00007FF9B629DEEA | 41:0FB68402 80E0CE00          | movzx eax,byte ptr ds:[r10+rax+CEE080]   |
00007FF9B629DEF3 | 41:8B8C82 70E0CE00            | mov ecx,dword ptr ds:[r10+rax*4+CEE070]  |
00007FF9B629DEFB | 49:03CA                       | add rcx,r10                              |
00007FF9B629DEFE | FFE1                          | jmp rcx                                  |
00007FF9B629DF00 | 41:0FB600                     | movzx eax,byte ptr ds:[r8]               |
00007FF9B629DF04 | 33C9                          | xor ecx,ecx                              |
00007FF9B629DF06 | 48:8B15 D3CD3000              | mov rdx,qword ptr ds:[7FF9B65AACE0]      |
00007FF9B629DF0D | 3982 283C0000                 | cmp dword ptr ds:[rdx+3C28],eax          | <----- MultiplayerPvpClientRunDialogUserOffset
00007FF9B629DF13 | C782 303C0000 00000000        | mov dword ptr ds:[rdx+3C30],0            |
00007FF9B629DF1D | 0F95C1                        | setne cl                                 |
00007FF9B629DF20 | 81C1 44020000                 | add ecx,244                              |
00007FF9B629DF26 | 42:8D0449                     | lea eax,qword ptr ds:[rcx+r9*2]          |
...
```

```
DLL_DUELCOMGetRecommendSide
Header RVA: 0x0000000180CEE440 (Master Duel v2.5.0)
00007FF9B629E440 | 48:8B05 99C83000              | mov rax,qword ptr ds:[7FF9B65AACE0]      |
00007FF9B629E447 | 83B8 603C0000 07              | cmp dword ptr ds:[rax+3C60],7            |
00007FF9B629E44E | 75 07                         | jne duel-2-5-0.7FF9B629E457              |
00007FF9B629E450 | 8B80 903C0000                 | mov eax,dword ptr ds:[rax+3C90]          | <----- MultiplayerPvpClientDoCommandUserOffset
00007FF9B629E456 | C3                            | ret                                      |
00007FF9B629E457 | 8B90 7C3C0000                 | mov edx,dword ptr ds:[rax+3C7C]          |
00007FF9B629E45D | 8B88 903C0000                 | mov ecx,dword ptr ds:[rax+3C90]          |
00007FF9B629E463 | E9 581880FF                   | jmp duel-2-5-0.7FF9B5A9FCC0              |
```
This currently isn't public but this is how YgoMasterUpdater is used to update YgoMaster:

- Create a new steam account
- Place YgoMasterUpdater folder into Master Duel folder
- Update YgoMasterUpdater/UpdaterSettings.json to include the new steam account username/password
- Run YgoMasterUpdater
- Manually update Docs/ReflectionDump.json based on Updating.md (client updates)
- Manually update Bgm.json
- Manually update Docs/AltCardsYdk.json (YgoMaster.exe --unknown-alt-cards) and run YgoMaster.exe --updateydk (if there are new alt arts)
- Create a release

## Loading errors

![Alt text](Pics/LoadError.png)

You may see some variation of the above error. This occurs because locale specific files are missing. Changing the language to match the language set via Steam should fix this issue.

If the issue persists follow these instructions:

- Navigate to `/Yu-Gi-Oh! Master Duel/LocalSave/`.
- There should be two folders in `/LocalSave/`; one with a bunch of random letters, and one called `00000000`.
- Delete the contents of the `00000000` folder, and copy the contents of the random letters folder into the `00000000` folder.

If this still doesn't fix the issue feel free to [create a new issue](https://github.com/pixeltris/YgoMaster/issues/new).

## Additional info

YgoMaster isn't able to act as a content delivery network (CDN) so it cannot provide the necessary download when switching languages. Therefore the language must be set to the same language as on Steam.

It's also possible that in some cases simply setting the language isn't enough. By copying the `/LocalSave/` folder you ensure that everything matches up to the settings used on Steam.
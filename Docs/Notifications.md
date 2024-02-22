# Menu Notifications

Home notifications are now available from v1.40. These are shown to the user on the main menu screen of the game and are refreshed everytime the menu is loaded.
![image](Pics/custom_notification1.jpg).

## Usage

The `Data/Notifications` folder is read by the game and will try to parse any json files places inside it.
There are certain Keys that are needed to be read from the json file to portray your message.

Currently the Order the files are read is also the order they appear inside of the game.

### Keys
- `Title` - String The main text that will be displayed on the image but not inside of the text when the notification is opened. You can also use color tags (*<color=#00eeeff>text will be colored goes here</ color>)*

- `Pattern` - String of the Type of Notification
 *(Usually "NOTIFY")*

- `Sort` - Integer to sort the value of the item from other.
 *(Note: This doesnt not seem to work at the moment.)*

- `BackImage` - String  - Path of the Image you want the background to have. *(e.g. "Images/Notification/System/Notice001")*

- `Contents` - This is a List of Dictionaries of the Text contents that you want to display when the notification is clicked.

To add Headers use the Following Template. (Replace Custom Header with the text you want to show)
	` { "tp": "H1", "text": { "en_US": "Custom Header" }`

To Show normal Text use the Following Template for. (Replace the Custom Message String with your message)
	`{ "tp": "Text", "text": { "en_US": "This is a Custom Message Read via JSON." }, "indent": -1 }`
	
  The Text item must have the locale or it will fail to display the text. There is no limit on what items you can have here. (See `Data/Notifications/YgoMaster.json` for reference.)

### Example JSON
```
{
    "Title": "Custom Message Here",
    "Pattern": NOTIFY,
    "Sort": 1591200,
    "BackImage": "Images/Notification/System/Gate001",
    "Contents": [
    {
        "tp": "H1",
        "text":
        {
            "en_US": "Custom Header"
        }
    },
    {
        "tp": "Text",
        "text":
        {
            "en_US": "This is a Custom Message Read via JSON."
        },
        "indent": -1
    }]
}


There is some basic functionality for giving CPU decks an Elo rating. Currently it's very CPU intensive and takes many hours. Idealy you want a threadripper (or similar high core CPU).

Create `/Data/CpuContest/ContestSettings.json` with the following settings:

```
{
    "instances": 12,
    "iterationsBeforeIdle": 100,
    "duelsPerDeck": 90
}
```

`instances` should match your CPU core count (or be slightly higher than it).

- Create `/Data/CpuContest/Decks` and copy all of the decks you want to rank into the folder.
- In `ClientSettings.json` set `ShowConsole` to `true` and run the client.
- In the client console run `carddata` which should create `/Data/ClientDataDump/Card/Data/{CLIENT_VERSION}/`, move and rename the `{CLIENT_VERSION}` folder to `/Data/CardData/`.
- Run `YgoMaster --cpucontest`.

This will take some hours to complete depending on how many decks you run. You should be able to close YgoMaster at any time and resume at a later date. At the end of it it'll produce `DecksByRating` which will copy the files from `Decks` and prefix the deck rating. Inside `DeckStats` it shows the individual wins / losses for each deck. And `Results.json` it'll list the decks and their ratings.

When you want to run the next contest delete `Results.json` and the `DeckStats` / `DecksByRatings` folders before starting again. Currently there isn't a way to introduce new decks to a set of existing rated decks (you need to re-run them from scratch each time).

For 284 decks at 90 duels per deck took me around 5 hours on a 12 core CPU. These were the resulting ratings for the decks (which were taken from the WC 2008 video game):

```
826 - Joey Wheeler - The Sacred Cards.json
845 - Gigobyte - The Weak Unite.json
853 - Dancing Fairy - Dancing Duel.json
857 - Elemental Hero Knospe - Pretty Plan.json
875 - Joey Wheeler - Reshef of Destruction.json
894 - Joey Wheeler - The Sacred Cards – Brainwashed.json
908 - Yubel - Light and Dark.json
912 - Stray Lambs - Like Two Heads.json
921 - Joey Wheeler - Dark Duel Stories.json
926 - Radiant Jeral - Draw No More.json
928 - Gorz the Emissary of Darkness - G-G-G-Gorz!.json
940 - Winged Kuriboh - Hello, Hero!.json
947 - Exodius the Ultimate Forbidden One - Forbidden Grave.json
953 - Vennominaga the Deity of Poisonous Snakes - Incurable Poison.json
955 - Green Guardian - Embust - Calm Spirits.json
960 - Yami Yugi - The Sacred Cards.json
982 - Ebon Magician Curran - Super Burn.json
995 - Jerry Beans Man - D.D. Homerun.json
1004 - Marcel Bonaparte - Lost Parts.json
1009 - Sonic Shooter - Powerful Twister.json
1017 - Alien Hypno - Contact Impact.json
1017 - Skull Servant - Grandpa's Deck.json
1019 - Dark Mimic LV3 - Mimic Panic.json
1020 - Lava Golem - Black Smoke.json
1020 - The Unhappy Maiden - Stop Fighting!.json
1021 - Maximillion Pegasus - Worldwide Edition Stairway to the Destined Duel.json
1029 - Underworld Guardian - Moley - Dark Legacy.json
1030 - Arcana Force Extra - The Light Ruler - Nightsong Crush.json
1033 - Chazz Princeton - Black Artifice.json
1033 - Dark Scorpion - Meanae the Thorn - You Ready.json
1046 - Joey Wheeler - The Eternal Duelist Soul.json
1049 - Reaper on the Nightmare - Perishing Darkfest.json
1050 - Harvest Angel of Wisdom - Shining Aitsu.json
1052 - Shell Guardian - Savan - Wanna Dance.json
1060 - Dark Mimic LV1 - Traveler's Trap.json
1063 - Nightmare Penguin - Sweat and Tears.json
1064 - Professor Thelonious Viper - Parasite of Light.json
1065 - Blazing Butterfly - Sinister Flame.json
1067 - VWXYZ-Dragon Catapult Cannon - Combined Orders.json
1070 - Different Dimension Dragon - Solid Power.json
1071 - Senju of the Thousand Hands - Counting Stars.json
1073 - Destiny Hero - Disk Commander - Earthbound Justice.json
1080 - White Magician Pikeru - Duelist Idol.json
1081 - The Legendary Fisherman - Ultimate Taste.json
1082 - Blast Sphere - Scared by Magic.json
1086 - Underworld Guardian - Moley - Dark Ordeal.json
1087 - Dark World Guardian - Gigori - Pitch-Dark Ordeal.json
1090 - Jesse Anderson - Whispering Gems.json
1094 - Maiden of the Aqua - Deluge.json
1095 - Kahkki, Guerilla of Dark World - Underworld.json
1096 - Mobius the Frost Monarch - Super Splash.json
1099 - Kairyu-Shin - Aquapolis.json
1099 - White Night Dragon - Blue-Eyes Light.json
1101 - Layard the Liberator - Shining Field.json
1104 - Molten Behemoth - Big Power.json
1104 - Spirit Reaper - Unstoppable Clock.json
1106 - Green Guardian - Embust - Open and Close.json
1107 - Armityle the Chaos Phantom - The Ultimate Being.json
1108 - St. Joan - Heavenly Blessing.json
1109 - Iron Blacksmith Kotetsu - Strong & Silent.json
1110 - Maximillion Pegasus - Future Vision.json
1112 - Ojama Yellow - Yellow Mischief.json
1112 - Volcanic Doomfire - Volcanic Eruption.json
1114 - Green Guardian - Embust - Silent Victory.json
1118 - Mech Bass - Water Hazard.json
1121 - Vampire Lady - Vampire Night.json
1123 - Warrior Lady of the Wasteland - Fighting Warriors.json
1125 - Otohime - Memory of the Sea.json
1126 - Dark World Guardian - Gigori - Dark Disturbance.json
1128 - Blowback Dragon - Locked On You.json
1129 - Vampire Lord - Vampire Feast.json
1131 - King of the Skull Servants - Rotten Spirits.json
1131 - Sky Guardian - Sefolile - Pitch Dark.json
1133 - Spirit of the Pharaoh - Young & Immortal.json
1135 - Marcel Bonaparte - D.D. Series.json
1142 - Gogiga Gagagigo - Is That a Fish.json
1147 - Amazoness Tiger - Rough and Ready.json
1147 - Decoy Dragon - Lock Down.json
1147 - The End of Anubis - Grand Protectors.json
1148 - Goldd, Wu-Lord of Dark World - Pure Gold.json
1148 - Speed Guardian - Ferrario - 10 karats.json
1149 - Marshmallon - Countdown Deck.json
1150 - Dark World Guardian - Gigori - Incoming Barrier.json
1150 - Gravekeeper's Commandant - Defense Ready!.json
1154 - Shell Guardian - Savan - Midnight Thunder.json
1154 - Underworld Guardian - Moley - Dark Bandits.json
1155 - Blazing Inpachi - Melody of Fire.json
1157 - Kabazauls - Today's Dino.json
1159 - Axel Brodie - Lava Explosion.json
1159 - Otohime - Otohime World.json
1159 - Sea Serpent Warrior of Darkness - Watery Soldier.json
1159 - Sillva, Warlord of Dark World - Special Silver.json
1160 - Green Guardian - Embust - Wiggling Beings.json
1161 - Kozaky - Draw.json
1161 - Perfect Machine King - New-Technology.json
1161 - Sky Guardian - Sefolile - Line Them Up.json
1162 - Don Zaloog - The Culprit's Here.json
1164 - Absorbing Kid from the Sky - Usual Shining.json
1164 - Alien Shocktrooper - Counter Culture.json
1165 - Jaden Yuki - E-Gate.json
1166 - Sand Moth - Rock Star.json
1172 - Rainbow Dragon - Over the Rainbow.json
1174 - Il Blud - Zombie Crazy.json
1175 - Bandit Keith - Everlasting Battery.json
1176 - Exxod, Master of The Guard - Solid Defense.json
1176 - Gogiga Gagagigo - Dream Splash.json
1177 - Jaden Yuki - Spiritual Guidance.json
1178 - Amazoness Chain Master - Call of Nature.json
1179 - Shell Guardian - Savan - Prohibited Trap.json
1180 - Dark World Guardian - Gigori - Dark Captor.json
1180 - Master of Oz - Aussie Deck.json
1180 - Vampire Lord - Age of Darkness.json
1181 - Machine King - Duel Machines.json
1182 - Curse of Vampire - The Curse.json
1182 - Dark Magician Girl - Magic School.json
1184 - Atticus Rhodes - Heaven Above.json
1184 - Elemental Hero Chaos Neos - Hero of Chaos.json
1185 - Curse of Vampire - Eternal Spirit.json
1185 - Harpie Girl - Monsoon Rider!.json
1185 - Ojama Green - Mischievous Trio.json
1185 - Sasuke Samurai - Swooping Sword.json
1186 - Amazoness Paladin - Female Warriors.json
1187 - Thunder Nyan Nyan - Bashful Maiden.json
1188 - Beast King Barbaros - Beast Beatdown.json
1188 - Yami Bakura - Forbidden Word.json
1192 - Speed Guardian - Ferrario - Like a Phoenix.json
1195 - Fox Fire - Fiery Chariot.json
1196 - Mokey Mokey - Mokey Mokey Trio.json
1197 - Seto Kaiba - Noble Soul.json
1198 - Dark Zane - Fortress.json
1198 - Neo-Spacian Dark Panther - Storm and Impulse.json
1198 - Royal Knight - Elite Shining.json
1198 - Stronghold the Moving Fortress - Follow the Symbol.json
1199 - Alien Infiltrator - Unidentified Space.json
1201 - Adrian Gecko - Cover the Sun.json
1201 - Kaiser Sea Horse - Natural Power.json
1203 - Yami Marik - Black Hole Force.json
1205 - Bastion Misawa - Equation of Love.json
1206 - Water Dragon - Undersea Shadows.json
1207 - Abyss Soldier - The Lost City.json
1208 - Mythical Beast Cerberus - Breath of Fire.json
1209 - Hino-Kagu-Tsuchi - Burning Warning.json
1209 - Joey Wheeler - Worldwide Edition Stairway to the Destined Duel – Brainwashed.json
1210 - Vanity's Ruler - Law of Victory.json
1212 - Satellite Cannon - Mecha Revolution.json
1212 - Yami Marik - Something Hidden.json
1214 - Adhesive Explosive - Afraid of the Dark.json
1214 - Dark Zane - Underworld Deck.json
1216 - Seto Kaiba - Ruinous Beast.json
1216 - Zure, Knight of Dark World - Dark Times.json
1217 - Alkana Knight Joker - A Warrior's Pride.json
1217 - Green Guardian - Embust - Silent Memory.json
1217 - Injection Fairy Lily - Maiden Honor.json
1217 - Jesse Anderson - Eternal Crystal.json
1217 - Yubel - D.D. Attendant.json
1219 - Maximillion Pegasus - World Championship Tournament 2004.json
1220 - Jasmine - Feeling Good!.json
1220 - Lich Lord, King of the Underworld - Zombie Defend.json
1222 - Maximillion Pegasus - The Eternal Duelist Soul.json
1224 - Destiny Hero - Plasma - Destiny and Blood.json
1224 - Guardian Angel Joan - Walking on Clouds.json
1224 - Watapon - Cute but Powerful.json
1225 - Shell Guardian - Savan - Back, You Beast!.json
1226 - Joey Wheeler - 7 Trials to Glory World Championship Tournament 2005 – Standard Deck.json
1227 - Aster Phoenix - Destiny Beatdown.json
1227 - Ocean Dragon Lord - Neo-Daedalus - Ocean Lord.json
1228 - Yami Bakura - Unlit Banquet.json
1229 - Speed Guardian - Ferrario - Bouncy Guardian.json
1231 - Underworld Guardian - Moley - Fiend's Trial.json
1232 - Luster Dragon - A Dragon's Way.json
1233 - Sasuke Samurai #2 - Intense Battler.json
1234 - Cyberdark Dragon - Armed Dragons.json
1234 - Tyranno Hassleberry - Dino Evolution.json
1237 - Yami Yugi - 7 Trials to Glory World Championship Tournament 2005.json
1238 - Great Angus - Let the Fire Burn!.json
1239 - Aquarian Alessa - Dripping Water.json
1242 - Freya, Spirit of Victory - Shining Bright.json
1242 - Mobius the Frost Monarch - Duel Under Water.json
1243 - Chrysalis Dolphin - Contact Impact.json
1244 - Gemini Elf - Get-a-long Sister.json
1244 - Mai Valentine - Bold Heroine.json
1245 - Joey Wheeler - 7 Trials to Glory World Championship Tournament 2005 – Advanced Deck.json
1246 - Alien Mars - Breakdown Attack.json
1247 - Vanity's Fiend - Triumphant Law.json
1248 - Alien Infiltrator - A-Type Gene.json
1249 - Elemental Mistress Doriado - Beautiful Tactics.json
1249 - Luster Dragon #2 - Dragonic Attack.json
1251 - Cloudian - Poison Cloud - All Wrapped Up.json
1252 - Elemental Hero Lady Heat - Enchanting Rhythm.json
1255 - Dark Dust Spirit - Impure Beings.json
1255 - Molten Zombie - Fan the Flames.json
1256 - Alien Shocktrooper - Unknown Intellect.json
1259 - Horus the Black Flame Dragon LV8 - Leveling Up.json
1259 - Underworld Guardian - Moley - The Dark Ones.json
1261 - Goe Goe the Gallant Ninja - Stand-alone Village.json
1264 - Alien Psychic - Abduction A.json
1265 - Spirit of the Pharaoh - A Pharaoh's Spirit.json
1266 - Dark World Guardian - Gigori - Fiend Mania.json
1266 - Joey Wheeler - Display of Courage.json
1270 - Sky Guardian - Sefolile - Good Old Strength.json
1273 - Alexis Rhodes - White Counselor.json
1273 - Petit Dragon - Everyday Deck.json
1275 - Shell Guardian - Savan - Prehistoric Drum.json
1279 - Syrus Truesdale - Roid Counterattack.json
1279 - Yami Yugi - 1,000 yr. Memories.json
1282 - Yami Yugi - Ties That Bind.json
1283 - Yami Bakura - RevivalTime.json
1285 - Gravekeeper's Chief - Gravekeeper's Deck.json
1285 - Marshmallon - Sky Gazer.json
1287 - Chazz Princeton - Deep Down Grit.json
1287 - Vampire Lady - Black Masquerade.json
1288 - Inpachi - Giant Lumber.json
1288 - Light Effigy - Flickering Flash.json
1288 - Yami Yugi - The Eternal Duelist Soul.json
1289 - Brron, Mad King of Dark World - Portable Darkness.json
1289 - Fenrir - Out of the Swamp!.json
1292 - Yugi Muto - Give You Courage.json
1294 - Gear Golem the Moving Fortress - Second Gear.json
1295 - Sky Guardian - Sefolile - Duel Responsive.json
1295 - Spirit of the Six Samurai - Samurai Mayhem.json
1295 - Woodborg Inpachi - Express Your Love.json
1298 - Brron, Mad King of Dark World - Sleepless Mind.json
1301 - Armed Samurai - Ben Kei - Start to Collapse.json
1306 - Vortex Kong - Power of Instinct.json
1307 - Mindy - Looking Good!.json
1308 - Alexis Rhodes - Fancy Tomorrow.json
1308 - Evil Hero Infernal Gainer - A Dark Goodbye.json
1308 - Kozaky - Goodbye Kozaky.json
1309 - Yami Yugi - World Championship 2004 - Almighty.json
1312 - Elemental Hero Neos Alius - A New World.json
1312 - Kaibaman - Blue Flame Servant.json
1313 - Sky Guardian - Sefolile - Monarch Deck.json
1314 - Submarineroid - Dueling Source.json
1316 - Great Shogun Shien - A Samurai's Life.json
1316 - Ishizu Ishtar - The Keepers.json
1316 - Tania - MightyWangfu.json
1317 - Yami Yugi - Worldwide Edition Stairway to the Destined Duel.json
1318 - Marie the Fallen One - Magical Temptation.json
1319 - Joey Wheeler - Ideal Partner.json
1321 - Gladiator Beast Heraklinos - Fighting Beast.json
1326 - Airknight Parshath - Flying Knight.json
1327 - Ojama Green - HighBurn.json
1328 - Harpie Lady 2 - Alluring Hunter.json
1328 - Joey Wheeler - Worldwide Edition Stairway to the Destined Duel.json
1330 - Mataza the Zapper - Inheritor of Justice.json
1330 - Sacred Phoenix of Nephthys - Eternal Phoenix.json
1333 - Metal Shooter - Laser Beam.json
1334 - Chancellor Sheppard - Master Sheppard.json
1337 - Mobius the Frost Monarch - F.G.E.json
1340 - Angel - HugoAdame.dek.json
1340 - Harpie Queen - Whirling Whirlwind!.json
1340 - Joey Wheeler - Reshef of Destruction – Brainwashed.json
1342 - Jaden Yuki - Dark Heroes.json
1343 - Soul of Purity and Light - Covered in Light.json
1346 - Tania - Amazoness Code.json
1347 - Dark Zane - Zane Truesdale.json
1350 - Lady Ninja Yae - Ninja Spy.json
1351 - Ancient Gear Gadjiltron Dragon - Ancient Mythology.json
1351 - Sabersaurus - Cretaceous Deck.json
1352 - Manju of the Ten Thousand Hands - Choose an Action.json
1355 - Silpheed - One-step Wind.json
1356 - White Magician Pikeru - CureBurn.json
1359 - Adrian Gecko - Cloudian.json
1360 - Bastion Misawa - WeakDrain.json
1364 - Bastion Misawa - Air Pressure.json
1366 - Cloudian - Poison Cloud - Freedom For All.json
1370 - Neo Space Pathfinder - Elemental Power.json
1371 - Giga Gagagigo - Gigagiga Heart.json
1372 - Cyber End Dragon - Cyber Diver.json
1372 - Joey Wheeler - World Championship Tournament 2004.json
1373 - Harpie Lady 1 - Winged Arrival.json
1382 - enyce - UG Ctr.json
1393 - D.D. Warrior Lady - Fear of D.D.json
1397 - HJY - Destiny.json
1397 - Woodborg Inpachi - Spirited Folkdance.json
1402 - Blowback Dragon - Gambling Addiction.json
1404 - Voltanis the Adjudicator - Arial Judgment.json
1407 - Sand Moth - My Kingdom.json
1409 - Evil Hero Malicious Edge - Dark Just for You.json
1424 - Yami Yugi - Reshef of Destruction.json
1426 - Marshmallon - T.G.E.json
1436 - Blowback Dragon - HighLevel.json
1478 - Green Gadget - The Annoying 3.json
1492 - Demise, King of Armageddon - Demise Ritual.json
```
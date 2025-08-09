using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        void Act_DeckGetDeckList(GameServerWebRequest request)
        {
            int deckId = Utils.GetValue<int>(request.ActParams, "deck_id");
            DeckInfo deck;
            if (request.Player.Decks.TryGetValue(deckId, out deck))
            {
                WriteDeckList(request, deckId);
            }
            request.Remove("DeckList." + deckId);
            request.Remove("Craft.secret_pack_list", "Craft.open_secret_pack");
        }

        void Act_DeckDeleteDeck(GameServerWebRequest request)
        {
            int deckId = Utils.GetValue<int>(request.ActParams, "deck_id");
            DeckInfo deck;
            if (request.Player.Decks.TryGetValue(deckId, out deck))
            {
                DeleteDeck(deck);
                request.Player.Decks.Remove(deckId);
                WriteDeckRemoval(request, deckId);
                SavePlayer(request.Player);
            }
            else
            {
                WriteDeckRemoval(request, deckId, true);
            }
        }

        void Act_DeckDeleteDeckMulti(GameServerWebRequest request)
        {
            List<int> deckIdList = Utils.GetIntList(request.ActParams, "deck_id_list");
            foreach (int deckId in deckIdList)
            {
                DeckInfo deck;
                if (request.Player.Decks.TryGetValue(deckId, out deck))
                {
                    DeleteDeck(deck);
                    request.Player.Decks.Remove(deckId);
                    WriteDeckRemoval(request, deckId);
                }
                else
                {
                    WriteDeckRemoval(request, deckId, true);
                }
            }
            SavePlayer(request.Player);
        }

        void Act_DeckUpdate(GameServerWebRequest request)
        {
            DeckInfo deck = new DeckInfo();
            deck.Id = Utils.GetValue(request.ActParams, "deck_id", 0);
            deck.Name = Utils.GetValue(request.ActParams, "name", "Deck");
            deck.RegulationId = Utils.GetValue(request.ActParams, "regulation_id", DeckInfo.DefaultRegulationId);
            deck.FixupRegulation();
            Dictionary<string, object> accessory = Utils.GetDictionary(request.ActParams, "accessory");
            deck.Accessory.FromDictionary(accessory);
            Dictionary<string, object> displayCards = Utils.GetDictionary(request.ActParams, "pick_cards");
            if (displayCards != null)
            {
                deck.DisplayCards.FromIndexedDictionary(displayCards);
            }
            Dictionary<string, object> deckListObj = Utils.GetDictionary(request.ActParams, "deck_list");
            if (deckListObj != null)
            {
                deck.FromDictionary(deckListObj);
            }
            if (deck.Id == 0)
            {
                while ((deck.Id = request.Player.NextDeckUId++) == DeckInfo.TradeDeckId)
                {
                }
                request.Player.Decks[deck.Id] = deck;
                deck.TimeCreated = Utils.GetEpochTime();
                deck.TimeEdited = Utils.GetEpochTime();
                deck.GetNewFilePath(GetDecksDirectory(request.Player));
            }
            else
            {
                DeckInfo existingDeck;
                if (!request.Player.Decks.TryGetValue(deck.Id, out existingDeck))
                {
                    WriteDeckRemoval(request, deck.Id, true);
                    return;
                }
                deck.File = existingDeck.File;
                deck.TimeCreated = existingDeck.TimeCreated;
                deck.TimeEdited = existingDeck.TimeEdited;
                request.Player.Decks[deck.Id] = deck;
                if (deck.Name != existingDeck.Name)
                {
                    DeleteDeck(deck);
                    deck.GetNewFilePath(GetDecksDirectory(request.Player));
                }
                if (accessory == null)
                {
                    deck.Accessory = existingDeck.Accessory;
                }
                if (displayCards == null)
                {
                    deck.DisplayCards.CopyFrom(existingDeck.DisplayCards);
                }
            }
            deck.Accessory.Sanitize(request.Player);
            if (!DontUpdateDeckEditTime)
            {
                deck.TimeEdited = Utils.GetEpochTime();
            }
            SortDecks(request.Player, request);
            SaveDeck(deck);
            WriteDeck(request, deck.Id);
            WriteDeckList(request, deck.Id);
            SavePlayer(request.Player);
        }

        void Act_DeckSetDeckAccessory(GameServerWebRequest request)
        {
            int deckId = Utils.GetValue<int>(request.ActParams, "deck_id");
            DeckInfo deck;
            if (!request.Player.Decks.TryGetValue(deckId, out deck))
            {
                WriteDeckRemoval(request, deck.Id, true);
                return;
            }
            Dictionary<string, object> args = Utils.GetValue(request.ActParams, "param", default(Dictionary<string, object>));
            if (args != null)
            {
                int value;
                if (Utils.TryGetValue(args, "deck_case", out value))
                {
                    if (!request.Player.Items.Contains(value))
                    {
                        foreach (int id in ItemID.Values[ItemID.Category.DECK_CASE])
                        {
                            if (request.Player.Items.Contains(id))
                            {
                                value = id;
                            }
                        }
                    }
                    deck.Accessory.Box = value;
                    if (DeckListByBoxThenAlphabetical)
                    {
                        SortDecks(request.Player, request);
                    }
                }
                if (Utils.TryGetValue(args, "protector", out value))
                {
                    deck.Accessory.Sleeve = value;
                }
                if (Utils.TryGetValue(args, "field", out value))
                {
                    deck.Accessory.Field = value;
                }
                if (Utils.TryGetValue(args, "field_obj", out value))
                {
                    deck.Accessory.FieldObj = value;
                }
                if (Utils.TryGetValue(args, "field_avatar_base", out value))
                {
                    deck.Accessory.AvBase = value;
                }
                if (Utils.TryGetValue(args, "avatar_id", out value))
                {
                    deck.Accessory.AvatarId = value;
                }
                if (Utils.TryGetValue(args, "coin", out value))
                {
                    deck.Accessory.Coin = value;
                }
                deck.Accessory.Sanitize(request.Player);
                Dictionary<string, object> pickCards = Utils.GetDictionary(args, "pick_cards");
                if (pickCards != null)
                {
                    deck.DisplayCards.FromIndexedDictionary(pickCards);
                }
                SaveDeck(deck);
            }
            WriteDeck(request, deckId);
        }

        void Act_DeckSetFavoriteCards(GameServerWebRequest request)
        {
            request.Player.CardFavorites.FromDictionary(Utils.GetDictionary(request.ActParams, "card_list"));
            SavePlayer(request.Player);
            WriteCards_favorite(request);
        }

        void Act_DeckSetLockCards(GameServerWebRequest request)
        {
            request.Player.CardLock.FromDictionary(Utils.GetDictionary(request.ActParams, "card_list"));
            SavePlayer(request.Player);
            WriteCards_lock(request);
        }

        void Act_SetSelectDeck(GameServerWebRequest request)
        {
            GameMode mode;
            int deckId;
            DeckInfo deck;
            if (Utils.TryGetValue(request.ActParams, "mode", out mode) &&
                Utils.TryGetValue(request.ActParams, "deck_id", out deckId) &&
                request.Player.Decks.TryGetValue(deckId, out deck))
            {
                request.Player.Duel.SetDeckId(mode, deckId);
            }
            else
            {
                request.Player.Duel.SetDeckId(mode, 0);
            }
            SavePlayer(request.Player);
        }

        void Act_DeckCopyStructureMulti(GameServerWebRequest request)
        {
            int numCreatedDecks = 0;
            List<int> structureIds = Utils.GetIntList(request.ActParams, "structure_ids");
            if (structureIds != null)
            {
                foreach (int structureId in structureIds)
                {
                    DeckInfo deckInfo;
                    if (StructureDecks.TryGetValue(structureId, out deckInfo) && request.Player.Items.Contains(structureId))
                    {
                        numCreatedDecks++;
                        request.ActParams["deck_id"] = 0;
                        request.ActParams["name"] = deckInfo.Name;
                        request.ActParams["deck_list"] = deckInfo.ToDictionary();
                        request.ActParams["pick_cards"] = deckInfo.DisplayCards.ToIndexDictionary();
                        request.ActParams["accessory"] = deckInfo.Accessory.ToDictionary();
                        Act_DeckUpdate(request);
                    }
                }
            }
            if (numCreatedDecks == 0)
            {
                request.ResultCode = (int)ResultCodes.DeckCode.ERROR_DECK_LIMIT;
            }
        }

        void Act_DeckSetAccessory(GameServerWebRequest request)
        {
            Dictionary<string, object> args = Utils.GetValue(request.ActParams, "param", default(Dictionary<string, object>));
            if (args != null)
            {
                int value;
                if (Utils.TryGetValue(args, "icon_id", out value) && request.Player.Items.Contains(value))
                {
                    request.Player.IconId = value;
                }
                if (Utils.TryGetValue(args, "icon_frame_id", out value) && request.Player.Items.Contains(value))
                {
                    request.Player.IconFrameId = value;
                }
                if (Utils.TryGetValue(args, "avatar_id", out value) && request.Player.Items.Contains(value))
                {
                    request.Player.AvatarId = value;
                }
                if (Utils.TryGetValue(args, "wallpaper", out value) && request.Player.Items.Contains(value))
                {
                    request.Player.Wallpaper = value;
                }
                ItemID.Category fieldItemCategory = ItemID.Category.NONE;
                if (Utils.TryGetValue(args, "field_avatar_base", out value) && request.Player.Items.Contains(value))
                {
                    fieldItemCategory = ItemID.Category.AVATAR_HOME;
                }
                if (Utils.TryGetValue(args, "field_obj", out value) && request.Player.Items.Contains(value))
                {
                    fieldItemCategory = ItemID.Category.FIELD_OBJ;
                }
                if (Utils.TryGetValue(args, "field", out value) && request.Player.Items.Contains(value))
                {
                    fieldItemCategory = ItemID.Category.FIELD;
                }
                if (Utils.TryGetValue(args, "deck_case", out value) && request.Player.Items.Contains(value))
                {
                    fieldItemCategory = ItemID.Category.DECK_CASE;
                }
                if (Utils.TryGetValue(args, "protector", out value) && request.Player.Items.Contains(value))
                {
                    fieldItemCategory = ItemID.Category.PROTECTOR;
                }
                if (Utils.TryGetValue(args, "coin", out value) && request.Player.Items.Contains(value))
                {
                    fieldItemCategory = ItemID.Category.COIN;
                }
                if (fieldItemCategory != ItemID.Category.NONE)
                {
                    // NOTE: Deck changes should be applied to the "Ranked Duels Deck"
                    // NOTE: Currently the client isn't showing the copy prompt with this code, probably because the client knows there's no assigned ranked deck
                    DeckInfo deck = request.Player.Duel.GetDeck(GameMode.SoloSingle);
                    if (deck != null)
                    {
                        switch (fieldItemCategory)
                        {
                            case ItemID.Category.AVATAR_HOME:
                                deck.Accessory.AvBase = value;
                                break;
                            case ItemID.Category.FIELD_OBJ:
                                deck.Accessory.FieldObj = value;
                                break;
                            case ItemID.Category.FIELD:
                                deck.Accessory.Field = value;
                                break;
                            case ItemID.Category.DECK_CASE:
                                deck.Accessory.Box = value;
                                if (DeckListByBoxThenAlphabetical)
                                {
                                    SortDecks(request.Player, request);
                                }
                                break;
                            case ItemID.Category.PROTECTOR:
                                deck.Accessory.Sleeve = value;
                                break;
                            case ItemID.Category.COIN:
                                deck.Accessory.Coin = value;
                                break;
                        }
                        SaveDeck(deck);
                    }
                    else
                    {
                        // TODO: Give error?
                    }
                }
                SavePlayer(request.Player);
                WriteUser(request);
            }
        }
    }
}

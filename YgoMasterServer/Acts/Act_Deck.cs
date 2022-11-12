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
                deck.Id = request.Player.NextDeckUId++;
                request.Player.Decks[deck.Id] = deck;
                deck.TimeCreated = Utils.GetEpochTime();
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
            deck.TimeEdited = Utils.GetEpochTime();
            SaveDeck(deck);
            WriteDeck(request, deck.Id);
            WriteDeckList(request, deck.Id);
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
                    if (!request.Player.Items.Contains(value) && !UnlockAllItems)
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
    }
}
